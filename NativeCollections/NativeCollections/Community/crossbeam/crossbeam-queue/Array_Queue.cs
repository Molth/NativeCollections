using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NativeCollections;
using static NativeCollections.PaddingHelpers;
using static crossbeam.Option;
using static crossbeam.Result;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable All

namespace crossbeam
{
    //! The implementation is based on Dmitry Vyukov's bounded MPMC queue.
    //!
    //! Source:
    //!   - <http://www.1024cores.net/home/lock-free-algorithms/queues/bounded-mpmc-queue>

    /// <summary>
    ///     A bounded multi-producer multi-consumer queue.
    /// </summary>
    /// <remarks>
    ///     https://github.com/crossbeam-rs/crossbeam
    /// </remarks>
    internal static unsafe class Array_Queue
    {
        /// A slot in a queue.
        [StructLayout(LayoutKind.Sequential)]
        private struct Slot<T> where T : unmanaged
        {
            /// The current stamp.
            /// <br />
            /// If the stamp equals the tail, this node will be next written to. If it equals head + 1,
            /// this node will be next read from.
            public UnsafeAtomicUsize stamp;

            /// The value in this slot.
            public T value;
        }

        /// A bounded multi-producer multi-consumer queue.
        /// <br />
        /// This queue allocates a fixed-capacity buffer on construction, which is used to store pushed
        /// elements. The queue cannot hold more elements than the buffer allows. Attempting to push an
        /// element into a full queue will fail. Alternatively, [`force_push`] makes it possible for
        /// this queue to be used as a ring-buffer. Having a buffer allocated upfront makes this queue
        /// a bit faster than [`SegQueue`].`
        [StructLayout(LayoutKind.Sequential, Size = 4 * CACHE_LINE_SIZE)]
        public struct ArrayQueue<T> : IIsCreated where T : unmanaged
        {
            private readonly CachePadding _padding;

            /// The head of the queue.
            /// <br />
            /// This value is a "stamp" consisting of an index into the buffer and a lap, but packed into a
            /// single `usize`. The lower bits represent the index, while the upper bits represent the lap.
            /// <br />
            /// Elements are popped from the head of the queue.
            private CachePaddedAtomicUsize head;

            /// The tail of the queue.
            /// <br />
            /// This value is a "stamp" consisting of an index into the buffer and a lap, but packed into a
            /// single `usize`. The lower bits represent the index, while the upper bits represent the lap.
            /// <br />
            /// Elements are pushed into the tail of the queue.
            private CachePaddedAtomicUsize tail;

            /// The buffer holding slots.
            private readonly NativeArray<Slot<T>> buffer;

            /// A stamp with the value of `{ lap: 1, index: 0 }`.
            private readonly nuint one_lap;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public readonly bool IsCreated => buffer.IsCreated;

            /// Creates a new bounded queue with the given capacity.
            public ArrayQueue(nuint cap)
            {
                _padding = new CachePadding();

                // Head is initialized to `{ lap: 0, index: 0 }`.
                // Tail is initialized to `{ lap: 0, index: 0 }`.
                head = new CachePaddedAtomicUsize();
                tail = new CachePaddedAtomicUsize();

                // Allocate a buffer of `cap` slots initialized
                // with stamps.

                buffer = new NativeArray<Slot<T>>((int)cap);
                for (nuint i = 0; i < cap; ++i)
                {
                    // Set the stamp to `{ lap: 0, index: i }`.
                    buffer.get_unchecked(i)->stamp = new UnsafeAtomicUsize(i);
                }

                // One lap is the smallest power of two greater than `cap`.
                one_lap = (nuint)BitOperationsHelpers.RoundUpToPowerOf2((uint)(cap + 1));
            }

