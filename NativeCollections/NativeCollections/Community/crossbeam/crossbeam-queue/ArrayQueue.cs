using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using static NativeCollections.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Contains the implementation of a bounded multi-producer multi-consumer queue.
    /// </summary>
    /// <remarks>
    ///     The implementation is based on Dmitry Vyukov's bounded MPMC queue.
    ///     <para>
    ///         Source:
    ///         <see href="http://www.1024cores.net/home/lock-free-algorithms/queues/bounded-mpmc-queue">
    ///             Dmitry Vyukov's bounded MPMC queue
    ///         </see>
    ///         .
    ///     </para>
    ///     <para>
    ///         The queue implementation is based on
    ///         <see href="https://github.com/crossbeam-rs/crossbeam">Crossbeam</see>.
    ///     </para>
    /// </remarks>
    internal static unsafe class ArrayQueue
    {
        /// <summary>
        ///     Returns a pointer to the element at the specified index without performing bounds checking.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="array">The array to access.</param>
        /// <param name="index">The zero-based index of the element to access.</param>
        /// <returns>A pointer to the element at <paramref name="index" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T* GetUnchecked<T>(this NativeArray<T> array, nuint index) where T : unmanaged => UnsafeHelpers.Add<T>(array.Buffer, Isize.FromUsizeUnchecked(index));

        /// <summary>
        ///     A slot in a queue.
        /// </summary>
        /// <typeparam name="T">The type of the value stored in the slot.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        private struct Slot<T> where T : unmanaged
        {
            /// <summary>
            ///     The current stamp.
            /// </summary>
            /// <remarks>
            ///     If the stamp equals the tail, this slot is the next slot to be written to.
            ///     If it equals <c>head + 1</c>, this slot is the next slot to be read from.
            /// </remarks>
            public UnsafeAtomicUsize Stamp;

            /// <summary>
            ///     The value in this slot.
            /// </summary>
            public T Value;
        }

        /// <summary>
        ///     A bounded multi-producer multi-consumer queue.
        /// </summary>
        /// <typeparam name="T">The type of elements stored in the queue.</typeparam>
        /// <remarks>
        ///     The queue allocates a fixed-capacity buffer when constructed. The
        ///     buffer determines the maximum number of elements the queue can hold.
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 <see cref="TryEnqueue" /> returns <see langword="false" /> when the queue is full.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 <see cref="Enqueue" /> replaces the oldest element when the
        ///                 queue is full and returns it through its output parameter.
        ///             </description>
        ///         </item>
        ///     </list>
        ///     The preallocated buffer makes this queue somewhat faster than <see cref="SegQueue" />.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, Size = 3 * CACHE_LINE_SIZE)]
        [InternalCollection(FromType.Community | FromType.Rust)]
        public struct RawArrayQueue<T> : IIsCreated, IDisposable, IEquatable<RawArrayQueue<T>> where T : unmanaged
        {
            /// <summary>
            ///     The head of the queue.
            /// </summary>
            /// <remarks>
            ///     This value is a stamp consisting of an index into the buffer and a lap, but packed
            ///     into a single <see cref="UIntPtr" />. The lower bits represent the index, while the
            ///     upper bits represent the lap. Elements are removed from the beginning of the queue.
            /// </remarks>
            private CachePaddedAtomicUsize _head;

            /// <summary>
            ///     The tail of the queue.
            /// </summary>
            /// <remarks>
            ///     This value is a stamp consisting of an index into the buffer and a lap, but packed
            ///     into a single <see cref="UIntPtr" />. The lower bits represent the index, while
            ///     the upper bits represent the lap. Elements are added to the end of the queue.
            /// </remarks>
            private CachePaddedAtomicUsize _tail;

            /// <summary>
            ///     The buffer holding slots.
            /// </summary>
            private readonly NativeArray<Slot<T>> _buffer;

            /// <summary>
            ///     A stamp with the value of <c>{ lap: 1, index: 0 }</c>.
            /// </summary>
            private readonly nuint _oneLap;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public readonly bool IsCreated => _buffer.IsCreated;

            /// <summary>
            ///     Indicates whether the current object is equal to another object.
            /// </summary>
            /// <param name="other">The value to compare with this instance.</param>
            /// <returns>
            ///     <see langword="true" /> if this instance is equal to
            ///     the other object; otherwise, <see langword="false" />.
            /// </returns>
            public readonly bool Equals(RawArrayQueue<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

            /// <summary>
            ///     Indicates whether the current object is equal to another object.
            /// </summary>
            /// <param name="obj">The object to compare with this instance.</param>
            /// <returns>
            ///     <see langword="true" /> if this instance is equal to
            ///     the other object; otherwise, <see langword="false" />.
            /// </returns>
            public readonly override bool Equals(object? obj) => obj is RawArrayQueue<T> other && other.Equals(this);

            /// <summary>
            ///     Returns the hash code for this instance.
            /// </summary>
            /// <returns>The hash code for this instance.</returns>
            public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

            /// <summary>
            ///     Returns the string representation of this instance.
            /// </summary>
            /// <returns>The string representation of this instance.</returns>
            public readonly override string ToString() => SR.Format("RawArrayQueue<{0}>", SR.GetTypeName(typeof(T)));

            /// <summary>
            ///     Indicates whether the current object is equal to another object.
            /// </summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>
            ///     <see langword="true" /> if this instance is equal to
            ///     the other object; otherwise, <see langword="false" />.
            /// </returns>
            public static bool operator ==(RawArrayQueue<T> left, RawArrayQueue<T> right) => left.Equals(right);

            /// <summary>
            ///     Indicates whether the current object is not equal to another object.
            /// </summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>
            ///     <see langword="true" /> if this instance is not equal to
            ///     the other object; otherwise, <see langword="false" />.
            /// </returns>
            public static bool operator !=(RawArrayQueue<T> left, RawArrayQueue<T> right) => !left.Equals(right);

            /// <summary>
            ///     Initializes a new instance of this class with the specified state.
            /// </summary>
            /// <param name="head">The stamp of the head of the queue.</param>
            /// <param name="tail">The stamp of the tail of the queue.</param>
            /// <param name="buffer">The buffer holding slots.</param>
            /// <param name="oneLap">A stamp with the value of <c>{ lap: 1, index: 0 }</c>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private RawArrayQueue(CachePaddedAtomicUsize head, CachePaddedAtomicUsize tail, NativeArray<Slot<T>> buffer, nuint oneLap)
            {
                _head = head;
                _tail = tail;
                _buffer = buffer;
                _oneLap = oneLap;
            }

            /// <summary>
            ///     Creates a new bounded queue with the given capacity.
            /// </summary>
            /// <param name="capacity">The requested queue capacity.</param>
            /// <returns>A newly allocated bounded queue.</returns>
            public static RawArrayQueue<T> Create(nuint capacity)
            {
                CachePaddedAtomicUsize head = default;
                CachePaddedAtomicUsize tail = default;
                var buffer = NativeArray<Slot<T>>.Create(I32.FromUsizeUnchecked(capacity));
                for (var i = Usize.FromI32Unchecked(0); i < capacity; i = i.WrappingAdd(Usize.FromI32Unchecked(1)))
                    buffer.GetUnchecked(i)->Stamp = UnsafeAtomicUsize.Create(i);

                var oneLap = Usize.FromU32Unchecked(BitOperationsHelpers.RoundUpToPowerOf2(U32.FromUsizeUnchecked(capacity.WrappingAdd(Usize.FromI32Unchecked(1)))));
                return new RawArrayQueue<T>(head, tail, buffer, oneLap);
            }

            /// <summary>
            ///     Attempts to add an element to the end of the queue.
            /// </summary>
            /// <param name="value">The element to add to the queue.</param>
            /// <returns>
            ///     <see langword="true" /> if the element was enqueued;
            ///     otherwise, <see langword="false" /> if the queue is full.
            /// </returns>
            public bool TryEnqueue(T value)
            {
                Backoff backoff = default;
                var tail = _tail.Load(Ordering.Relaxed);

                while (true)
                {
                    var index = tail & _oneLap.WrappingSub(Usize.FromI32Unchecked(1));
                    var lap = tail & ~_oneLap.WrappingSub(Usize.FromI32Unchecked(1));

                    var newTail = index.WrappingAdd(Usize.FromI32Unchecked(1)) < Capacity() ? tail.WrappingAdd(Usize.FromI32Unchecked(1)) : lap.WrappingAdd(_oneLap);

                    var slot = _buffer.GetUnchecked(index);
                    var stamp = slot->Stamp.Load(Ordering.Acquire);

                    if (tail == stamp)
                    {
                        var t = _tail.CompareExchange(newTail, tail);
                        if (t == tail)
                        {
                            slot->Value = value;
                            slot->Stamp.Store(tail.WrappingAdd(Usize.FromI32Unchecked(1)), Ordering.Release);
                            return true;
                        }

                        tail = t;
                        backoff.Spin();
                    }
                    else if (stamp.WrappingAdd(_oneLap) == tail.WrappingAdd(Usize.FromI32Unchecked(1)))
                    {
                        Interlocked.MemoryBarrier();

                        var head = _head.Load(Ordering.Relaxed);
                        if (head.WrappingAdd(_oneLap) == tail)
                            return false;

                        backoff.Spin();
                        tail = _tail.Load(Ordering.Relaxed);
                    }
                    else
                    {
                        backoff.Snooze();
                        tail = _tail.Load(Ordering.Relaxed);
                    }
                }
            }

            /// <summary>
            ///     Attempts to add an element using exclusive mutable access to the queue.
            /// </summary>
            /// <param name="value">The element to add to the queue.</param>
            /// <param name="overwritten">
            ///     When this method returns, contains the default value
            ///     because this operation does not overwrite an element.
            /// </param>
            /// <returns>
            ///     <see cref="InsertResult.Success" /> if the element was inserted; otherwise,
            ///     <see cref="InsertResult.InsufficientCapacity" /> if the queue is full.
            /// </returns>
            /// <remarks>
            ///     Atomic operations are omitted. The capacity check is retained, but the
            ///     caller must ensure that no other thread accesses the queue concurrently.
            /// </remarks>
            public InsertResult EnqueueMut(T value, out T overwritten)
            {
                var tail = _tail.AsRef();
                var head = _head.AsRef();

                if (head.WrappingAdd(_oneLap) == tail)
                {
                    overwritten = default;
                    return InsertResult.InsufficientCapacity;
                }

                var index = tail & _oneLap.WrappingSub(Usize.FromI32Unchecked(1));
                var lap = tail & ~_oneLap.WrappingSub(Usize.FromI32Unchecked(1));

                var newTail = index.WrappingAdd(Usize.FromI32Unchecked(1)) < Capacity() ? tail.WrappingAdd(Usize.FromI32Unchecked(1)) : lap.WrappingAdd(_oneLap);

                _tail.AsRef() = newTail;

                var slot = _buffer.GetUnchecked(index);
                slot->Value = value;
                slot->Stamp.AsRef() = tail.WrappingAdd(Usize.FromI32Unchecked(1));

                overwritten = default;
                return InsertResult.Success;
            }

            /// <summary>
            ///     Adds an element to the end of the queue, replacing the oldest element if necessary.
            /// </summary>
            /// <param name="value">The element to add to the queue.</param>
            /// <param name="overwritten">
            ///     When this method returns <see cref="InsertResult.Overwritten" />, contains
            ///     the element that was replaced; otherwise, contains the default value.
            /// </param>
            /// <returns>
            ///     <see cref="InsertResult.Success" /> if no element was replaced; otherwise,
            ///     <see cref="InsertResult.Overwritten" /> if the oldest element was replaced.
            /// </returns>
            public InsertResult Enqueue(T value, out T overwritten)
            {
                Backoff backoff = default;
                var tail = _tail.Load(Ordering.Relaxed);

                while (true)
                {
                    var index = tail & _oneLap.WrappingSub(Usize.FromI32Unchecked(1));
                    var lap = tail & ~_oneLap.WrappingSub(Usize.FromI32Unchecked(1));

                    var newTail = index.WrappingAdd(Usize.FromI32Unchecked(1)) < Capacity() ? tail.WrappingAdd(Usize.FromI32Unchecked(1)) : lap.WrappingAdd(_oneLap);

                    var slot = _buffer.GetUnchecked(index);
                    var stamp = slot->Stamp.Load(Ordering.Acquire);

                    if (tail == stamp)
                    {
                        var t = _tail.CompareExchange(newTail, tail);
                        if (t == tail)
                        {
                            slot->Value = value;
                            slot->Stamp.Store(tail.WrappingAdd(Usize.FromI32Unchecked(1)), Ordering.Release);

                            overwritten = default;
                            return InsertResult.Success;
                        }

                        tail = t;
                        backoff.Spin();
                    }
                    else if (stamp.WrappingAdd(_oneLap) == tail.WrappingAdd(Usize.FromI32Unchecked(1)))
                    {
                        Interlocked.MemoryBarrier();

                        var head = tail.WrappingSub(_oneLap);
                        var newHead = newTail.WrappingSub(_oneLap);

                        if (_head.CompareExchange(newHead, head) == head)
                        {
                            _tail.Store(newTail, Ordering.SeqCst);

                            var old = slot->Value;
                            slot->Value = value;
                            slot->Stamp.Store(tail.WrappingAdd(Usize.FromI32Unchecked(1)), Ordering.Release);

                            overwritten = old;
                            return InsertResult.Overwritten;
                        }

                        backoff.Spin();
                        tail = _tail.Load(Ordering.Relaxed);
                    }
                    else
                    {
                        backoff.Snooze();
                        tail = _tail.Load(Ordering.Relaxed);
                    }
                }
            }

            /// <summary>
            ///     Attempts to remove an element from the beginning of the queue.
            /// </summary>
            /// <param name="result">
            ///     When this method returns <see langword="true" />, contains the removed
            ///     element; otherwise, contains the default value of <typeparamref name="T" />.
            /// </param>
            /// <returns>
            ///     <see langword="true" /> if an element was removed;
            ///     otherwise, <see langword="false" /> if the queue is empty.
            /// </returns>
            public bool TryDequeue(out T result)
            {
                Backoff backoff = default;
                var head = _head.Load(Ordering.Relaxed);
                while (true)
                {
                    var index = head & _oneLap.WrappingSub(Usize.FromI32Unchecked(1));
                    var lap = head & ~_oneLap.WrappingSub(Usize.FromI32Unchecked(1));
                    var slot = _buffer.GetUnchecked(index);

                    var stamp = slot->Stamp.Load(Ordering.Acquire);
                    if (head.WrappingAdd(Usize.FromI32Unchecked(1)) == stamp)
                    {
                        var @new = index.WrappingAdd(Usize.FromI32Unchecked(1)) < Capacity() ? head.WrappingAdd(Usize.FromI32Unchecked(1)) : lap.WrappingAdd(_oneLap);

                        var h = _head.CompareExchange(@new, head);
                        if (h == head)
                        {
                            var msg = slot->Value;
                            slot->Stamp.Store(head.WrappingAdd(_oneLap), Ordering.Release);

                            result = msg;
                            return true;
                        }

                        head = h;
                        backoff.Spin();
                    }
                    else if (stamp == head)
                    {
                        Interlocked.MemoryBarrier();

                        var tail = _tail.Load(Ordering.Relaxed);
                        if (tail == head)
                        {
                            result = default;
                            return false;
                        }

                        backoff.Spin();
                        head = _head.Load(Ordering.Relaxed);
                    }
                    else
                    {
                        backoff.Snooze();
                        head = _head.Load(Ordering.Relaxed);
                    }
                }
            }

            /// <summary>
            ///     Attempts to remove an element using exclusive mutable access to the queue.
            /// </summary>
            /// <param name="result">
            ///     When this method returns <see langword="true" />, contains the removed
            ///     element; otherwise, contains the default value of <typeparamref name="T" />.
            /// </param>
            /// <returns>
            ///     <see langword="true" /> if an element was removed;
            ///     otherwise, <see langword="false" /> if the queue is empty.
            /// </returns>
            /// <remarks>
            ///     Atomic operations are omitted. The empty check is retained, but the
            ///     caller must ensure that no other thread accesses the queue concurrently.
            /// </remarks>
            public bool TryDequeueMut(out T result)
            {
                var head = _head.AsRef();
                var tail = _tail.AsRef();

                if (tail == head)
                {
                    result = default;
                    return false;
                }

                var index = head & _oneLap.WrappingSub(Usize.FromI32Unchecked(1));
                var lap = head & ~_oneLap.WrappingSub(Usize.FromI32Unchecked(1));

                var @new = index.WrappingAdd(Usize.FromI32Unchecked(1)) < Capacity() ? head.WrappingAdd(Usize.FromI32Unchecked(1)) : lap.WrappingAdd(_oneLap);

                var slot = _buffer.GetUnchecked(index);
                var msg = slot->Value;
                slot->Stamp.AsRef() = head.WrappingAdd(_oneLap);

                _head.AsRef() = @new;
                result = msg;
                return true;
            }

            /// <summary>
            ///     Returns the capacity of the queue.
            /// </summary>
            /// <returns>The maximum number of elements the queue can hold.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly nuint Capacity() => Usize.FromI32Unchecked(_buffer.Length);

            /// <summary>
            ///     Determines whether the queue is empty.
            /// </summary>
            /// <returns><see langword="true" /> if the queue is empty; otherwise, <see langword="false" />.</returns>
            /// <remarks>
            ///     The result may change as other threads modify the queue. Use this
            ///     method when only the presence or absence of elements is needed.
            /// </remarks>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public bool IsEmpty()
            {
                var head = _head.Load(Ordering.SeqCst);
                var tail = _tail.Load(Ordering.SeqCst);
                return tail == head;
            }

            /// <summary>
            ///     Determines whether the queue is full.
            /// </summary>
            /// <returns><see langword="true" /> if the queue is full; otherwise, <see langword="false" />.</returns>
            /// <remarks>
            ///     The result may change as other threads modify the queue.
            /// </remarks>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public bool IsFull()
            {
                var tail = _tail.Load(Ordering.SeqCst);
                var head = _head.Load(Ordering.SeqCst);
                return head.WrappingAdd(_oneLap) == tail;
            }

            /// <summary>
            ///     Returns the number of elements contained in the queue.
            /// </summary>
            /// <returns>The number of elements contained in the queue.</returns>
            /// <remarks>
            ///     The result may change as other threads modify the queue. Use
            ///     <see cref="IsEmpty" /> when only the presence or absence of elements is needed.
            /// </remarks>
            public nuint Count()
            {
                while (true)
                {
                    var tail = _tail.Load(Ordering.SeqCst);
                    var head = _head.Load(Ordering.SeqCst);
                    if (_tail.Load(Ordering.SeqCst) == tail)
                    {
                        var hix = head & _oneLap.WrappingSub(Usize.FromI32Unchecked(1));
                        var tix = tail & _oneLap.WrappingSub(Usize.FromI32Unchecked(1));

                        if (hix < tix)
                            return tix.WrappingSub(hix);

                        if (hix > tix)
                            return Capacity().WrappingSub(hix).WrappingAdd(tix);

                        if (tail == head)
                            return Usize.FromI32Unchecked(0);

                        return Capacity();
                    }
                }
            }

            /// <summary>
            ///     Releases the resources used by the queue.
            /// </summary>
            public readonly void Dispose() => _buffer.Dispose();
        }
    }
}