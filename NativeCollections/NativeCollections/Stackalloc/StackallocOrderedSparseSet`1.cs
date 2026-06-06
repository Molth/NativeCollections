using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc orderedSparseSet
    /// </summary>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.None)]
    public unsafe struct StackallocOrderedSparseSet<TValue> : IReadOnlyCollection<KeyValuePair<int, TValue>> where TValue : unmanaged
    {
        /// <summary>
        ///     Dense
        /// </summary>
        private readonly Entry* _dense;

        /// <summary>
        ///     Sparse
        /// </summary>
        private readonly int* _sparse;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Head
        /// </summary>
        private int _head;

        /// <summary>
        ///     Tail
        /// </summary>
        private int _tail;

        /// <summary>
        ///     Count
        /// </summary>
        private int _count;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     KeyValuePairs
        /// </summary>
        public OrderedKeyValuePairCollection OrderedKeyValuePairs => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Keys
        /// </summary>
        public OrderedKeyCollection OrderedKeys => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Values
        /// </summary>
        public OrderedValueCollection OrderedValues => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="key">Key</param>
        public TValue this[int key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                if (TryGetValue(key, out var obj))
                    return obj;
                ThrowHelpers.ThrowKeyNotFoundException(key);
                return default;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Insert(key, value);
        }

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Length
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Min
        /// </summary>
        public readonly KeyValuePair<int, TValue>? Min
        {
            get
            {
                if (_count > 0)
                {
                    var index = 0;
                    var min = Unsafe.AsRef<Entry>(_dense).Key;
                    for (var i = 1; i < _count; ++i)
                    {
                        var key = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)i).Key;
                        if (key < min)
                        {
                            min = key;
                            index = i;
                        }
                    }

                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
                    return Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
                }

                return null;
            }
        }

        /// <summary>
        ///     Max
        /// </summary>
        public readonly KeyValuePair<int, TValue>? Max
        {
            get
            {
                if (_count > 0)
                {
                    var index = 0;
                    var max = Unsafe.AsRef<Entry>(_dense).Key;
                    for (var i = 1; i < _count; ++i)
                    {
                        var key = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)i).Key;
                        if (key > max)
                        {
                            max = key;
                            index = i;
                        }
                    }

                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
                    return Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
                }

                return null;
            }
        }

        /// <summary>
        ///     Get byte count
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<Entry>(), NativeMemoryAllocator.AlignOf<int>());
            var denseByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<Entry>()), alignment);
            return (int)(denseByteCount + capacity * Unsafe.SizeOf<int>() + alignment - 1);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned("Span<byte> buffer")]
        public StackallocOrderedSparseSet(Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<Entry>(), NativeMemoryAllocator.AlignOf<int>());
            var denseByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<Entry>()), alignment);
            _dense = (Entry*)NativeArray<byte>.Create(buffer, alignment).Buffer;
            _sparse = UnsafeHelpers.AddByteOffset<int>(_dense, (nint)denseByteCount);
            MemoryMarshal.CreateSpan(ref Unsafe.AsRef<int>(_sparse), capacity).Fill(-1);
            _length = capacity;
            _head = -1;
            _tail = -1;
            _count = 0;
            _version = 0;
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            MemoryMarshal.CreateSpan(ref Unsafe.AsRef<int>(_sparse), _length).Fill(-1);
            _head = -1;
            _tail = -1;
            _count = 0;
            ++_version;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(int key, in TValue value)
        {
            ThrowHelpers.ThrowIfNegative(key, ExceptionArgument.key);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(key, _length, ExceptionArgument.key);
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index != -1)
                return false;
            ref var count = ref _count;
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)count);
            entry.Key = key;
            entry.Value = value;
            entry.Next = -1;
            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = count;
            ref var tail = ref _tail;
            if (tail != -1)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)tail).Next = count;
                entry.Previous = tail;
            }
            else
            {
                _head = count;
                entry.Previous = -1;
            }

            tail = count;
            ++count;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult Insert(int key, in TValue value)
        {
            ThrowHelpers.ThrowIfNegative(key, ExceptionArgument.key);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(key, _length, ExceptionArgument.key);
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index != -1)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Value = value;
                ++_version;
                return InsertResult.Overwritten;
            }

            ref var count = ref _count;
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)count);
            entry.Key = key;
            entry.Value = value;
            entry.Next = -1;
            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = count;
            ref var tail = ref _tail;
            if (tail != -1)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)tail).Next = count;
                entry.Previous = tail;
            }
            else
            {
                _head = count;
                entry.Previous = -1;
            }

            tail = count;
            ++count;
            ++_version;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key)
        {
            if ((uint)key >= (uint)_length)
                return false;
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index == -1)
                return false;
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            if (entry.Next != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)entry.Next).Previous = entry.Previous;
            else
                _tail = entry.Previous;
            if (entry.Previous != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)entry.Previous).Next = entry.Next;
            else
                _head = entry.Next;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                if (entry.Next != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)entry.Next).Previous = index;
                else
                    _tail = index;
                if (entry.Previous != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)entry.Previous).Next = index;
                else
                    _head = index;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key, out TValue value)
        {
            if ((uint)key >= (uint)_length)
            {
                value = default;
                return false;
            }

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index == -1)
            {
                value = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            value = entry.Value;
            if (entry.Next != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)entry.Next).Previous = entry.Previous;
            else
                _tail = entry.Previous;
            if (entry.Previous != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)entry.Previous).Next = entry.Next;
            else
                _head = entry.Next;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                if (entry.Next != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)entry.Next).Previous = index;
                else
                    _tail = index;
                if (entry.Previous != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)entry.Previous).Next = index;
                else
                    _head = index;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsKey(int key) => key >= 0 && key < _length && Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) != -1;

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(int key, out TValue value)
        {
            if ((uint)key >= (uint)_length)
            {
                value = default;
                return false;
            }

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index != -1)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(int key, out NativeReference<TValue> value)
        {
            if ((uint)key >= (uint)_length)
            {
                value = default;
                return false;
            }

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index != -1)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
                value = new NativeReference<TValue>(Unsafe.AsPointer(ref entry.Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(int key) => (uint)key >= (uint)_length ? -1 : Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetKeyAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Key;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Value;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetKeyAt(int index, out int key)
        {
            if ((uint)index >= (uint)_count)
            {
                key = default;
                return false;
            }

            key = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Key;
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueAt(int index, out TValue value)
        {
            if ((uint)index >= (uint)_count)
            {
                value = default;
                return false;
            }

            value = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Value;
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReferenceAt(int index, out NativeReference<TValue> value)
        {
            if ((uint)index >= (uint)_count)
            {
                value = default;
                return false;
            }

            value = new NativeReference<TValue>(Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Value));
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly KeyValuePair<int, TValue> GetAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index));
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly KeyValuePair<int, NativeReference<TValue>> GetReferenceAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            return new KeyValuePair<int, NativeReference<TValue>>(entry.Key, new NativeReference<TValue>(Unsafe.AsPointer(ref entry.Value)));
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetAt(int index, out KeyValuePair<int, TValue> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index));
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetReferenceAt(int index, out KeyValuePair<int, NativeReference<TValue>> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            keyValuePair = new KeyValuePair<int, NativeReference<TValue>>(entry.Key, new NativeReference<TValue>(Unsafe.AsPointer(ref entry.Value)));
            return true;
        }

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in TValue value)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Value = value;
            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            var key = entry.Key;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<int, TValue> keyValuePair)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            var key = entry.Key;
            keyValuePair = Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            if ((uint)index >= (uint)_count)
                return false;
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            var key = entry.Key;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out KeyValuePair<int, TValue> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            var key = entry.Key;
            keyValuePair = Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<KeyValuePair<int, TValue>> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<KeyValuePair<int, TValue>>(_dense), _count);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<KeyValuePair<int, TValue>> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<KeyValuePair<int, TValue>>(_dense), (nint)start), _count - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<KeyValuePair<int, TValue>> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<KeyValuePair<int, TValue>>(_dense), (nint)start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<KeyValuePair<int, TValue>>(in StackallocOrderedSparseSet<TValue> stackallocOrderedSparseSet) => stackallocOrderedSparseSet.AsReadOnlySpan();

        /// <summary>
        ///     Entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>
            ///     Key
            /// </summary>
            public int Key;

            /// <summary>
            ///     Value
            /// </summary>
            public TValue Value;

            /// <summary>
            ///     Next
            /// </summary>
            public int Next;

            /// <summary>
            ///     Previous
            /// </summary>
            public int Previous;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocOrderedSparseSet<TValue> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete("Call this method will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<KeyValuePair<int, TValue>> IEnumerable<KeyValuePair<int, TValue>>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete("Call this method will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<KeyValuePair<int, TValue>>
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeSparseSet)
            {
                var handle = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;
                _nativeSparseSet = handle;
                _version = handle->_version;
                _index = -1;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeSparseSet;
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                var num = _index + 1;
                if (num >= handle->_count)
                    return false;
                _index = num;
                return true;
            }

            /// <summary>
            ///     Reset
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            /// <summary>
            ///     Current
            /// </summary>
            public readonly KeyValuePair<int, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.Add(ref Unsafe.AsRef<KeyValuePair<int, TValue>>(_nativeSparseSet->_dense), (nint)_index);
            }
        }

        /// <summary>
        ///     Key collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection : IReadOnlyCollection<int>
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

            /// <summary>
            ///     Get key
            /// </summary>
            /// <param name="index">Index</param>
            public int this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var handle = _nativeSparseSet;
                    ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
                    ThrowHelpers.ThrowIfGreaterThanOrEqual(index, handle->_count, ExceptionArgument.index);
                    return Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_dense), (nint)index).Key;
                }
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSparseSet);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [Obsolete("Call this method will always throw an exception.")]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<int> IEnumerable<int>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [Obsolete("Call this method will always throw an exception.")]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<int>
            {
                /// <summary>
                ///     NativeSparseSet
                /// </summary>
                private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSparseSet;
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    var num = _index + 1;
                    if (num >= handle->_count)
                        return false;
                    _index = num;
                    return true;
                }

                /// <summary>
                ///     Reset
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => _index = -1;

                /// <summary>
                ///     Current
                /// </summary>
                public readonly int Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => Unsafe.Add(ref Unsafe.AsRef<Entry>(_nativeSparseSet->_dense), (nint)_index).Key;
                }
            }
        }

        /// <summary>
        ///     Value collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ValueCollection : IReadOnlyCollection<TValue>
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

            /// <summary>
            ///     Get reference
            /// </summary>
            /// <param name="index">Index</param>
            public ref TValue this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var handle = _nativeSparseSet;
                    ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
                    ThrowHelpers.ThrowIfGreaterThanOrEqual(index, handle->_count, ExceptionArgument.index);
                    return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_dense), (nint)index).Value;
                }
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSparseSet);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [Obsolete("Call this method will always throw an exception.")]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [Obsolete("Call this method will always throw an exception.")]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TValue>
            {
                /// <summary>
                ///     NativeSparseSet
                /// </summary>
                private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSparseSet;
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    var num = _index + 1;
                    if (num >= handle->_count)
                        return false;
                    _index = num;
                    return true;
                }

                /// <summary>
                ///     Reset
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => _index = -1;

                /// <summary>
                ///     Current
                /// </summary>
                public readonly TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => Unsafe.Add(ref Unsafe.AsRef<Entry>(_nativeSparseSet->_dense), (nint)_index).Value;
                }
            }
        }

        /// <summary>
        ///     KeyValuePair collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct OrderedKeyValuePairCollection
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal OrderedKeyValuePairCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<KeyValuePair<int, TValue>> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _nativeSparseSet->_count));
                var dense = _nativeSparseSet->_dense;
                var current = _nativeSparseSet->_head;
                for (var index = 0; index < count; ++index)
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), (nint)current);
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), new KeyValuePair<int, TValue>(entry.Key, entry.Value));
                    current = entry.Next;
                }

                return count;
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<int, TValue>>(buffer), count);

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<KeyValuePair<int, TValue>> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var dense = _nativeSparseSet->_dense;
                var current = _nativeSparseSet->_head;
                for (var index = 0; index < _nativeSparseSet->_count; ++index)
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), (nint)current);
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), new KeyValuePair<int, TValue>(entry.Key, entry.Value));
                    current = entry.Next;
                }
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<int, TValue>>(buffer));

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSparseSet);

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<KeyValuePair<int, TValue>>
            {
                /// <summary>
                ///     NativeSparseSet
                /// </summary>
                private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

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
                private Entry* _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                    _current = handle->_head != -1 ? UnsafeHelpers.Add<Entry>(handle->_dense, handle->_head) : null;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSparseSet;
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    var num = _index + 1;
                    if (num >= handle->_count)
                        return false;
                    _index = num;
                    if (num != 0)
                        _current = UnsafeHelpers.Add<Entry>(handle->_dense, _current->Next);
                    return true;
                }

                /// <summary>
                ///     Reset
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => _index = -1;

                /// <summary>
                ///     Current
                /// </summary>
                public readonly KeyValuePair<int, TValue> Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => Unsafe.AsRef<KeyValuePair<int, TValue>>(_current);
                }
            }
        }

        /// <summary>
        ///     Key collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct OrderedKeyCollection
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal OrderedKeyCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<int> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _nativeSparseSet->_count));
                var dense = _nativeSparseSet->_dense;
                var current = _nativeSparseSet->_head;
                for (var index = 0; index < count; ++index)
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), (nint)current);
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), entry.Key);
                    current = entry.Next;
                }

                return count;
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, int>(buffer), count);

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<int> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var dense = _nativeSparseSet->_dense;
                var current = _nativeSparseSet->_head;
                for (var index = 0; index < _nativeSparseSet->_count; ++index)
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), (nint)current);
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), entry.Key);
                    current = entry.Next;
                }
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, int>(buffer));

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSparseSet);

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<int>
            {
                /// <summary>
                ///     NativeSparseSet
                /// </summary>
                private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

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
                private Entry* _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                    _current = handle->_head != -1 ? UnsafeHelpers.Add<Entry>(handle->_dense, handle->_head) : null;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSparseSet;
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    var num = _index + 1;
                    if (num >= handle->_count)
                        return false;
                    _index = num;
                    if (num != 0)
                        _current = UnsafeHelpers.Add<Entry>(handle->_dense, _current->Next);
                    return true;
                }

                /// <summary>
                ///     Reset
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => _index = -1;

                /// <summary>
                ///     Current
                /// </summary>
                public int Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current->Key;
                }
            }
        }

        /// <summary>
        ///     Value collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct OrderedValueCollection
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSparseSet">NativeSparseSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal OrderedValueCollection(void* nativeSparseSet) => _nativeSparseSet = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSparseSet->_count;

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<TValue> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _nativeSparseSet->_count));
                var dense = _nativeSparseSet->_dense;
                var current = _nativeSparseSet->_head;
                for (var index = 0; index < count; ++index)
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), (nint)current);
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), entry.Value);
                    current = entry.Next;
                }

                return count;
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer), count);

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<TValue> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var dense = _nativeSparseSet->_dense;
                var current = _nativeSparseSet->_head;
                for (var index = 0; index < _nativeSparseSet->_count; ++index)
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), (nint)current);
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), entry.Value);
                    current = entry.Next;
                }
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer));

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSparseSet);

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TValue>
            {
                /// <summary>
                ///     NativeSparseSet
                /// </summary>
                private readonly StackallocOrderedSparseSet<TValue>* _nativeSparseSet;

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
                private Entry* _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSparseSet">NativeSparseSet</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSparseSet)
                {
                    var handle = (StackallocOrderedSparseSet<TValue>*)nativeSparseSet;
                    _nativeSparseSet = handle;
                    _version = handle->_version;
                    _index = -1;
                    _current = handle->_head != -1 ? UnsafeHelpers.Add<Entry>(handle->_dense, handle->_head) : null;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativeSparseSet;
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    var num = _index + 1;
                    if (num >= handle->_count)
                        return false;
                    _index = num;
                    if (num != 0)
                        _current = UnsafeHelpers.Add<Entry>(handle->_dense, _current->Next);
                    return true;
                }

                /// <summary>
                ///     Reset
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => _index = -1;

                /// <summary>
                ///     Current
                /// </summary>
                public TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current->Value;
                }
            }
        }
    }
}