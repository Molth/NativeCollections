using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     A fixed-capacity work-stealing deque with one owner and multiple concurrent stealers.
    /// </summary>
    /// <typeparam name="T">The unmanaged value stored inline in each slot.</typeparam>
    /// <remarks>
    ///     <para>
    ///         Only one thread at a time may act as the owner and call <see cref="TryPush" /> or
    ///         <see cref="TryPop" />. Synchronize any transfer of ownership between threads.
    ///         The owner removes the newest queued item. Concurrent <see cref="Stealer" /> views remove
    ///         the oldest queued item and may retry internally when they race to claim it.
    ///     </para>
    ///     <para>
    ///         <see cref="Capacity" /> is the exact number of items the deque can hold. The backing ring
    ///         rounds the slot count up to a power of two and stays fixed after creation.
    ///         Queue operations allocate no memory. When <see cref="TryPush" /> returns
    ///         <see langword="false" />, the caller still owns the item and can retry or submit it
    ///         to a shared queue.
    ///     </para>
    ///     <para>
    ///         The deque, its copies, and its stealer views share a single native allocation.
    ///         Publish the initialized handles to other threads with synchronization. Call
    ///         <see cref="Dispose" /> exactly once after all users have finished, then stop using
    ///         every copy and view. <see cref="IsCreated" /> checks the handle value and cannot
    ///         detect disposal through another copy.
    ///     </para>
    ///     <para>
    ///         This deque is intended for workers that usually process their own tasks and steal
    ///         when idle. When other threads consume most of the items, especially small task
    ///         handles, <see cref="NativeArrayQueue{T}" /> can have higher throughput.
    ///     </para>
    ///     <para>
    ///         The index protocol follows the LIFO variant of
    ///         <see href="https://docs.rs/crossbeam-deque/latest/src/crossbeam_deque/deque.rs.html">
    ///             Crossbeam's Chase–Lev
    ///             deque
    ///         </see>
    ///         .
    ///         A stealer copies the slot in atomic chunks and returns the copy only after successfully
    ///         claiming its index. A failed claim discards the copy because the slot may have been reused.
    ///     </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Community | FromType.Rust)]
    public readonly unsafe struct NativeWorkStealingDeque<T> : IIsCreated, IDisposable, IEquatable<NativeWorkStealingDeque<T>> where T : unmanaged
    {
        /// <summary>
        ///     The shared native control block.
        /// </summary>
        private readonly State* _state;

        /// <summary>
        ///     Wraps the native control block.
        /// </summary>
        /// <param name="state">The initialized control block.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeWorkStealingDeque(State* state) => _state = state;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _state != null;

        /// <summary>
        ///     Gets the fixed maximum number of queued items.
        /// </summary>
        public int Capacity => I32.FromUsizeUnchecked(_state->Capacity);

        /// <summary>
        ///     Creates a deque that can hold exactly <paramref name="capacity" /> values.
        /// </summary>
        /// <param name="capacity">The maximum number of items, which must be positive.</param>
        /// <returns>The owner of the deque's native allocation.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than or equal to zero.
        /// </exception>
        /// <exception cref="OverflowException">The allocation size does not fit in a native-sized integer.</exception>
        /// <exception cref="OutOfMemoryException">Native allocation failed.</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeWorkStealingDeque<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(capacity, ExceptionArgument.capacity);
            var slotCount = Usize.FromI32Unchecked(capacity);
            var ringSize = Usize.FromU32Unchecked(BitOperationsHelpers.RoundUpToPowerOf2(U32.FromI32Unchecked(capacity)));
            var alignment = Usize.FromI32Unchecked(CACHE_LINE_SIZE);
            var offset = Usize.FromI32Unchecked(Unsafe.SizeOf<State>());
            var byteCount = offset + ringSize * GetSlotStride();
            var state = (State*)NativeMemoryAllocator.AlignedAlloc(byteCount, alignment);
            Unsafe.AsRef<State>(state) = default;
            state->Capacity = slotCount;
            state->PushLimit = slotCount;
            state->Mask = ringSize.WrappingSub(Usize.FromI32Unchecked(1));
            return new NativeWorkStealingDeque<T>(state);
        }

        /// <summary>
        ///     Creates a non-owning view for taking items from the front.
        /// </summary>
        /// <returns>A view that can be copied to multiple stealing threads.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Stealer GetStealer() => Stealer.From(_state);

        /// <summary>
        ///     Appends an item at the owner's end.
        /// </summary>
        /// <param name="item">The value to copy into the deque.</param>
        /// <returns>Whether the item was inserted. A full deque leaves the item with the caller.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPush(T item)
        {
            var state = _state;
            var back = state->Back.Load(Ordering.Relaxed);
            var position = Usize.FromU64Unchecked(back);
            if (position == state->PushLimit)
            {
                var front = Usize.FromU64Unchecked(state->Front.Index.Load(Ordering.Acquire));
                var limit = front.WrappingAdd(state->Capacity);
                if (position == limit)
                    return false;

                state->PushLimit = limit;
            }

            WriteSlot(ref GetSlot(state, position), ref item);
            state->Back.Store(back.WrappingAdd(1UL), Ordering.Release);
            return true;
        }

        /// <summary>
        ///     Removes the most recently pushed item still queued.
        /// </summary>
        /// <param name="item">The removed value, or default if the deque was empty.</param>
        /// <returns>Whether an item was removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T item)
        {
            var state = _state;
            var back = state->Back.Load(Ordering.Relaxed);
            var front = state->Front.Index.Load(Ordering.Relaxed);
            var count = back.WrappingSub(front);
            if (count == 0UL)
            {
                item = default;
                return false;
            }

            if (count == 1UL)
            {
                if (state->Front.Index.CompareExchange(back, front) == front)
                {
                    item = Unsafe.ReadUnaligned<T>(ref GetSlot(state, Usize.FromU64Unchecked(front)));
                    return true;
                }

                item = default;
                return false;
            }

            var index = back.WrappingSub(1UL);
            state->Back.Store(index, Ordering.Relaxed);
            Interlocked.MemoryBarrier();
            front = state->Front.Index.Load(Ordering.Relaxed);
            var distance = index.WrappingSub(front);
            if (IsPositiveDistance(distance))
            {
                item = Unsafe.ReadUnaligned<T>(ref GetSlot(state, Usize.FromU64Unchecked(index)));
                return true;
            }

            var claimed = distance == 0UL && state->Front.Index.CompareExchange(back, front) == front;
            state->Back.Store(back, Ordering.Release);
            item = claimed ? Unsafe.ReadUnaligned<T>(ref GetSlot(state, Usize.FromU64Unchecked(index))) : default;
            return claimed;
        }

        /// <summary>
        ///     Releases the deque's storage. Finish all owner and stealer operations first.
        /// </summary>
        /// <remarks>A default value has no allocation to release.</remarks>
        public void Dispose() => NativeMemoryAllocator.AlignedFree(_state);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeWorkStealingDeque<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeWorkStealingDeque<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeWorkStealingDeque<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeWorkStealingDeque<T> left, NativeWorkStealingDeque<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeWorkStealingDeque<T> left, NativeWorkStealingDeque<T> right) => !left.Equals(right);

        /// <summary>
        ///     Tests whether back is ahead of front in the modular index sequence.
        ///     A delayed stealer can observe a distance greater than the capacity and still
        ///     needs to attempt the CAS so it can refresh its front snapshot.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPositiveDistance(ulong distance) => distance.WrappingSub(1UL) < ulong.MaxValue >> 1;

        /// <summary>
        ///     The slot width rounded up to the alignment of the largest chunk used for this value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint GetSlotStride()
        {
            var alignment = Unsafe.SizeOf<T>() >= Unsafe.SizeOf<nuint>() ? Unsafe.SizeOf<nuint>() : Unsafe.SizeOf<T>() >= Unsafe.SizeOf<uint>() ? Unsafe.SizeOf<uint>() : Unsafe.SizeOf<T>() >= Unsafe.SizeOf<ushort>() ? Unsafe.SizeOf<ushort>() : Unsafe.SizeOf<byte>();
            var alignmentMask = Usize.FromI32Unchecked(alignment.WrappingSub(1));
            return Usize.FromI32Unchecked(Unsafe.SizeOf<T>()).WrappingAdd(alignmentMask) & ~alignmentMask;
        }

        /// <summary>
        ///     Maps the native-width part of an index to its aligned ring slot.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref byte GetSlot(State* state, nuint position)
        {
            var slot = position & state->Mask;
            var offset = Usize.FromI32Unchecked(Unsafe.SizeOf<State>()).WrappingAdd(slot.WrappingMul(GetSlotStride()));
            return ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(state), Isize.FromUsizeUnchecked(offset));
        }

        /// <summary>
        ///     Stores one aligned native word from the unaligned source.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteWord(ref byte slot, ref byte source, nint offset) => Volatile.Write(ref Unsafe.As<byte, nuint>(ref Unsafe.Add(ref slot, offset)), Unsafe.ReadUnaligned<nuint>(ref Unsafe.Add(ref source, offset)));

        /// <summary>
        ///     Copies one aligned native word to the unaligned destination.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadWord(ref byte slot, ref byte destination, nint offset) => Unsafe.WriteUnaligned(ref Unsafe.Add(ref destination, offset), Volatile.Read(ref Unsafe.As<byte, nuint>(ref Unsafe.Add(ref slot, offset))));

        /// <summary>
        ///     Publishes each chunk with a naturally aligned atomic store.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteSlot(ref byte slot, ref T item)
        {
            ref var source = ref Unsafe.As<T, byte>(ref item);
            var wordCount = Unsafe.SizeOf<T>() / Unsafe.SizeOf<nuint>();
            if (wordCount <= 5)
            {
                if (wordCount > 0)
                    WriteWord(ref slot, ref source, Isize.FromI32Unchecked(0));

                if (wordCount > 1)
                    WriteWord(ref slot, ref source, Isize.FromI32Unchecked(Unsafe.SizeOf<nuint>()));

                if (wordCount > 2)
                    WriteWord(ref slot, ref source, Isize.FromI32Unchecked(Unsafe.SizeOf<nuint>() * 2));

                if (wordCount > 3)
                    WriteWord(ref slot, ref source, Isize.FromI32Unchecked(Unsafe.SizeOf<nuint>() * 3));

                if (wordCount > 4)
                    WriteWord(ref slot, ref source, Isize.FromI32Unchecked(Unsafe.SizeOf<nuint>() * 4));
            }
            else
            {
                for (var i = 0; i < wordCount; i = i.WrappingAdd(1))
                    WriteWord(ref slot, ref source, Isize.FromI32Unchecked(i.WrappingMul(Unsafe.SizeOf<nuint>())));
            }

            var offset = wordCount.WrappingMul(Unsafe.SizeOf<nuint>());
            if (Environment.Is64BitProcess && (Unsafe.SizeOf<T>() & 4) != 0)
            {
                Volatile.Write(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref slot, Isize.FromI32Unchecked(offset))), Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref source, Isize.FromI32Unchecked(offset))));
                offset = offset.WrappingAdd(4);
            }

            if ((Unsafe.SizeOf<T>() & 2) != 0)
            {
                Volatile.Write(ref Unsafe.As<byte, ushort>(ref Unsafe.Add(ref slot, Isize.FromI32Unchecked(offset))), Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref source, Isize.FromI32Unchecked(offset))));
                offset = offset.WrappingAdd(2);
            }

            if ((Unsafe.SizeOf<T>() & 1) != 0)
                Volatile.Write(ref Unsafe.Add(ref slot, Isize.FromI32Unchecked(offset)), Unsafe.Add(ref source, Isize.FromI32Unchecked(offset)));
        }

        /// <summary>
        ///     Copies atomic chunks to private storage before the caller confirms ownership.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReadSlot(ref byte slot, out T item)
        {
            Unsafe.SkipInit(out item);
            ref var destination = ref Unsafe.As<T, byte>(ref item);
            var wordCount = Unsafe.SizeOf<T>() / Unsafe.SizeOf<nuint>();
            if (wordCount <= 5)
            {
                if (wordCount > 0)
                    ReadWord(ref slot, ref destination, Isize.FromI32Unchecked(0));

                if (wordCount > 1)
                    ReadWord(ref slot, ref destination, Isize.FromI32Unchecked(Unsafe.SizeOf<nuint>()));

                if (wordCount > 2)
                    ReadWord(ref slot, ref destination, Isize.FromI32Unchecked(Unsafe.SizeOf<nuint>() * 2));

                if (wordCount > 3)
                    ReadWord(ref slot, ref destination, Isize.FromI32Unchecked(Unsafe.SizeOf<nuint>() * 3));

                if (wordCount > 4)
                    ReadWord(ref slot, ref destination, Isize.FromI32Unchecked(Unsafe.SizeOf<nuint>() * 4));
            }
            else
            {
                for (var i = 0; i < wordCount; i = i.WrappingAdd(1))
                    ReadWord(ref slot, ref destination, Isize.FromI32Unchecked(i.WrappingMul(Unsafe.SizeOf<nuint>())));
            }

            var offset = wordCount.WrappingMul(Unsafe.SizeOf<nuint>());
            if (Environment.Is64BitProcess && (Unsafe.SizeOf<T>() & 4) != 0)
            {
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destination, Isize.FromI32Unchecked(offset)), Volatile.Read(ref Unsafe.As<byte, uint>(ref Unsafe.Add(ref slot, Isize.FromI32Unchecked(offset)))));
                offset = offset.WrappingAdd(4);
            }

            if ((Unsafe.SizeOf<T>() & 2) != 0)
            {
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destination, Isize.FromI32Unchecked(offset)), Volatile.Read(ref Unsafe.As<byte, ushort>(ref Unsafe.Add(ref slot, Isize.FromI32Unchecked(offset)))));
                offset = offset.WrappingAdd(2);
            }

            if ((Unsafe.SizeOf<T>() & 1) != 0)
                Unsafe.Add(ref destination, Isize.FromI32Unchecked(offset)) = Volatile.Read(ref Unsafe.Add(ref slot, Isize.FromI32Unchecked(offset)));
        }

        /// <summary>
        ///     A copyable, non-owning view of a live deque's stealing end.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Stealer : IIsCreated, IEquatable<Stealer>
        {
            /// <summary>
            ///     The shared control block owned by the deque.
            /// </summary>
            private readonly State* _state;

            /// <summary>
            ///     Wraps the native control block without transferring ownership.
            /// </summary>
            /// <param name="state">The deque's control block.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Stealer(State* state) => _state = state;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _state != null;

            /// <summary>
            ///     Creates a stealing view over the specified native address.
            /// </summary>
            /// <param name="state">The address of the deque's control block.</param>
            /// <returns>The view that shares the control block.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Stealer From(State* state) => new(state);

            /// <summary>
            ///     Removes the oldest queued item. Multiple stealers may call this concurrently.
            /// </summary>
            /// <param name="item">The removed value, or default if the deque was empty.</param>
            /// <returns>Whether an item was removed.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TrySteal(out T item)
            {
                var state = _state;
                Backoff backoff = default;
                var front = state->Front.Index.Load(Ordering.Acquire);
                Interlocked.MemoryBarrier();
                while (true)
                {
                    var back = state->Back.Load(Ordering.Acquire);
                    if (!IsPositiveDistance(back.WrappingSub(front)))
                    {
                        item = default;
                        return false;
                    }

                    ReadSlot(ref GetSlot(state, Usize.FromU64Unchecked(front)), out var candidate);
                    var observed = state->Front.Index.CompareExchange(front.WrappingAdd(1UL), front);
                    if (observed == front)
                    {
                        item = candidate;
                        return true;
                    }

                    front = observed;
                    backoff.Spin();
                }
            }

            /// <summary>
            ///     Indicates whether the current object is equal to another object.
            /// </summary>
            /// <param name="other">The value to compare with this instance.</param>
            /// <returns>
            ///     <see langword="true" /> if this instance is equal to
            ///     the other object; otherwise, <see langword="false" />.
            /// </returns>
            public bool Equals(Stealer other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

            /// <summary>
            ///     Indicates whether the current object is equal to another object.
            /// </summary>
            /// <param name="obj">The object to compare with this instance.</param>
            /// <returns>
            ///     <see langword="true" /> if this instance is equal to
            ///     the other object; otherwise, <see langword="false" />.
            /// </returns>
            public override bool Equals(object? obj) => obj is Stealer other && other.Equals(this);

            /// <summary>
            ///     Returns the hash code for this instance.
            /// </summary>
            /// <returns>The hash code for this instance.</returns>
            public override int GetHashCode() => NativeHashCode.GetHashCode(this);

            /// <summary>
            ///     Returns the string representation of this instance.
            /// </summary>
            /// <returns>The string representation of this instance.</returns>
            public override string ToString() => "Stealer";

            /// <summary>
            ///     Indicates whether the current object is equal to another object.
            /// </summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>
            ///     <see langword="true" /> if this instance is equal to
            ///     the other object; otherwise, <see langword="false" />.
            /// </returns>
            public static bool operator ==(Stealer left, Stealer right) => left.Equals(right);

            /// <summary>
            ///     Indicates whether the current object is not equal to another object.
            /// </summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>
            ///     <see langword="true" /> if this instance is not equal to
            ///     the other object; otherwise, <see langword="false" />.
            /// </returns>
            public static bool operator !=(Stealer left, Stealer right) => !left.Equals(right);
        }

        /// <summary>
        ///     Places the contended front index on a separate cache line.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        internal struct FrontState
        {
            /// <summary>
            ///     The logical index of the oldest item. Keeping a 64-bit CAS operand on x86 prevents
            ///     a paused stealer's snapshot from matching again after only 2^32 advances.
            /// </summary>
            public UnsafeAtomicU64 Index;
        }

        /// <summary>
        ///     The two published indices and immutable ring configuration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct State
        {
            /// <summary>
            ///     The index that stealers advance with CAS.
            /// </summary>
            public FrontState Front;

            /// <summary>
            ///     The next logical insertion index in the same 64-bit sequence as front.
            /// </summary>
            public UnsafeAtomicU64 Back;

            /// <summary>
            ///     The native-width part of the owner's cached exclusive back limit.
            /// </summary>
            public nuint PushLimit;

            /// <summary>
            ///     The maximum number of queued items.
            /// </summary>
            public nuint Capacity;

            /// <summary>
            ///     The mask of the physical ring.
            /// </summary>
            public nuint Mask;
        }
    }
}