            private Result<T> push_or_else(T value, delegate* managed<ref ArrayQueue<T>, T, nuint, nuint, Slot<T>*, Result<T>> f)
            {
                var backoff = new Backoff();
                var tail = this.tail.load(Ordering.Relaxed);

                while (true)
                {
                    // Deconstruct the tail.
                    var index = tail & (this.one_lap - 1);
                    var lap = tail & ~(this.one_lap - 1);

                    nuint new_tail;
                    if (index + 1 < this.capacity())
                    {
                        // Same lap, incremented index.
                        // Set to `{ lap: lap, index: index + 1 }`.
                        new_tail = tail + 1;
                    }
                    else
                    {
                        // One lap forward, index wraps around to zero.
                        // Set to `{ lap: lap.wrapping_add(1), index: 0 }`.
                        new_tail = lap.wrapping_add(this.one_lap);
                    }

                    // Inspect the corresponding slot.
                    Debug.Assert(index < (nuint)this.buffer.Length);
                    var slot = this.buffer.get_unchecked(index);
                    var stamp = slot->stamp.load(Ordering.Acquire);

                    // If the tail and the stamp match, we may attempt to push.
                    if (tail == stamp)
                    {
                        // Try moving the tail.
                        var t = this.tail.compare_exchange(tail, new_tail);
                        if (t == tail)
                        {
                            // Write the value into the slot and update the stamp.
                            slot->value = value;
                            slot->stamp.store(tail + 1, Ordering.Release);
                            return Ok<T>(default);
                        }
                        else
                        {
                            tail = t;
                            backoff.spin();
                        }
                    }
                    else if (stamp.wrapping_add(this.one_lap) == tail + 1)
                    {
                        Interlocked.MemoryBarrier();
                        var result = f(ref this, value, tail, new_tail, slot);
                        if (result.is_err())
                            return result;
                        value = result.unwrap_unchecked();
                        backoff.spin();
                        tail = this.tail.load(Ordering.Relaxed);
                    }
                    else
                    {
                        // Snooze because we need to wait for the stamp to get updated.
                        backoff.snooze();
                        tail = this.tail.load(Ordering.Relaxed);
                    }
                }
            }

            /// Attempts to push an element into the queue.
            /// <br />
            /// If the queue is full, the element is returned back as an error.
            public Result<T> push(T value)
            {
                return this.push_or_else(value, &f);

                static Result<T> f(ref ArrayQueue<T> self, T v, nuint tail, nuint _, Slot<T>* __)
                {
                    var head = self.head.load(Ordering.Relaxed);

                    // If the head lags one lap behind the tail as well...
                    if (head.wrapping_add(self.one_lap) == tail)
                    {
                        // ...then the queue is full.
                        return Err(v);
                    }
                    else
                    {
                        return Ok(v);
                    }
                }
            }

            /// Attempts to push an element using an exclusive reference of the queue.
            /// <br />
            /// Atomic operations and checks are omitted
            public Result<T> push_mut(T value)
            {
                var tail = this.tail.get_mut();
                var head = this.head.get_mut();

                if (head.wrapping_add(this.one_lap) == tail)
                {
                    return Err(value);
                }

                var index = tail & (this.one_lap - 1);
                var lap = tail & ~(this.one_lap - 1);
                nuint new_tail;
                if (index + 1 < this.capacity())
                {
                    new_tail = tail + 1;
                }
                else
                {
                    new_tail = lap.wrapping_add(this.one_lap);
                }

                this.tail.get_mut() = new_tail;

                var slot = this.buffer.get_unchecked(index);
                slot->value = value;
                slot->stamp.get_mut() = tail + 1;

                return Ok<T>(default);
            }

            /// Pushes an element into the queue, replacing the oldest element if necessary.
            /// <br />
            /// If the queue is full, the oldest element is replaced and returned,
            /// otherwise `None` is returned.
            public Option<T> force_push(T value)
            {
                return this.push_or_else(value, &f).err();

                static Result<T> f(ref ArrayQueue<T> self, T v, nuint tail, nuint new_tail, Slot<T>* slot)
                {
                    var head = tail.wrapping_sub(self.one_lap);
                    var new_head = new_tail.wrapping_sub(self.one_lap);

                    // Try moving the head.
                    if (self
                            .head
                            .compare_exchange(head, new_head)
                        == head)
                    {
                        // Move the tail.
                        self.tail.store(new_tail, Ordering.SeqCst);

                        // Swap the previous value.
                        var old = slot->value;
                        slot->value = v;

                        // Update the stamp.
                        slot->stamp.store(tail + 1, Ordering.Release);

                        return Err(old);
                    }
                    else
                    {
                        return Ok(v);
                    }
                }
            }

