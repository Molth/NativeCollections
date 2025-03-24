using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native list
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeList<T> : IDisposable, IEquatable<NativeList<T>> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeListHandle
        {
            /// <summary>
            ///     Array
            /// </summary>
            public T* Array;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Size
            /// </summary>
            public int Size;

            /// <summary>
            ///     Version
            /// </summary>
            public int Version;

            /// <summary>
            ///     Get or set value
            /// </summary>
            /// <param name="index">Index</param>
            public ref T this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref Array[index];
            }

            /// <summary>
            ///     Get or set value
            /// </summary>
            /// <param name="index">Index</param>
            public ref T this[uint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref Array[index];
            }

            /// <summary>
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                Version++;
                Size = 0;
            }

            /// <summary>
            ///     Add
            /// </summary>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(in T item)
            {
                Version++;
                var size = Size;
                if ((uint)size < (uint)Length)
                {
                    Size = size + 1;
                    Array[size] = item;
                }
                else
                {
                    Grow(size + 1);
                    Size = size + 1;
                    Array[size] = item;
                }
            }

            /// <summary>
            ///     Try add
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Added</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryAdd(in T item)
            {
                var size = Size;
                if ((uint)size < (uint)Length)
                {
                    Version++;
                    Size = size + 1;
                    Array[size] = item;
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Add range
            /// </summary>
            /// <param name="collection">Collection</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddRange(NativeListHandle* collection)
            {
                var other = collection;
                var count = other->Size;
                if (count > 0)
                {
                    if (Length - Size < count)
                        Grow(checked(Size + count));
                    Unsafe.CopyBlockUnaligned(Array + Size, other->Array, (uint)(other->Size * sizeof(T)));
                    Size += count;
                    Version++;
                }
            }

            /// <summary>
            ///     Insert
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Insert(int index, in T item)
            {
                if ((uint)index > (uint)Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                if (Size == Length)
                    Grow(Size + 1);
                if (index < Size)
                    Unsafe.CopyBlockUnaligned(Array + (index + 1), Array + index, (uint)((Size - index) * sizeof(T)));
                Array[index] = item;
                Size++;
                Version++;
            }

            /// <summary>
            ///     Insert
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="collection">Collection</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void InsertRange(int index, NativeListHandle* collection)
            {
                if ((uint)index > (uint)Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                var other = collection;
                var count = other->Size;
                if (count > 0)
                {
                    if (Length - Size < count)
                        Grow(checked(Size + count));
                    if (index < Size)
                        Unsafe.CopyBlockUnaligned(Array + index + count, Array + index, (uint)((Size - index) * sizeof(T)));
                    if (Unsafe.AsPointer(ref this) == collection)
                    {
                        Unsafe.CopyBlockUnaligned(Array + index, Array, (uint)(index * sizeof(T)));
                        Unsafe.CopyBlockUnaligned(Array + index * 2, Array + index + count, (uint)((Size - index) * sizeof(T)));
                    }
                    else
                    {
                        Unsafe.CopyBlockUnaligned(Array + index, other->Array, (uint)(other->Size * sizeof(T)));
                    }

                    Size += count;
                    Version++;
                }
            }

            /// <summary>
            ///     Remove
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Removed</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in T item)
            {
                var index = IndexOf(item);
                if (index >= 0)
                {
                    RemoveAt(index);
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Swap remove
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Removed</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool SwapRemove(in T item)
            {
                var index = IndexOf(item);
                if (index >= 0)
                {
                    SwapRemoveAt(index);
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Remove at
            /// </summary>
            /// <param name="index">Index</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveAt(int index)
            {
                if ((uint)index >= (uint)Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                Size--;
                if (index < Size)
                    Unsafe.CopyBlockUnaligned(Array + index, Array + (index + 1), (uint)((Size - index) * sizeof(T)));
                Version++;
            }

            /// <summary>
            ///     Swap remove at
            /// </summary>
            /// <param name="index">Index</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapRemoveAt(int index)
            {
                if ((uint)index >= (uint)Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                Size--;
                if (index != Size)
                    Array[index] = Array[Size];
                Version++;
            }

            /// <summary>
            ///     Remove range
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveRange(int index, int count)
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
                var offset = Size - index;
                if (offset < count)
                    throw new ArgumentOutOfRangeException(nameof(count), "MustBeLess");
                if (count > 0)
                {
                    Size -= count;
                    if (index < Size)
                        Unsafe.CopyBlockUnaligned(Array + index, Array + (index + count), (uint)((Size - index) * sizeof(T)));
                    Version++;
                }
            }

            /// <summary>
            ///     Reverse
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reverse()
            {
                if (Size > 1)
                    MemoryMarshal.CreateSpan(ref *Array, Size).Reverse();
                Version++;
            }

            /// <summary>
            ///     Reverse
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reverse(int index, int count)
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
                var offset = Size - index;
                if (offset < count)
                    throw new ArgumentOutOfRangeException(nameof(count), "MustBeLess");
                if (count > 1)
                    MemoryMarshal.CreateSpan(ref *(Array + index), count).Reverse();
                Version++;
            }

            /// <summary>
            ///     Contains
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Contains</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(in T item) => Size != 0 && IndexOf(item) >= 0;

            /// <summary>
            ///     Ensure capacity
            /// </summary>
            /// <param name="capacity">Capacity</param>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int EnsureCapacity(int capacity)
            {
                if (capacity < 0)
                    throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
                if (Length < capacity)
                    Grow(capacity);
                return Length;
            }

            /// <summary>
            ///     Trim excess
            /// </summary>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TrimExcess()
            {
                var threshold = (int)(Length * 0.9);
                if (Size < threshold)
                    SetCapacity(Size);
                return Length;
            }

            /// <summary>
            ///     Grow
            /// </summary>
            /// <param name="capacity">Capacity</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Grow(int capacity)
            {
                var newCapacity = 2 * Length;
                if ((uint)newCapacity > 2147483591)
                    newCapacity = 2147483591;
                var expected = Length + 4;
                newCapacity = newCapacity > expected ? newCapacity : expected;
                if (newCapacity < capacity)
                    newCapacity = capacity;
                SetCapacity(newCapacity);
            }

            /// <summary>
            ///     Index of
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Index</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int IndexOf(in T item) => Size == 0 ? -1 : MemoryMarshal.CreateReadOnlySpan(ref *Array, Size).IndexOf(item);

            /// <summary>
            ///     Index of
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="index">Index</param>
            /// <returns>Index</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int IndexOf(in T item, int index)
            {
                if (Size == 0)
                    return -1;
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
                if (index > Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                return MemoryMarshal.CreateReadOnlySpan(ref *(Array + index), Size - index).IndexOf(item);
            }

            /// <summary>
            ///     Index of
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="index">Index</param>
            /// <param name="count">Count</param>
            /// <returns>Index</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int IndexOf(in T item, int index, int count)
            {
                if (Size == 0)
                    return -1;
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
                if (index > Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
                if (index > Size - count)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "BiggerThanCollection");
                return MemoryMarshal.CreateReadOnlySpan(ref *(Array + index), count).IndexOf(item);
            }

            /// <summary>
            ///     Last index of
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Index</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int LastIndexOf(in T item) => Size == 0 ? -1 : MemoryMarshal.CreateReadOnlySpan(ref *(Array + (Size - 1)), Size).LastIndexOf(item);

            /// <summary>
            ///     Last index of
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="index">Index</param>
            /// <returns>Index</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int LastIndexOf(in T item, int index)
            {
                if (Size == 0)
                    return -1;
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
                if (index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                return MemoryMarshal.CreateReadOnlySpan(ref *(Array + index), index + 1).LastIndexOf(item);
            }

            /// <summary>
            ///     Last index of
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="index">Index</param>
            /// <param name="count">Count</param>
            /// <returns>Index</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int LastIndexOf(in T item, int index, int count)
            {
                if (Size == 0)
                    return -1;
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
                if (index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "BiggerThanCollection");
                if (count > index + 1)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "BiggerThanCollection");
                return MemoryMarshal.CreateReadOnlySpan(ref *(Array + index), count).LastIndexOf(item);
            }

            /// <summary>
            ///     Set capacity
            /// </summary>
            /// <param name="capacity">Capacity</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetCapacity(int capacity)
            {
                if (capacity < Size)
                    throw new ArgumentOutOfRangeException(nameof(Capacity), capacity, "SmallCapacity");
                if (capacity != Length)
                {
                    if (capacity > 0)
                    {
                        var newItems = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
                        if (Size > 0)
                            Unsafe.CopyBlockUnaligned(newItems, Array, (uint)(Size * sizeof(T)));
                        NativeMemoryAllocator.Free(Array);
                        Array = newItems;
                        Length = capacity;
                    }
                    else
                    {
                        NativeMemoryAllocator.Free(Array);
                        Array = (T*)NativeMemoryAllocator.Alloc(0);
                        Length = 0;
                    }
                }
            }

            /// <summary>
            ///     As span
            /// </summary>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref *Array, Length);

            /// <summary>
            ///     As span
            /// </summary>
            /// <param name="start">Start</param>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref *(Array + start), Length - start);

            /// <summary>
            ///     As span
            /// </summary>
            /// <param name="start">Start</param>
            /// <param name="length">Length</param>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref *(Array + start), length);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *Array, Length);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <param name="start">Start</param>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(Array + start), Length - start);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <param name="start">Start</param>
            /// <param name="length">Length</param>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(Array + start), length);
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeListHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeListHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeListHandle));
            handle->Array = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
            handle->Length = capacity;
            handle->Size = 0;
            handle->Version = 0;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->Size == 0;

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref (*_handle)[index];
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref (*_handle)[index];
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->Size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->SetCapacity(value);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeList<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeList<T> nativeList && nativeList == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeList<{typeof(T).Name}>";

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeList<T> nativeList) => nativeList.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeList<T> nativeList) => nativeList.AsReadOnlySpan();

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeList<T> left, NativeList<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeList<T> left, NativeList<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            NativeMemoryAllocator.Free(handle->Array);
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item) => _handle->Add(item);

        /// <summary>
        ///     Try add
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in T item) => _handle->TryAdd(item);

        /// <summary>
        ///     Add range
        /// </summary>
        /// <param name="collection">Collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(NativeList<T> collection) => _handle->AddRange(collection._handle);

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item) => _handle->Insert(index, item);

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="collection">Collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRange(int index, NativeList<T> collection) => _handle->InsertRange(index, collection._handle);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T item) => _handle->Remove(item);

        /// <summary>
        ///     Swap remove
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SwapRemove(in T item) => _handle->SwapRemove(item);

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => _handle->RemoveAt(index);

        /// <summary>
        ///     Swap remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapRemoveAt(int index) => _handle->SwapRemoveAt(index);

        /// <summary>
        ///     Remove range
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count) => _handle->RemoveRange(index, count);

        /// <summary>
        ///     Reverse
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse() => _handle->Reverse();

        /// <summary>
        ///     Reverse
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index, int count) => _handle->Reverse(index, count);

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item) => _handle->Contains(item);

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item) => _handle->IndexOf(item);

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index) => _handle->IndexOf(item, index);

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index, int count) => _handle->IndexOf(item, index, count);

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item) => _handle->LastIndexOf(item);

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item, int index) => _handle->LastIndexOf(item, index);

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item, int index, int count) => _handle->LastIndexOf(item, index, count);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => _handle->AsSpan();

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => _handle->AsSpan(start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => _handle->AsSpan(start, length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => _handle->AsReadOnlySpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => _handle->AsReadOnlySpan(start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => _handle->AsReadOnlySpan(start, length);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeList<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(_handle);

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeList
            /// </summary>
            private readonly NativeListHandle* _nativeList;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Current
            /// </summary>
            private T _current;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeList">NativeList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeList)
            {
                var handle = (NativeListHandle*)nativeList;
                _nativeList = handle;
                _index = 0;
                _version = handle->Version;
                _current = default;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeList;
                if (_version == handle->Version && (uint)_index < (uint)handle->Size)
                {
                    _current = handle->Array[_index];
                    _index++;
                    return true;
                }

                if (_version != handle->Version)
                    throw new InvalidOperationException("EnumFailedVersion");
                _index = handle->Size + 1;
                _current = default;
                return false;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}