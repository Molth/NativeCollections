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
    [NativeCollection]
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
            get => ref _handle->Array[index];
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _handle->Array[index];
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
            set
            {
                var handle = _handle;
                if (value < handle->Size)
                    throw new ArgumentOutOfRangeException(nameof(Capacity), value, "SmallCapacity");
                if (value != handle->Length)
                {
                    if (value > 0)
                    {
                        var newItems = (T*)NativeMemoryAllocator.Alloc((uint)(value * sizeof(T)));
                        if (handle->Size > 0)
                            Unsafe.CopyBlockUnaligned(newItems, handle->Array, (uint)(handle->Size * sizeof(T)));
                        NativeMemoryAllocator.Free(handle->Array);
                        handle->Array = newItems;
                        handle->Length = value;
                    }
                    else
                    {
                        NativeMemoryAllocator.Free(handle->Array);
                        handle->Array = (T*)NativeMemoryAllocator.Alloc(0);
                        handle->Length = 0;
                    }
                }
            }
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
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            var handle = _handle;
            return MemoryMarshal.CreateSpan(ref *handle->Array, handle->Length);
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start)
        {
            var handle = _handle;
            return MemoryMarshal.CreateSpan(ref *(handle->Array + start), handle->Length - start);
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length)
        {
            var handle = _handle;
            return MemoryMarshal.CreateSpan(ref *(handle->Array + start), length);
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            var handle = _handle;
            return MemoryMarshal.CreateReadOnlySpan(ref *handle->Array, handle->Length);
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start)
        {
            var handle = _handle;
            return MemoryMarshal.CreateReadOnlySpan(ref *(handle->Array + start), handle->Length - start);
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length)
        {
            var handle = _handle;
            return MemoryMarshal.CreateReadOnlySpan(ref *(handle->Array + start), length);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var handle = _handle;
            handle->Version++;
            handle->Size = 0;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            var handle = _handle;
            handle->Version++;
            var size = handle->Size;
            if ((uint)size < (uint)handle->Length)
            {
                handle->Size = size + 1;
                handle->Array[size] = item;
            }
            else
            {
                Grow(size + 1);
                handle->Size = size + 1;
                handle->Array[size] = item;
            }
        }

        /// <summary>
        ///     Add range
        /// </summary>
        /// <param name="collection">Collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(in NativeList<T> collection)
        {
            var handle = _handle;
            var count = collection._handle->Size;
            if (count > 0)
            {
                if (handle->Length - handle->Size < count)
                    Grow(checked(handle->Size + count));
                Unsafe.CopyBlockUnaligned(handle->Array + handle->Size, collection._handle->Array, (uint)(collection._handle->Size * sizeof(T)));
                handle->Size += count;
                handle->Version++;
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
            var handle = _handle;
            if ((uint)index > (uint)handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            if (handle->Size == handle->Length)
                Grow(handle->Size + 1);
            if (index < handle->Size)
                Unsafe.CopyBlockUnaligned(handle->Array + (index + 1), handle->Array + index, (uint)((handle->Size - index) * sizeof(T)));
            handle->Array[index] = item;
            handle->Size++;
            handle->Version++;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="collection">Collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRange(int index, in NativeList<T> collection)
        {
            var handle = _handle;
            if ((uint)index > (uint)handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            var count = collection._handle->Size;
            if (count > 0)
            {
                if (handle->Length - handle->Size < count)
                    Grow(checked(handle->Size + count));
                if (index < handle->Size)
                    Unsafe.CopyBlockUnaligned(handle->Array + index + count, handle->Array + index, (uint)((handle->Size - index) * sizeof(T)));
                if (this == collection)
                {
                    Unsafe.CopyBlockUnaligned(handle->Array + index, handle->Array, (uint)(index * sizeof(T)));
                    Unsafe.CopyBlockUnaligned(handle->Array + index * 2, handle->Array + index + count, (uint)((handle->Size - index) * sizeof(T)));
                }
                else
                {
                    Unsafe.CopyBlockUnaligned(handle->Array + index, collection._handle->Array, (uint)(collection._handle->Size * sizeof(T)));
                }

                handle->Size += count;
                handle->Version++;
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
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            var handle = _handle;
            if ((uint)index >= (uint)handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            handle->Size--;
            if (index < handle->Size)
                Unsafe.CopyBlockUnaligned(handle->Array + index, handle->Array + (index + 1), (uint)((handle->Size - index) * sizeof(T)));
            handle->Version++;
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
            var handle = _handle;
            var offset = handle->Size - index;
            if (offset < count)
                throw new ArgumentOutOfRangeException(offset.ToString(), "InvalidOffLen");
            if (count > 0)
            {
                handle->Size -= count;
                if (index < handle->Size)
                    Unsafe.CopyBlockUnaligned(handle->Array + index, handle->Array + (index + count), (uint)((handle->Size - index) * sizeof(T)));
                handle->Version++;
            }
        }

        /// <summary>
        ///     Reverse
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse()
        {
            var handle = _handle;
            if (handle->Size > 1)
                MemoryMarshal.CreateSpan(ref *handle->Array, handle->Size).Reverse();
            handle->Version++;
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
            var handle = _handle;
            var offset = handle->Size - index;
            if (offset < count)
                throw new ArgumentOutOfRangeException(offset.ToString(), "InvalidOffLen");
            if (count > 1)
                MemoryMarshal.CreateSpan(ref *(handle->Array + index), count).Reverse();
            handle->Version++;
        }

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item) => _handle->Size != 0 && IndexOf(item) >= 0;

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
            var handle = _handle;
            if (handle->Length < capacity)
                Grow(capacity);
            return handle->Length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var handle = _handle;
            var threshold = (int)(handle->Length * 0.9);
            if (handle->Size < threshold)
                Capacity = handle->Size;
            return handle->Length;
        }

        /// <summary>
        ///     Grow
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var handle = _handle;
            var newCapacity = 2 * handle->Length;
            if ((uint)newCapacity > 2147483591)
                newCapacity = 2147483591;
            var expected = handle->Length + 4;
            newCapacity = newCapacity > expected ? newCapacity : expected;
            if (newCapacity < capacity)
                newCapacity = capacity;
            Capacity = newCapacity;
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item)
        {
            var handle = _handle;
            return handle->Size == 0 ? -1 : MemoryMarshal.CreateReadOnlySpan(ref *handle->Array, handle->Size).IndexOf(item);
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index)
        {
            var handle = _handle;
            if (handle->Size == 0)
                return -1;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (index > handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            return MemoryMarshal.CreateReadOnlySpan(ref *(handle->Array + index), handle->Size - index).IndexOf(item);
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
            var handle = _handle;
            if (handle->Size == 0)
                return -1;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
            if (index > handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            if (index > handle->Size - count)
                throw new ArgumentOutOfRangeException(nameof(count), count, "BiggerThanCollection");
            return MemoryMarshal.CreateReadOnlySpan(ref *(handle->Array + index), count).IndexOf(item);
        }

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item)
        {
            var handle = _handle;
            return handle->Size == 0 ? -1 : MemoryMarshal.CreateReadOnlySpan(ref *(handle->Array + (handle->Size - 1)), handle->Size).LastIndexOf(item);
        }

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item, int index)
        {
            var handle = _handle;
            if (handle->Size == 0)
                return -1;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (index >= handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return MemoryMarshal.CreateReadOnlySpan(ref *(handle->Array + index), index + 1).LastIndexOf(item);
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
            var handle = _handle;
            if (handle->Size == 0)
                return -1;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
            if (index >= handle->Size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "BiggerThanCollection");
            if (count > index + 1)
                throw new ArgumentOutOfRangeException(nameof(count), count, "BiggerThanCollection");
            return MemoryMarshal.CreateReadOnlySpan(ref *(handle->Array + index), count).LastIndexOf(item);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeList<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeList
            /// </summary>
            private readonly NativeList<T> _nativeList;

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
            internal Enumerator(in NativeList<T> nativeList)
            {
                _nativeList = nativeList;
                _index = 0;
                _version = nativeList._handle->Version;
                _current = default;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeList._handle;
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