            /// Attempts to pop an element from the queue.
            /// <br />
            /// If the queue is empty, `None` is returned.
            public Option<T> pop()
            {
                var backoff = new Backoff();
                var head = this.head.load(Ordering.Relaxed);

                while (true)
                {
                    // Deconstruct the head.
                    var index = head & (this.one_lap - 1);
                    var lap = head & ~(this.one_lap - 1);

                    // Inspect the corresponding slot.
                    Debug.Assert(index < (nuint)this.buffer.Length);
                    var slot = this.buffer.get_unchecked(index);
                    var stamp = slot->stamp.load(Ordering.Acquire);

                    // If the stamp is ahead of the head by 1, we may attempt to pop.
                    if (head + 1 == stamp)
                    {
                        nuint @new;
                        if (index + 1 < this.capacity())
                        {
                            // Same lap, incremented index.
                            // Set to `{ lap: lap, index: index + 1 }`.
                            @new = head + 1;
                        }
                        else
                        {
                            // One lap forward, index wraps around to zero.
                            // Set to `{ lap: lap.wrapping_add(1), index: 0 }`.
                            @new = lap.wrapping_add(this.one_lap);
                        }

                        // Try moving the head.
                        var h = this.head.compare_exchange(head, @new);
                        if (h == head)
                        {
                            // Read the value from the slot and update the stamp.
                            var msg = slot->value;
                            slot->stamp
                                .store(head.wrapping_add(this.one_lap), Ordering.Release);
                            return Some(msg);
                        }
                        else
                        {
                            head = h;
                            backoff.spin();
                        }
                    }
                    else if (stamp == head)
                    {
                        Interlocked.MemoryBarrier();
                        var tail = this.tail.load(Ordering.Relaxed);

                        // If the tail equals the head, that means the channel is empty.
                        if (tail == head)
                        {
                            return None<T>();
                        }

                        backoff.spin();
                        head = this.head.load(Ordering.Relaxed);
                    }
                    else
                    {
                        // Snooze because we need to wait for the stamp to get updated.
                        backoff.snooze();
                        head = this.head.load(Ordering.Relaxed);
                    }
                }
            }

            /// Attempts to pop an element using an exclusive reference of the queue.
            /// <br />
            /// Due to having an exclusive reference, atomic operations and checks are omitted
            public Option<T> pop_mut()
            {
                var head = this.head.get_mut();
                var tail = this.tail.get_mut();

                // If the tail equals the head, that means the channel is empty.
                if (tail == head)
                {
                    return None<T>();
                }

                var index = head & (this.one_lap - 1);
                var lap = head & ~(this.one_lap - 1);

                // Inspect the corresponding slot.
                Debug.Assert(index < (nuint)this.buffer.Length);

                nuint @new;
                if (index + 1 < this.capacity())
                {
                    // Same lap, incremented index.
                    // Set to `{ lap: lap, index: index + 1 }`.
                    @new = head + 1;
                }
                else
                {
                    // One lap forward, index wraps around to zero.
                    // Set to `{ lap: lap.wrapping_add(1), index: 0 }`.
                    @new = lap.wrapping_add(this.one_lap);
                }

                var slot = this.buffer.get_unchecked(index);

                var msg = slot->value;
                slot->stamp.get_mut() = head.wrapping_add(this.one_lap);
                this.head.get_mut() = @new;
                return Some(msg);
            }

            /// Returns the capacity of the queue.
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly nuint capacity()
            {
                return (nuint)this.buffer.Length;
            }

            /// Returns `true` if the queue is empty.
            public bool is_empty()
            {
                var head = this.head.load(Ordering.SeqCst);
                var tail = this.tail.load(Ordering.SeqCst);

                // Is the tail lagging one lap behind head?
                // Is the tail equal to the head?
                //
                // Note: If the head changes just before we load the tail, that means there was a moment
                // when the channel was not empty, so it is safe to just return `false`.
                return tail == head;
            }

            /// Returns `true` if the queue is full.
            public bool is_full()
            {
                var tail = this.tail.load(Ordering.SeqCst);
                var head = this.head.load(Ordering.SeqCst);

                // Is the head lagging one lap behind tail?
                //
                // Note: If the tail changes just before we load the head, that means there was a moment
                // when the queue was not full, so it is safe to just return `false`.
                return head.wrapping_add(this.one_lap) == tail;
            }

            /// Returns the number of elements in the queue.
            public nuint len()
            {
                while (true)
                {
                    // Load the tail, then load the head.
                    var tail = this.tail.load(Ordering.SeqCst);
                    var head = this.head.load(Ordering.SeqCst);

                    // If the tail didn't change, we've got consistent values to work with.
                    if (this.tail.load(Ordering.SeqCst) == tail)
                    {
                        var hix = head & (this.one_lap - 1);
                        var tix = tail & (this.one_lap - 1);

                        if (hix < tix)
                        {
                            return tix - hix;
                        }
                        else if (hix > tix)
                        {
                            return this.capacity() - hix + tix;
                        }
                        else if (tail == head)
                        {
                            return 0;
                        }
                        else
                        {
                            return this.capacity();
                        }
                    }
                }
            }

            public readonly void drop()
            {
                buffer.Dispose();
            }
        }
    }
}