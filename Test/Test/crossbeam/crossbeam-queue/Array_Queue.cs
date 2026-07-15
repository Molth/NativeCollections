using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NativeCollections;
using static crossbeam.PaddingHelpers;

#pragma warning disable CS0162 // Unreachable code detected
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace crossbeam
{
    //! The implementation is based on Dmitry Vyukov's bounded MPMC queue.
    //!
    //! Source:
    //!   - <http://www.1024cores.net/home/lock-free-algorithms/queues/bounded-mpmc-queue>

    /// <summary>
    ///     https://github.com/crossbeam-rs/crossbeam
    /// </summary>
    public static unsafe class Array_Queue
    {
        /// A slot in a queue.
        public struct Slot<T> where T : unmanaged
        {
            /// The current stamp.
            /// <br />
            /// If the stamp equals the tail, this node will be next written to. If it equals head + 1,
            /// this node will be next read from.
            public UnsafeAtomicUsize stamp;

            /// The value in this slot.
            public T value;
        }

        [StructLayout(LayoutKind.Sequential, Size = 2 * CACHE_LINE_SIZE)]
        public struct CachePaddedUsize
        {
            private CachePadding padding;

            public UnsafeAtomicUsize data;
        }

        /// A bounded multi-producer multi-consumer queue.
        /// <br />
        /// This queue allocates a fixed-capacity buffer on construction, which is used to store pushed
        /// elements. The queue cannot hold more elements than the buffer allows. Attempting to push an
        /// element into a full queue will fail. Alternatively, [`force_push`] makes it possible for
        /// this queue to be used as a ring-buffer. Having a buffer allocated upfront makes this queue
        /// a bit faster than [`SegQueue`].`
        public struct ArrayQueue<T> where T : unmanaged
        {
            /// The head of the queue.
            /// <br />
            /// This value is a "stamp" consisting of an index into the buffer and a lap, but packed into a
            /// single `usize`. The lower bits represent the index, while the upper bits represent the lap.
            /// <br />
            /// Elements are popped from the head of the queue.
            private CachePaddedUsize _head;

            public ref UnsafeAtomicUsize head => ref _head.data;

            /// The tail of the queue.
            /// <br />
            /// This value is a "stamp" consisting of an index into the buffer and a lap, but packed into a
            /// single `usize`. The lower bits represent the index, while the upper bits represent the lap.
            /// <br />
            /// Elements are pushed into the tail of the queue.
            private CachePaddedUsize _tail;

            public ref UnsafeAtomicUsize tail => ref _tail.data;

            /// The buffer holding slots.
            public NativeMemoryArray<Slot<T>> buffer;

            /// A stamp with the value of `{ lap: 1, index: 0 }`.
            public nuint one_lap;

            public ArrayQueue(nuint cap)
            {
                _head = new CachePaddedUsize();
                _tail = new CachePaddedUsize();
                buffer = new NativeMemoryArray<Slot<T>>((int)cap);
                for (nuint i = 0; i < cap; i++)
                    buffer[(int)i]->stamp = new UnsafeAtomicUsize(i);

                // One lap is the smallest power of two greater than `cap`.
                one_lap = BitOperations.RoundUpToPowerOf2(cap + 1);
            }

            public Result<T> push_or_else(T value, delegate* managed<ref ArrayQueue<T>, T, nuint, nuint, Slot<T>*, Result<T>> f)
            {
                var backoff = new UnsafeSpinWait();
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
                    var slot = this.buffer[(int)index];

                    var stamp = slot->stamp.load(Ordering.Acquire);

                    // If the tail and the stamp match, we may attempt to push.
                    if (tail == stamp)
                    {
                        // Try moving the tail.
                        var t = this.tail.CompareExchange(new_tail, tail);
                        if (t == tail)
                        {
                            // Write the value into the slot and update the stamp.
                            unsafe
                            {
                                slot->value = value;
                            }

                            slot->stamp.store(tail + 1, Ordering.Release);
                            return Result.Ok(default(T));
                        }
                        else
                        {
                            tail = t;
                            backoff.SpinOnce();
                        }
                    }
                    else if (stamp.wrapping_add(this.one_lap) == tail + 1)
                    {
                        Interlocked.MemoryBarrier();
                        var result = f(ref this, value, tail, new_tail, slot);
                        if (!result.Ok)
                            return result;
                        value = result.Value;
                        backoff.SpinOnce();
                        tail = this.tail.load(Ordering.Relaxed);
                    }
                    else
                    {
                        // Snooze because we need to wait for the stamp to get updated.
                        backoff.SpinOnce(-1);
                        tail = this.tail.load(Ordering.Relaxed);
                    }
                }
            }

            /// Attempts to push an element into the queue.
            /// <br />
            /// If the queue is full, the element is returned back as an error.
            public bool push(T value)
            {
                return this.push_or_else(value, &f).Ok;

                static Result<T> f(ref ArrayQueue<T> self, T v, nuint tail, nuint _, Slot<T>* __)
                {
                    var head = self.head.load(Ordering.Relaxed);

                    // If the head lags one lap behind the tail as well...
                    if (head.wrapping_add(self.one_lap) == tail)
                    {
                        // ...then the queue is full.
                        return Result.Err(v);
                    }
                    else
                    {
                        return Result.Ok(v);
                    }
                }
            }

            /// Attempts to push an element using an exclusive reference of the queue.
            /// <br />
            /// Atomic operations and checks are omitted
            public bool push_mut(T value)
            {
                var tail = this.tail.AsRef();
                var head = this.head.AsRef();

                if (head.wrapping_add(this.one_lap) == tail)
                {
                    return false;
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

                this.tail.AsRef() = new_tail;

                var slot = this.buffer[(int)index];
                slot->value = value;
                slot->stamp.AsRef() = tail + 1;

                return true;
            }

            /// Pushes an element into the queue, replacing the oldest element if necessary.
            /// <br />
            /// If the queue is full, the oldest element is replaced and returned,
            /// otherwise `None` is returned.
            public bool force_push(T value, out T old_value)
            {
                var result = this.push_or_else(value, &f);

                old_value = result.Value;
                return result.Ok;

                static Result<T> f(ref ArrayQueue<T> self, T v, nuint tail, nuint new_tail, Slot<T>* slot)
                {
                    var head = tail.wrapping_sub(self.one_lap);
                    var new_head = new_tail.wrapping_sub(self.one_lap);

                    // Try moving the head.
                    if (self
                            .head
                            .CompareExchange(new_head, head)
                        == head)
                    {
                        // Move the tail.
                        self.tail.store(new_tail, Ordering.SeqCst);

                        // Swap the previous value.
                        var old = slot->value;
                        slot->value = v;

                        // Update the stamp.
                        slot->stamp.store(tail + 1, Ordering.Release);

                        return Result.Err(old);
                    }
                    else
                    {
                        return Result.Ok(v);
                    }
                }
            }

            /// Attempts to pop an element from the queue.
            /// <br />
            /// If the queue is empty, `None` is returned.
            public bool pop(out T result)
            {
                var backoff = new UnsafeSpinWait();
                var head = this.head.load(Ordering.Relaxed);

                while (true)
                {
                    // Deconstruct the head.
                    var index = head & (this.one_lap - 1);
                    var lap = head & ~(this.one_lap - 1);

                    // Inspect the corresponding slot.
                    Debug.Assert(index < (nuint)this.buffer.Length);
                    var slot = this.buffer[(int)index];
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
                        var h = this.head.CompareExchange(@new, head);
                        if (h == head)
                        {
                            // Read the value from the slot and update the stamp.
                            result = slot->value;

                            slot->stamp
                                .store(head.wrapping_add(this.one_lap), Ordering.Release);

                            return true;
                        }
                        else
                        {
                            head = h;
                            backoff.SpinOnce();
                        }
                    }
                    else if (stamp == head)
                    {
                        Interlocked.MemoryBarrier();
                        var tail = this.tail.load(Ordering.Relaxed);

                        // If the tail equals the head, that means the channel is empty.
                        if (tail == head)
                        {
                            result = default;
                            return false;
                        }

                        backoff.SpinOnce();
                        head = this.head.load(Ordering.Relaxed);
                    }
                    else
                    {
                        // Snooze because we need to wait for the stamp to get updated.
                        backoff.SpinOnce(-1);
                        head = this.head.load(Ordering.Relaxed);
                    }
                }
            }

            /// Attempts to pop an element using an exclusive reference of the queue.
            /// <br />
            /// Due to having an exclusive reference, atomic operations and checks are omitted
            public bool pop_mut(out T result)
            {
                var head = this.head.AsRef();
                var tail = this.tail.AsRef();

                // If the tail equals the head, that means the channel is empty.
                if (tail == head)
                {
                    result = default;
                    return false;
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


                var slot = this.buffer[(int)index];

                result = slot->value;

                slot->stamp.AsRef() = head.wrapping_add(this.one_lap);
                this.head.AsRef() = @new;
                return true;
            }

            /// Returns the capacity of the queue.
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public nuint capacity()
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

            public void drop()
            {
                buffer.Dispose();
            }
        }
    }
}