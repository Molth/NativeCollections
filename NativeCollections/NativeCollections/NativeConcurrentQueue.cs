using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
using System.Threading;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberHidesStaticFromOuterClass

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct NativeConcurrentQueue<T> : IDisposable, IEquatable<NativeConcurrentQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeConcurrentQueueHandle
        {
            /// <summary>
            ///     Cross segment lock
            /// </summary>
            public NativeMonitorLock CrossSegmentLock;

            /// <summary>
            ///     Segment pool
            /// </summary>
            public NativeMemoryPool SegmentPool;

            /// <summary>
            ///     Slots pool
            /// </summary>
            public NativeConcurrentQueueArrayPool<NativeConcurrentQueueSegment<T>.Slot> SlotsPool;

            /// <summary>
            ///     Tail
            /// </summary>
            public volatile NativeConcurrentQueueSegment<T>* Tail;

            /// <summary>
            ///     Head
            /// </summary>
            public volatile NativeConcurrentQueueSegment<T>* Head;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeConcurrentQueueHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="arrayPoolSize">Array pool size</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentQueue(int size, int maxFreeSlabs, int arrayPoolSize, int capacity)
        {
            var segmentPool = new NativeMemoryPool(size, sizeof(NativeConcurrentQueueSegment<T>), maxFreeSlabs);
            if (arrayPoolSize < 1)
                arrayPoolSize = 1;
            else if (arrayPoolSize > 64)
                arrayPoolSize = 64;
            if (capacity < 32)
                capacity = 32;
#if NET5_0_OR_GREATER
            var n = (int)Math.Ceiling(Math.Log2(capacity / 32.0 + 1.0));
#else
            var n = (int)Math.Ceiling(Math.Log(capacity / 32.0 + 1.0) / Math.Log(2));
#endif
            var maxLength = 32 * (2 << (n - 2));
            var slotsPool = new NativeConcurrentQueueArrayPool<NativeConcurrentQueueSegment<T>.Slot>(arrayPoolSize, maxLength);
            _handle = (NativeConcurrentQueueHandle*)NativeMemoryAllocator.Alloc(sizeof(NativeConcurrentQueueHandle));
            _handle->CrossSegmentLock = new NativeMonitorLock(new object());
            _handle->SegmentPool = segmentPool;
            _handle->SlotsPool = slotsPool;
            var segment = (NativeConcurrentQueueSegment<T>*)_handle->SegmentPool.Rent();
            var slots = _handle->SlotsPool.Rent(32);
            segment->Initialize(slots, 32);
            _handle->Tail = _handle->Head = segment;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var segment = _handle->Head;
                while (true)
                {
                    var next = Volatile.Read(ref segment->NextSegment);
                    if (segment->TryPeek())
                        return false;
                    if (next != IntPtr.Zero)
                        segment = (NativeConcurrentQueueSegment<T>*)next;
                    else if (Volatile.Read(ref segment->NextSegment) == IntPtr.Zero)
                        break;
                }

                return true;
            }
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var spinCount = 0;
                while (true)
                {
                    var head = _handle->Head;
                    var tail = _handle->Tail;
                    var headHead = Volatile.Read(ref head->HeadAndTail.Head);
                    var headTail = Volatile.Read(ref head->HeadAndTail.Tail);
                    if (head == tail)
                    {
                        if (head == _handle->Head && tail == _handle->Tail && headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail))
                            return GetCount(head, headHead, headTail);
                    }
                    else if ((NativeConcurrentQueueSegment<T>*)head->NextSegment == tail)
                    {
                        var tailHead = Volatile.Read(ref tail->HeadAndTail.Head);
                        var tailTail = Volatile.Read(ref tail->HeadAndTail.Tail);
                        if (head == _handle->Head && tail == _handle->Tail && headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail) && tailHead == Volatile.Read(ref tail->HeadAndTail.Head) && tailTail == Volatile.Read(ref tail->HeadAndTail.Tail))
                            return GetCount(head, headHead, headTail) + GetCount(tail, tailHead, tailTail);
                    }
                    else
                    {
                        _handle->CrossSegmentLock.Enter();
                        if (head == _handle->Head && tail == _handle->Tail)
                        {
                            var tailHead = Volatile.Read(ref tail->HeadAndTail.Head);
                            var tailTail = Volatile.Read(ref tail->HeadAndTail.Tail);
                            if (headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail) && tailHead == Volatile.Read(ref tail->HeadAndTail.Head) && tailTail == Volatile.Read(ref tail->HeadAndTail.Tail))
                            {
                                var count = GetCount(head, headHead, headTail) + GetCount(tail, tailHead, tailTail);
                                for (var s = (NativeConcurrentQueueSegment<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegment<T>*)s->NextSegment)
                                    count += s->HeadAndTail.Tail - s->FreezeOffset;
                                return count;
                            }
                        }

                        _handle->CrossSegmentLock.Exit();
                    }

                    if ((spinCount >= 10 && (spinCount - 10) % 2 == 0) || Environment.ProcessorCount == 1)
                    {
                        var yieldsSoFar = spinCount >= 10 ? (spinCount - 10) / 2 : spinCount;
                        if (yieldsSoFar % 5 == 4)
                            Thread.Sleep(0);
                        else
                            Thread.Yield();
                    }
                    else
                    {
                        var iterations = Environment.ProcessorCount / 2;
                        if (spinCount <= 30 && 1 << spinCount < iterations)
                            iterations = 1 << spinCount;
                        Thread.SpinWait(iterations);
                    }

                    spinCount = spinCount == int.MaxValue ? 10 : spinCount + 1;
                }
            }
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentQueue<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentQueue<T> nativeConcurrentQueue && nativeConcurrentQueue == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_handle;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeConcurrentQueue<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentQueue<T> left, NativeConcurrentQueue<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentQueue<T> left, NativeConcurrentQueue<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_handle == null)
                return;
            _handle->CrossSegmentLock.Dispose();
            _handle->SegmentPool.Dispose();
            _handle->SlotsPool.Dispose();
            NativeMemoryAllocator.Free(_handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _handle->CrossSegmentLock.Enter();
            _handle->Tail->EnsureFrozenForEnqueues();
            var node = _handle->Head;
            while (node != null)
            {
                var temp = node;
                node = (NativeConcurrentQueueSegment<T>*)node->NextSegment;
                temp->Dispose(_handle->SlotsPool);
                _handle->SegmentPool.Return(temp);
            }

            var segment = (NativeConcurrentQueueSegment<T>*)NativeMemoryAllocator.Alloc(sizeof(NativeConcurrentQueueSegment<T>));
            var slots = _handle->SlotsPool.Rent(32);
            segment->Initialize(slots, 32);
            _handle->Tail = _handle->Head = segment;
            _handle->CrossSegmentLock.Exit();
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            if (!_handle->Tail->TryEnqueue(item))
            {
                while (true)
                {
                    var tail = _handle->Tail;
                    if (tail->TryEnqueue(item))
                        return;
                    _handle->CrossSegmentLock.Enter();
                    if (tail == _handle->Tail)
                    {
                        tail->EnsureFrozenForEnqueues();
                        var newSize = tail->Length * 2;
                        var nextSize = newSize <= 1048576 ? newSize : 1048576;
                        var newTail = (NativeConcurrentQueueSegment<T>*)_handle->SegmentPool.Rent();
                        var array = _handle->SlotsPool.Rent(nextSize);
                        newTail->Initialize(array, nextSize);
                        tail->NextSegment = (nint)newTail;
                        _handle->Tail = newTail;
                    }

                    _handle->CrossSegmentLock.Exit();
                }
            }
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result)
        {
            var head = _handle->Head;
            if (head->TryDequeue(out result))
                return true;
            if (head->NextSegment == IntPtr.Zero)
            {
                result = default;
                return false;
            }

            while (true)
            {
                head = _handle->Head;
                if (head->TryDequeue(out result))
                    return true;
                if (head->NextSegment == IntPtr.Zero)
                {
                    result = default;
                    return false;
                }

                if (head->TryDequeue(out result))
                    return true;
                _handle->CrossSegmentLock.Enter();
                if (head == _handle->Head)
                {
                    _handle->Head = (NativeConcurrentQueueSegment<T>*)head->NextSegment;
                    head->Dispose(_handle->SlotsPool);
                    _handle->SegmentPool.Return(head);
                }

                _handle->CrossSegmentLock.Exit();
            }
        }

        /// <summary>
        ///     Get count
        /// </summary>
        /// <param name="segment">Segment</param>
        /// <param name="head">Head</param>
        /// <param name="tail">Tail</param>
        /// <returns>Count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetCount(NativeConcurrentQueueSegment<T>* segment, int head, int tail)
        {
            if (head != tail && head != tail - segment->FreezeOffset)
            {
                head &= segment->SlotsMask;
                tail &= segment->SlotsMask;
                return head < tail ? tail - head : segment->Length - head + tail;
            }

            return 0;
        }

        /// <summary>
        ///     Get count
        /// </summary>
        /// <param name="head">Head</param>
        /// <param name="headHead">Head head</param>
        /// <param name="tail">Tail</param>
        /// <param name="tailTail">Tail tail</param>
        /// <returns>Count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetCount(NativeConcurrentQueueSegment<T>* head, int headHead, NativeConcurrentQueueSegment<T>* tail, int tailTail)
        {
            long count = 0;
            var headTail = (head == tail ? tailTail : Volatile.Read(ref head->HeadAndTail.Tail)) - head->FreezeOffset;
            if (headHead < headTail)
            {
                headHead &= head->SlotsMask;
                headTail &= head->SlotsMask;
                count += headHead < headTail ? headTail - headHead : head->Length - headHead + headTail;
            }

            if (head != tail)
            {
                for (var s = (NativeConcurrentQueueSegment<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegment<T>*)s->NextSegment)
                    count += s->HeadAndTail.Tail - s->FreezeOffset;
                count += tailTail - tail->FreezeOffset;
            }

            return count;
        }
    }

    /// <summary>
    ///     Native concurrentQueue segment
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeConcurrentQueueSegment<T> where T : unmanaged
    {
        /// <summary>
        ///     Slots
        /// </summary>
        public Slot* Slots;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length;

        /// <summary>
        ///     Slots mask
        /// </summary>
        public int SlotsMask;

        /// <summary>
        ///     Head and tail
        /// </summary>
        public NativeConcurrentQueuePaddedHeadAndTail HeadAndTail;

        /// <summary>
        ///     Frozen for enqueues
        /// </summary>
        public bool FrozenForEnqueues;

        /// <summary>
        ///     Next segment
        /// </summary>
        public nint NextSegment;

        /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="slots">Slots</param>
        /// <param name="boundedLength">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(Slot* slots, int boundedLength)
        {
            Slots = slots;
            for (var i = 0; i < boundedLength; ++i)
                Slots[i].SequenceNumber = i;
            Length = boundedLength;
            SlotsMask = boundedLength - 1;
            HeadAndTail = new NativeConcurrentQueuePaddedHeadAndTail();
            FrozenForEnqueues = false;
            NextSegment = IntPtr.Zero;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="arrayPool">Slots pool</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose(in NativeConcurrentQueueArrayPool<Slot> arrayPool) => arrayPool.Return(Length, Slots);

        /// <summary>
        ///     Freeze offset
        /// </summary>
        public int FreezeOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Length * 2;
        }

        /// <summary>
        ///     Ensure frozen for enqueues
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureFrozenForEnqueues()
        {
            if (!FrozenForEnqueues)
            {
                FrozenForEnqueues = true;
                Interlocked.Add(ref HeadAndTail.Tail, FreezeOffset);
            }
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result)
        {
            var slots = Slots;
            var count = 0;
            while (true)
            {
                var currentHead = Volatile.Read(ref HeadAndTail.Head);
                var slotsIndex = currentHead & SlotsMask;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                {
                    if (Interlocked.CompareExchange(ref HeadAndTail.Head, currentHead + 1, currentHead) == currentHead)
                    {
                        result = slots[slotsIndex].Item;
                        Volatile.Write(ref slots[slotsIndex].SequenceNumber, currentHead + Length);
                        return true;
                    }
                }
                else if (diff < 0)
                {
                    var frozen = FrozenForEnqueues;
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - FreezeOffset - currentHead <= 0))
                    {
                        result = default;
                        return false;
                    }

                    if ((count >= 10 && (count - 10) % 2 == 0) || Environment.ProcessorCount == 1)
                    {
                        var yieldsSoFar = count >= 10 ? (count - 10) / 2 : count;
                        if (yieldsSoFar % 5 == 4)
                            Thread.Sleep(0);
                        else
                            Thread.Yield();
                    }
                    else
                    {
                        var iterations = Environment.ProcessorCount / 2;
                        if (count <= 30 && 1 << count < iterations)
                            iterations = 1 << count;
                        Thread.SpinWait(iterations);
                    }

                    count = count == int.MaxValue ? 10 : count + 1;
                }
            }
        }

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek()
        {
            var slots = Slots;
            var count = 0;
            while (true)
            {
                var currentHead = Volatile.Read(ref HeadAndTail.Head);
                var slotsIndex = currentHead & SlotsMask;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                    return true;
                if (diff < 0)
                {
                    var frozen = FrozenForEnqueues;
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - FreezeOffset - currentHead <= 0))
                        return false;
                    if ((count >= 10 && (count - 10) % 2 == 0) || Environment.ProcessorCount == 1)
                    {
                        var yieldsSoFar = count >= 10 ? (count - 10) / 2 : count;
                        if (yieldsSoFar % 5 == 4)
                            Thread.Sleep(0);
                        else
                            Thread.Yield();
                    }
                    else
                    {
                        var iterations = Environment.ProcessorCount / 2;
                        if (count <= 30 && 1 << count < iterations)
                            iterations = 1 << count;
                        Thread.SpinWait(iterations);
                    }

                    count = count == int.MaxValue ? 10 : count + 1;
                }
            }
        }

        /// <summary>
        ///     Try enqueue
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(T item)
        {
            var slots = Slots;
            while (true)
            {
                var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                var slotsIndex = currentTail & SlotsMask;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - currentTail;
                if (diff == 0)
                {
                    if (Interlocked.CompareExchange(ref HeadAndTail.Tail, currentTail + 1, currentTail) == currentTail)
                    {
                        slots[slotsIndex].Item = item;
                        Volatile.Write(ref slots[slotsIndex].SequenceNumber, currentTail + 1);
                        return true;
                    }
                }
                else if (diff < 0)
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     Slot
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Slot
        {
            /// <summary>
            ///     Item
            /// </summary>
            public T Item;

            /// <summary>
            ///     Sequence number
            /// </summary>
            public int SequenceNumber;
        }
    }

    /// <summary>
    ///     NativeConcurrentQueue padded head and tail
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 3 * CACHE_LINE_SIZE)]
    internal struct NativeConcurrentQueuePaddedHeadAndTail
    {
        /// <summary>
        ///     Head
        /// </summary>
        [FieldOffset(1 * CACHE_LINE_SIZE)] public int Head;

        /// <summary>
        ///     Tail
        /// </summary>
        [FieldOffset(2 * CACHE_LINE_SIZE)] public int Tail;

        /// <summary>
        ///     Catch line size
        /// </summary>
#if TARGET_ARM64
        public const int CACHE_LINE_SIZE = 128;
#else
        public const int CACHE_LINE_SIZE = 64;
#endif
    }

    /// <summary>
    ///     Native concurrentQueue array pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal readonly unsafe struct NativeConcurrentQueueArrayPool<T> : IDisposable, IEquatable<NativeConcurrentQueueArrayPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Buckets
        /// </summary>
        private readonly NativeConcurrentQueueArrayPoolBucket* _buckets;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Size
        /// </summary>
        private readonly int _size;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxLength">Max length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentQueueArrayPool(int size, int maxLength)
        {
            if (maxLength > 1048576)
                maxLength = 1048576;
            else if (maxLength < 32)
                maxLength = 32;
            var length = SelectBucketIndex(maxLength) + 1;
            var buckets = (NativeConcurrentQueueArrayPoolBucket*)NativeMemoryAllocator.Alloc(length * sizeof(NativeConcurrentQueueArrayPoolBucket));
            for (var i = 0; i < length; ++i)
                buckets[i].Initialize(size, 32 << i);
            _buckets = buckets;
            _length = length;
            _size = size;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _buckets != null;

        /// <summary>
        ///     Size
        /// </summary>
        public int Size => _size;

        /// <summary>
        ///     Max length
        /// </summary>
        public int MaxLength => 32 << (_length - 1);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentQueueArrayPool<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentQueueArrayPool<T> nativeConcurrentQueueArrayPool && nativeConcurrentQueueArrayPool == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_buckets;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeConcurrentQueueArrayPool<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentQueueArrayPool<T> left, NativeConcurrentQueueArrayPool<T> right) => left._buckets == right._buckets;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentQueueArrayPool<T> left, NativeConcurrentQueueArrayPool<T> right) => left._buckets != right._buckets;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_buckets == null)
                return;
            for (var i = 0; i < _length; ++i)
                _buckets[i].Dispose();
            NativeMemoryAllocator.Free(_buckets);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <param name="minimumLength">Minimum buffer length</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent(int minimumLength)
        {
            if (minimumLength < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLength), minimumLength, "MustBeNonNegative");
            var index = SelectBucketIndex(minimumLength);
            return index < _length ? _buckets[index].Rent() : (T*)NativeMemoryAllocator.Alloc(minimumLength * sizeof(T));
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="array">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(int length, T* array)
        {
            if (length < 32 || (length & (length - 1)) != 0)
            {
                NativeMemoryAllocator.Free(array);
                return;
            }

            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
            {
                NativeMemoryAllocator.Free(array);
                return;
            }

            _buckets[bucket].Return(array);
        }

        /// <summary>
        ///     Select bucket index
        /// </summary>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>Bucket index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int SelectBucketIndex(int bufferSize) => BitOperationsHelper.Log2(((uint)bufferSize - 1) | 15) - 4;

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentQueueArrayPool<T> Empty => new();

        /// <summary>
        ///     NativeConcurrentQueueArrayPool bucket
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeConcurrentQueueArrayPoolBucket : IDisposable
        {
            /// <summary>
            ///     Size
            /// </summary>
            private int _size;

            /// <summary>
            ///     Buffers
            /// </summary>
            private T** _array;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Memory pool
            /// </summary>
            private NativeMemoryPool _memoryPool;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="size">Size</param>
            /// <param name="length">Length</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Initialize(int size, int length)
            {
                _size = size;
                _array = (T**)NativeMemoryAllocator.AllocZeroed(size * sizeof(T*));
                _index = 0;
                _memoryPool = new NativeMemoryPool(size, length * sizeof(T), 0);
            }

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                NativeMemoryAllocator.Free(_array);
                _memoryPool.Dispose();
            }

            /// <summary>
            ///     Rent buffer
            /// </summary>
            /// <returns>Buffer</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T* Rent()
            {
                T* ptr = null;
                if (_index < _size)
                {
                    ptr = _array[_index];
                    _array[_index++] = null;
                }

                if (ptr == null)
                    ptr = (T*)_memoryPool.Rent();
                return ptr;
            }

            /// <summary>
            ///     Return buffer
            /// </summary>
            /// <param name="ptr">Pointer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Return(T* ptr)
            {
                if (_index != 0)
                    _array[--_index] = ptr;
                else
                    _memoryPool.Return(ptr);
            }
        }
    }
}