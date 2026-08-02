using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using crossbeam;
using static NativeCollections.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     An epoch-based memory reclamation (EBR) collector.
    ///     Implements a lock-free garbage collection mechanism for concurrent data structures.
    ///     This implementation uses three epochs (0, 1, 2) and per-slot bags of retired objects.
    /// </summary>
    /// <remarks>
    ///     https://github.com/dotnet/dotNext/blob/master/src/DotNext/Threading/Epoch.cs
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community)]
    public unsafe struct UnsafeEpochCollector : IIsCreated, IDisposable, IEquatable<UnsafeEpochCollector>
    {
        /// <summary>
        ///     Padding to avoid false sharing with adjacent data.
        /// </summary>
        private readonly CachePadding _padding;

        /// <summary>
        ///     The global epoch counter, atomically updated and padded to avoid false sharing.
        /// </summary>
        private CachePaddedAtomicU32 _globalEpoch;

        /// <summary>
        ///     The three slot structures, one for each epoch (0, 1, 2).
        /// </summary>
        private Slots _slots;

        /// <summary>
        ///     Number of unpinnings after which a participant will execute some deferred functions from the global queue.
        /// </summary>
        private const uint UNPINNINGS_BETWEEN_COLLECT = 64;

        /// <summary>
        ///     Mask to check when unpinning count reaches a multiple of <see cref="UNPINNINGS_BETWEEN_COLLECT" />.
        /// </summary>
        private const uint UNPINNINGS_BETWEEN_COLLECT_MASK = UNPINNINGS_BETWEEN_COLLECT - 1;

        /// <summary>
        ///     Maximum number of deferred actions that can be collected in a single batch.
        /// </summary>
        private const uint COLLECT_BATCH_SIZE = 16;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => true;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeEpochCollector other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeEpochCollector other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeEpochCollector";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeEpochCollector left, UnsafeEpochCollector right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeEpochCollector left, UnsafeEpochCollector right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            for (uint i = 0; i < 3; ++i)
                _slots[i].Dispose();
        }

        /// <summary>
        ///     Enters the current epoch and returns a disposable scope that automatically exits the epoch on disposal.
        /// </summary>
        /// <remarks>
        ///     This method is thread-safe. The returned scope must be disposed to unpin the epoch.
        /// </remarks>
        /// <returns>A disposable scope representing the pinned epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeEpochCollectorScope EnterScope() => new(UnsafeHelpers.AsPointer(ref this), Pin());

        /// <summary>
        ///     Enters the current epoch and returns a disposable scope that automatically exits the epoch on disposal.
        /// </summary>
        /// <remarks>
        ///     This method is thread-safe. The returned scope must be disposed to unpin the epoch.
        /// </remarks>
        /// <returns>A disposable scope representing the pinned epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeEpochCollectorRefScope EnterRefScope() => new(NativeRef<UnsafeEpochCollector>.Create(ref this), Pin());

        /// <summary>
        ///     Enters the current epoch and returns the epoch identifier.
        /// </summary>
        /// <remarks>
        ///     This method is thread-safe and must be paired with a call to <see cref="Unpin" />.
        /// </remarks>
        /// <returns>The current epoch number that the caller is pinned to.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Pin()
        {
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                var globalEpoch = _globalEpoch.load(Ordering.Acquire);
                var current = globalEpoch % 3;
                ref var pinCount = ref _slots[current].PinCount;
                pinCount.fetch_add(1);
                if (globalEpoch == _globalEpoch.load(Ordering.Acquire))
                    return globalEpoch;
                pinCount.fetch_sub(1);
                spinWait.SpinOnce(-1);
            }
        }

        /// <summary>
        ///     Exits the pinned epoch and triggers garbage collection if necessary.
        /// </summary>
        /// <param name="epoch">The epoch value returned from a prior call to <see cref="Pin" />.</param>
        /// <remarks>
        ///     This method decrements the pin counter for the specified epoch. If the unpinning count
        ///     reaches the threshold, a collection attempt is made to reclaim retired objects.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpin(uint epoch)
        {
            var current = epoch % 3;
            ref var slot = ref _slots[current];
            slot.PinCount.fetch_sub(1);
            if ((slot.UnpinCount.fetch_add(1).wrapping_add(1) & UNPINNINGS_BETWEEN_COLLECT_MASK) == 0)
                Collect();
        }

        /// <summary>
        ///     Collects several bags from the global queue and executes deferred functions in them.
        /// </summary>
        /// <remarks>
        ///     This method attempts to advance the global epoch when the previous epoch has no active pins.
        ///     It then processes all expired bags and executes their deferred actions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Collect()
        {
            var globalEpoch = _globalEpoch.load(Ordering.Acquire);
            var current = globalEpoch % 3;
            var previous = current == 0 ? 2 : current - 1;
            ref var slot = ref _slots[previous];
            if (slot.PinCount.load(Ordering.Acquire) == 0 && _globalEpoch.compare_exchange(globalEpoch, globalEpoch.wrapping_add(1)) == globalEpoch)
            {
                Span<Deferred> garbage = stackalloc Deferred[(int)COLLECT_BATCH_SIZE];
                var count = 0;
                Option<SealedBag> option;
                while ((option = slot.Bag.pop()).is_some())
                {
                    var sealedBag = option.unwrap_unchecked();
                    if (sealedBag.IsExpired(globalEpoch))
                    {
                        garbage[count++] = sealedBag.Waste;
                        if (count == COLLECT_BATCH_SIZE)
                        {
                            for (var i = 0; i < COLLECT_BATCH_SIZE; ++i)
                                garbage[i].Call();
                            count = 0;
                        }
                    }
                    else
                    {
                        slot.Bag.push(sealedBag);
                        break;
                    }
                }

                for (var i = 0; i < count; ++i)
                    garbage[i].Call();
            }
        }

        /// <summary>
        ///     Retires a pointer to be freed when it is safe to do so.
        /// </summary>
        /// <param name="epoch">The epoch value returned from <see cref="Pin" />.</param>
        /// <param name="data">The pointer to be freed.</param>
        /// <remarks>
        ///     The pointer will be deallocated using <see cref="NativeMemoryAllocator.AlignedFree" />.
        ///     This method is thread-safe and does not block the caller.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Retire(uint epoch, void* data) => Retire(epoch, data, &NativeMemoryAllocator.AlignedFree);

        /// <summary>
        ///     Retires a pointer to be freed using a custom deallocation function when it is safe to do so.
        /// </summary>
        /// <param name="epoch">The epoch value returned from <see cref="Pin" />.</param>
        /// <param name="data">The pointer to be freed.</param>
        /// <param name="call">A function pointer that deallocates the memory pointed to by <paramref name="data" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="call" /> is <see langword="null" />.</exception>
        /// <remarks>
        ///     This method is thread-safe and does not block the caller. The deallocation callback
        ///     will be invoked exactly once after the epoch advances.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Retire(uint epoch, void* data, delegate* managed<void*, void> call)
        {
            if (call == null)
                ThrowHelpers.ThrowArgumentNullException(ExceptionArgument.call);
            var current = epoch % 3;
            ref var slot = ref _slots[current];
            var sealedBag = new SealedBag(epoch, data, call);
            slot.Bag.push(sealedBag);
        }

        /// <summary>
        ///     Represents a single deferred action (pointer + deleter function).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Deferred
        {
            /// <summary>
            ///     The pointer to the resource to be freed.
            /// </summary>
            private readonly void* _data;

            /// <summary>
            ///     The function pointer that frees the resource.
            /// </summary>
            private readonly delegate* managed<void*, void> _call;

            /// <summary>
            ///     Initializes a new deferred action.
            /// </summary>
            /// <param name="data">The resource pointer.</param>
            /// <param name="call">The deallocation function.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Deferred(void* data, delegate* managed<void*, void> call)
            {
                _call = call;
                _data = data;
            }

            /// <summary>
            ///     Executes the deferred action by calling the deallocation function on the stored pointer.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Call() => _call(_data);
        }

        /// <summary>
        ///     A sealed bag containing a deferred action and the epoch at which it was enqueued.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct SealedBag
        {
            /// <summary>
            ///     The epoch at which this bag was created.
            /// </summary>
            public readonly uint Epoch;

            /// <summary>
            ///     The deferred action to be executed.
            /// </summary>
            public readonly Deferred Waste;

            /// <summary>
            ///     Initializes a new sealed bag.
            /// </summary>
            /// <param name="epoch">The current epoch.</param>
            /// <param name="data">The resource pointer.</param>
            /// <param name="call">The deallocation function.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SealedBag(uint epoch, void* data, delegate* managed<void*, void> call)
            {
                Epoch = epoch;
                Waste = new Deferred(data, call);
            }

            /// <summary>
            ///     Checks if it is safe to drop the bag w.r.t. the given global epoch.
            /// </summary>
            /// <param name="epoch">The old global epoch.</param>
            /// <returns><see langword="true" /> if the bag has expired and can be collected; otherwise, <see langword="false" />.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsExpired(uint epoch) => (int)epoch.wrapping_sub(Epoch) >= 1;
        }

        /// <summary>
        ///     Container for the three slots (one per epoch).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Slots
        {
            /// <summary>
            ///     Epoch 0
            /// </summary>
            private Slot _slot0;

            /// <summary>
            ///     Epoch 1
            /// </summary>
            private Slot _slot1;

            /// <summary>
            ///     Epoch 2
            /// </summary>
            private Slot _slot2;

            /// <summary>
            ///     Indexer to retrieve a slot by its epoch index (0, 1, or 2).
            /// </summary>
            /// <param name="index">The epoch index (must be 0, 1, or 2).</param>
            /// <returns>A reference to the corresponding slot.</returns>
            public ref Slot this[uint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref Unsafe.Add(ref _slot0, (nint)index);
            }
        }

        /// <summary>
        ///     A single slot containing pin count, unpin count, and the bag of retired objects.
        ///     Padded to avoid false sharing.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 5 * CACHE_LINE_SIZE)]
        private struct Slot : IDisposable
        {
            /// <summary>
            ///     The number of active pins on this epoch.
            /// </summary>
            public CachePaddedAtomicU32 PinCount;

            /// <summary>
            ///     The total number of unpins performed on this epoch (used to trigger collection).
            /// </summary>
            public CachePaddedAtomicU32 UnpinCount;

            /// <summary>
            ///     The bag (lock-free queue) of sealed bags pending collection for this epoch.
            /// </summary>
            public Seg_Queue.SegQueue<SealedBag> Bag;

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                Option<SealedBag> option;
                while ((option = Bag.pop_mut()).is_some())
                {
                    var sealedBag = option.unwrap_unchecked();
                    sealedBag.Waste.Call();
                }

                Bag.drop();
            }
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeEpochCollector Empty => default;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeEpochCollector Create() => new();
    }
}