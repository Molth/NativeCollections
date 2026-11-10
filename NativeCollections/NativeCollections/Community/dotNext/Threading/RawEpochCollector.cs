using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     An epoch-based memory reclamation (EBR) collector. Implements a lock-free
    ///     garbage collection mechanism for concurrent data structures. This
    ///     implementation uses three epochs (0, 1, 2) and per-slot bags of retired objects.
    /// </summary>
    /// <remarks>
    ///     https://github.com/dotnet/dotNext/blob/master/src/DotNext/Threading/Epoch.cs
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [InternalCollection(FromType.Community)]
    internal unsafe struct RawEpochCollector : IIsCreated, IDisposable, IEquatable<RawEpochCollector>
    {
        /// <summary>
        ///     The global epoch counter, atomically updated and padded to avoid false sharing.
        /// </summary>
        private CachePaddedAtomicU32 _globalEpoch;

        /// <summary>
        ///     The three slot structures, one for each epoch (0, 1, 2).
        /// </summary>
        private Slots _slots;

        /// <summary>
        ///     Number of unpinnings after which a participant will
        ///     execute some deferred functions from the previous epoch slot.
        /// </summary>
        private const uint UNPINNINGS_BETWEEN_COLLECT = 64U;

        /// <summary>
        ///     Mask to check when unpinning count reaches a multiple of <see cref="UNPINNINGS_BETWEEN_COLLECT" />.
        /// </summary>
        private const uint UNPINNINGS_BETWEEN_COLLECT_MASK = UNPINNINGS_BETWEEN_COLLECT - 1U;

        /// <summary>
        ///     Maximum number of deferred actions that can be collected in a single batch.
        /// </summary>
        private const uint COLLECT_BATCH_SIZE = 16U;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Always <see langword="true" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The default value of this type is a valid empty collector: the epoch slots start
        ///                 zeroed, so there is no separate initialization step.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public readonly bool IsCreated => true;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(RawEpochCollector other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is RawEpochCollector other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "RawEpochCollector";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(RawEpochCollector left, RawEpochCollector right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(RawEpochCollector left, RawEpochCollector right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            for (var i = 0U; i < 3U; i = i.WrappingAdd(1U))
                _slots.GetUnchecked(i).Dispose();
        }

        /// <summary>
        ///     Enters the current epoch and returns a disposable scope that automatically exits the epoch on disposal.
        /// </summary>
        /// <returns>A disposable scope representing the pinned epoch.</returns>
        /// <remarks>
        ///     This method is thread-safe. The returned scope must be disposed to unpin the epoch.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeEpochCollectorScope EnterScope() => NativeEpochCollectorScope.From(UnsafeHelpers.AsPointer(ref this), Pin());

        /// <summary>
        ///     Enters the current epoch and returns a disposable scope that automatically exits the epoch on disposal.
        /// </summary>
        /// <returns>A disposable scope representing the pinned epoch.</returns>
        /// <remarks>
        ///     This method is thread-safe. The returned scope must be disposed to unpin the epoch.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeEpochCollectorRefScope EnterRefScope() => NativeEpochCollectorRefScope.From(NativeRef<RawEpochCollector>.From(ref this), Pin());

        /// <summary>
        ///     Enters the current epoch and returns the epoch identifier.
        /// </summary>
        /// <returns>The current epoch number that the caller is pinned to.</returns>
        /// <remarks>
        ///     This method is thread-safe and must be paired with a call to <see cref="Unpin" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Pin()
        {
            UnsafeSpinWait spinWait = default;
            while (true)
            {
                var globalEpoch = _globalEpoch.Load(Ordering.Acquire);
                var current = globalEpoch % 3U;

                ref var pinCount = ref _slots.GetUnchecked(current).PinCount;
                pinCount.Add(1U);

                if (globalEpoch == _globalEpoch.Load(Ordering.Acquire))
                    return globalEpoch;

                pinCount.Sub(1U);
                spinWait.SpinOnce(-1);
            }
        }

        /// <summary>
        ///     Exits the pinned epoch and triggers garbage collection if necessary.
        /// </summary>
        /// <param name="epoch">The epoch value returned from a prior call to <see cref="Pin" />.</param>
        /// <remarks>
        ///     This method decrements the pin counter for the specified epoch. The unpinning
        ///     counter is incremented by one per call, and a collection attempt is made whenever
        ///     the resulting count reaches a multiple of <see cref="UNPINNINGS_BETWEEN_COLLECT" />
        ///     (that is, on the 64th, 128th, 192nd unpinning of a slot).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpin(uint epoch)
        {
            var current = epoch % 3U;

            ref var slot = ref _slots.GetUnchecked(current);
            slot.PinCount.Sub(1U);

            if ((slot.UnpinCount.Add(1U) & UNPINNINGS_BETWEEN_COLLECT_MASK) == 0U)
                Collect();
        }

        /// <summary>
        ///     Collects several bags from the previous epoch slot and executes deferred functions in them.
        /// </summary>
        /// <remarks>
        ///     This method attempts to advance the global epoch when the previous epoch has no
        ///     active pins. It then processes all expired bags and executes their deferred actions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [SkipLocalsInit]
        private void Collect()
        {
            var globalEpoch = _globalEpoch.Load(Ordering.Acquire);
            var current = globalEpoch % 3U;

            var previous = current == 0U ? 2U : current.WrappingSub(1U);
            ref var slot = ref _slots.GetUnchecked(previous);
            if (slot.PinCount.Load(Ordering.Acquire) == 0U && _globalEpoch.CompareExchange(globalEpoch.WrappingAdd(1U), globalEpoch) == globalEpoch)
            {
                Span<Deferred> garbage = stackalloc Deferred[I32.FromU32Unchecked(COLLECT_BATCH_SIZE)];
                var count = 0;
                while (slot.Bag.TryDequeue(out var sealedBag))
                {
                    if (sealedBag.IsExpired(globalEpoch))
                    {
                        garbage[(count = count.WrappingAdd(1)).WrappingSub(1)] = sealedBag.Deferred;
                        if (I64.FromI32Unchecked(count) == I64.FromU32Unchecked(COLLECT_BATCH_SIZE))
                        {
                            for (var i = 0; I64.FromI32Unchecked(i) < I64.FromU32Unchecked(COLLECT_BATCH_SIZE); i = i.WrappingAdd(1))
                                garbage[i].Call();

                            count = 0;
                        }
                    }
                    else
                    {
                        slot.Bag.Enqueue(sealedBag);
                        break;
                    }
                }

                for (var i = 0; i < count; i = i.WrappingAdd(1))
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
        /// <param name="call">
        ///     A function pointer that deallocates the memory pointed to by <paramref name="data" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="call" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     This method is thread-safe and does not block the caller. The deallocation
        ///     callback will be invoked exactly once after the epoch advances.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Retire(uint epoch, void* data, delegate* managed<void*, void> call)
        {
            ThrowHelpers.ThrowIfNull(call, ExceptionArgument.call);
            var current = epoch % 3U;

            ref var slot = ref _slots.GetUnchecked(current);
            var sealedBag = SealedBag.From(epoch, data, call);
            slot.Bag.Enqueue(sealedBag);
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
            ///     Initializes a new instance of this class with the specified resource pointer and deallocation function.
            /// </summary>
            /// <param name="data">The resource pointer.</param>
            /// <param name="call">The deallocation function.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Deferred(void* data, delegate* managed<void*, void> call)
            {
                _call = call;
                _data = data;
            }

            /// <summary>
            ///     Creates a new deferred action.
            /// </summary>
            /// <param name="data">The resource pointer.</param>
            /// <param name="call">The deallocation function.</param>
            /// <returns>A new deferred action.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Deferred From(void* data, delegate* managed<void*, void> call) => new(data, call);

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
            private readonly uint _epoch;

            /// <summary>
            ///     The deferred action to be executed.
            /// </summary>
            public readonly Deferred Deferred;

            /// <summary>
            ///     Initializes a new instance of this class with the specified epoch, resource pointer, and deallocation function.
            /// </summary>
            /// <param name="epoch">The current epoch.</param>
            /// <param name="data">The resource pointer.</param>
            /// <param name="call">The deallocation function.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private SealedBag(uint epoch, void* data, delegate* managed<void*, void> call)
            {
                _epoch = epoch;
                Deferred = Deferred.From(data, call);
            }

            /// <summary>
            ///     Creates a new sealed bag.
            /// </summary>
            /// <param name="epoch">The current epoch.</param>
            /// <param name="data">The resource pointer.</param>
            /// <param name="call">The deallocation function.</param>
            /// <returns>A new sealed bag.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static SealedBag From(uint epoch, void* data, delegate* managed<void*, void> call) => new(epoch, data, call);

            /// <summary>
            ///     Checks if it is safe to drop the bag w.r.t. the given global epoch.
            /// </summary>
            /// <param name="epoch">The old global epoch.</param>
            /// <returns>
            ///     <see langword="true" /> if the bag has expired and
            ///     can be collected; otherwise, <see langword="false" />.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsExpired(uint epoch) => epoch.WrappingSub(_epoch) >= 2U;
        }

        /// <summary>
        ///     Container for the three slots (one per epoch).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Slots
        {
            /// <summary>
            ///     Epoch 0.
            /// </summary>
            private Slot _slot0;

            /// <summary>
            ///     Epoch 1.
            /// </summary>
            private Slot _slot1;

            /// <summary>
            ///     Epoch 2.
            /// </summary>
            private Slot _slot2;

            /// <summary>
            ///     Retrieves a reference to the slot of the specified epoch index without validating the index.
            /// </summary>
            /// <param name="index">The epoch index (must be 0, 1, or 2).</param>
            /// <returns>A reference to the corresponding slot.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref Slot GetUnchecked(uint index) => ref Unsafe.Add(ref _slot0, Isize.FromU32Unchecked(index));
        }

        /// <summary>
        ///     A single slot containing pin count, unpin count, and the
        ///     bag of retired objects. Padded to avoid false sharing.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
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
            public SegQueue.RawSegQueue<SealedBag> Bag;

            /// <summary>
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                while (Bag.TryDequeueMut(out var sealedBag))
                    sealedBag.Deferred.Call();

                Bag.Dispose();
            }
        }
    }
}