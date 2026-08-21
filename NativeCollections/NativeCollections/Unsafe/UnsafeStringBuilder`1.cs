using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     This class represents a mutable string.
    ///     It is convenient for situations in
    ///     which it is desirable to modify a string, perhaps by removing, replacing, or
    ///     inserting characters, without creating a new String subsequent to
    ///     each modification.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    [IsAssignableTo(typeof(IEquatable<>))]
    public unsafe struct UnsafeStringBuilder<T> : IIsCreated, IDisposable, IEquatable<UnsafeStringBuilder<T>>, IReadOnlyCollection<T>, IBufferWriter<T>
#if NET9_0_OR_GREATER
        , IEquatable<ReadOnlySpan<T>>
#endif
        where T : unmanaged, IComparable<T>, IEquatable<T>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private NativeArray<T> _buffer;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private NativeArray<T> _array;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        private int _length;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buffer.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _length == 0;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _length;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _buffer.Length;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public readonly Span<T> Buffer => _buffer;

        /// <summary>
        ///     Gets the portion of the buffer that contains the current string content.
        /// </summary>
        /// <remarks>
        ///     This span represents the characters that are currently considered part of the string.
        ///     Its length equals <see cref="Length" />.
        /// </remarks>
        public readonly Span<T> Text => _buffer.Slice(0, _length);

        /// <summary>
        ///     Gets the unused portion of the buffer available for appending new characters.
        /// </summary>
        /// <remarks>
        ///     This span represents the free space after the current content.
        ///     Its length equals <see cref="Capacity" /> - <see cref="Length" />.
        /// </remarks>
        public readonly Span<T> Space => _buffer.Slice(_length);

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer.AsSpan()[index];
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     that wraps an existing span,
        ///     setting the initial length to the full length of the span.
        /// </summary>
        /// <param name="buffer">The underlying span to use as storage.</param>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStringBuilder([MustBePinned] Span<T> buffer)
        {
            _buffer = buffer;
            _array = default;
            _length = buffer.Length;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     that wraps an existing span,
        ///     with a specified initial length.
        /// </summary>
        /// <param name="buffer">The underlying span to use as storage.</param>
        /// <param name="length">
        ///     The initial number of elements considered in use.
        ///     Must be between 0 and <paramref name="buffer" /> length.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="length" /> is negative or exceeds
        ///     <paramref name="buffer" /> length.
        /// </exception>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStringBuilder([MustBePinned] Span<T> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, buffer.Length, ExceptionArgument.length);
            _buffer = buffer;
            _array = default;
            _length = length;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified capacity,
        /// </summary>
        /// <param name="capacity">
        ///     The initial storage capacity.
        ///     Must be non‑negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="capacity" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStringBuilder(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            _buffer = _array = new NativeArray<T>(capacity);
            _length = 0;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified capacity and initial length,
        /// </summary>
        /// <param name="capacity">The initial storage capacity. Must be non‑negative.</param>
        /// <param name="length">
        ///     The initial number of elements considered in use.
        ///     Must be between 0 and <paramref name="capacity" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="capacity" /> or <paramref name="length" /> is
        ///     negative, or if <paramref name="length" /> exceeds <paramref name="capacity" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStringBuilder(int capacity, int length)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, capacity, ExceptionArgument.length);
            _buffer = _array = new NativeArray<T>(capacity);
            _length = length;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _buffer = default;
            _length = 0;
            var array = _array;
            if (!array.IsCreated)
                return;
            _array = default;
            array.Dispose();
        }

        /// <summary>
        ///     Appends the string representation of a specified read-only character span to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ReadOnlySpan<T> buffer)
        {
            EnsureCapacity(_length + buffer.Length);
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref _buffer.GetPinnableReference(), (nint)_length)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * Unsafe.SizeOf<T>()));
            _length += buffer.Length;
        }

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its first occurrence.
        ///     If not found, returns -1.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(ReadOnlySpan<T> buffer) => Text.IndexOf(buffer);

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its last occurrence.
        ///     If not found, returns -1.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(ReadOnlySpan<T> buffer) => Text.LastIndexOf(buffer);

        /// <summary>
        ///     Searches for the first index of any of the specified values similar to calling IndexOf several times with the
        ///     logical OR operator.
        ///     If not found, returns -1.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOfAny(ReadOnlySpan<T> buffer) => Text.IndexOfAny(buffer);

        /// <summary>
        ///     Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the
        ///     logical OR operator.
        ///     If not found, returns -1.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOfAny(ReadOnlySpan<T> buffer) => Text.LastIndexOfAny(buffer);

        /// <summary>
        ///     Searches for the specified values and returns true if found.
        ///     If not found, returns false.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(ReadOnlySpan<T> buffer) => Text.IndexOf(buffer) >= 0;

        /// <summary>
        ///     Replaces all occurrences of a specified string in this instance with <see langword="null" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ReadOnlySpan<T> buffer) => Replace(buffer, (ReadOnlySpan<T>)Array.Empty<T>());

        /// <summary>
        ///     Inserts the sequence of characters into this instance at the specified character position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, ReadOnlySpan<T> buffer)
        {
            if ((uint)startIndex > (uint)_length)
                return false;
            EnsureCapacity(_length + buffer.Length);
            ref var reference = ref _buffer.GetPinnableReference();
            var count = _length - startIndex;
            if (count > 0)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)(startIndex + buffer.Length))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), (uint)(count * Unsafe.SizeOf<T>()));
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * Unsafe.SizeOf<T>()));
            _length += buffer.Length;
            return true;
        }

        /// <summary>
        ///     Replaces all occurrences of a specified string in this instance with another specified string.
        /// </summary>
        public bool Replace(ReadOnlySpan<T> oldValue, ReadOnlySpan<T> newValue)
        {
            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(oldValue)) || oldValue.IsEmpty)
                return false;

            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(newValue)))
            {
                if (newValue.Length != 0)
                    return false;
                newValue = (ReadOnlySpan<T>)Array.Empty<T>();
            }

            using var replacementIndices = new UnsafeListBuilder<int>(stackalloc int[128]);
            if (oldValue.Length == 1)
            {
                if (newValue.Length == 1)
                {
                    Replace(oldValue[0], newValue[0]);
                    return true;
                }

                var c = oldValue[0];
                var i = 0;

                while (true)
                {
                    var pos = AsReadOnlySpan(i).IndexOf(c);
                    if (pos < 0)
                        break;

                    replacementIndices.Append(i + pos);
                    i += pos + 1;
                }
            }
            else
            {
                var i = 0;
                while (true)
                {
                    var pos = AsReadOnlySpan(i).IndexOf(oldValue);
                    if (pos < 0)
                        break;

                    replacementIndices.Append(i + pos);
                    i += pos + oldValue.Length;
                }
            }

            if (replacementIndices.Length == 0)
                return true;

            var dst = ReplaceHelper(oldValue.Length, newValue, replacementIndices.AsReadOnlySpan());
            _length = dst.Length;
            if (dst.Length < Capacity)
            {
                dst.AsReadOnlySpan().CopyTo(AsSpan());
                dst.Dispose();
            }
            else
            {
                var array = _array;
                _buffer = _array = dst;
                if (array.IsCreated)
                    array.Dispose();
            }

            return true;
        }

        /// <summary>
        ///     Replaces all occurrences of a specified string in this instance with another specified string.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private readonly NativeArray<T> ReplaceHelper(int oldValueLength, ReadOnlySpan<T> newValue, ReadOnlySpan<int> indices)
        {
            var dstLength = Length + (long)(newValue.Length - oldValueLength) * indices.Length;
            ThrowHelpers.ThrowIfGreaterThan(dstLength, int.MaxValue, ExceptionArgument._dummy);
            var newCapacity = (int)dstLength <= Capacity ? (int)dstLength : BuilderHelpers.GrowCapacity(Capacity, (int)dstLength);
            var dst = new NativeArray<T>(newCapacity);
            var dstSpan = dst.AsSpan();
            var thisIdx = 0;
            var dstIdx = 0;
            for (var r = 0; r < indices.Length; ++r)
            {
                var replacementIdx = indices[r];
                var count = replacementIdx - thisIdx;
                if (count != 0)
                {
                    AsReadOnlySpan(thisIdx, count).CopyTo(dstSpan.Slice(dstIdx));
                    dstIdx += count;
                }

                thisIdx = replacementIdx + oldValueLength;
                newValue.CopyTo(dstSpan.Slice(dstIdx));
                dstIdx += newValue.Length;
            }

            AsReadOnlySpan(thisIdx).CopyTo(dstSpan.Slice(dstIdx));
            return dst;
        }

        /// <summary>
        ///     Determines whether the specified sequence appears at the start of the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(ReadOnlySpan<T> buffer) => Text.StartsWith(buffer);

        /// <summary>
        ///     Determines whether the specified sequence appears at the end of the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(ReadOnlySpan<T> buffer) => Text.EndsWith(buffer);

        /// <summary>
        ///     Determines the relative order of the sequences being compared by comparing the elements using
        ///     IComparable{T}.CompareTo(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Compare(ReadOnlySpan<T> buffer) => Text.SequenceCompareTo(buffer);

        /// <summary>
        ///     Appends the string representation of a specified object to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in T value)
        {
            EnsureCapacity(_length + 1);
            _buffer[_length++] = value;
        }

        /// <summary>
        ///     Appends a specified number of copies of the string representation of a specified object to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in T value, int repeatCount)
        {
            EnsureCapacity(_length + repeatCount);
            _buffer.Slice(_length, repeatCount).AsSpan().Fill(value);
            _length += repeatCount;
        }

        /// <summary>
        ///     Searches for the specified value and returns the index of its first occurrence.
        ///     If not found, returns -1.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in T value) => Text.IndexOf(value);

        /// <summary>
        ///     Searches for the specified value and returns the index of its last occurrence.
        ///     If not found, returns -1.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(in T value) => Text.LastIndexOf(value);

        /// <summary>
        ///     Searches for the specified value and returns true if found.
        ///     If not found, returns false.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in T value) => SpanHelpers.Contains(Text, value);

        /// <summary>
        ///     Replaces all occurrences of a specified string in this instance with <see langword="null" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(in T value)
        {
            ref var reference = ref _buffer.GetPinnableReference();
            var newLength = 0;
            for (var index = 0; index < _length; ++index)
            {
                var ch = Unsafe.Add(ref reference, (nint)index);
                if (!ch.Equals(value))
                    Unsafe.Add(ref reference, (nint)newLength++) = ch;
            }

            _length = newLength;
        }

        /// <summary>
        ///     Inserts the string representation of a specified object into this instance at the specified character position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, in T value)
        {
            if ((uint)startIndex > (uint)_length)
                return false;
            EnsureCapacity(_length + 1);
            ref var reference = ref _buffer.GetPinnableReference();
            var count = _length - startIndex;
            if (count > 0)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)(startIndex + 1))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), (uint)(count * Unsafe.SizeOf<T>()));
            _buffer[startIndex] = value;
            ++_length;
            return true;
        }

        /// <summary>
        ///     Replaces all occurrences of a specified string in this instance with another specified string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Replace(in T oldValue, in T newValue)
        {
#if NET8_0_OR_GREATER
            Text.Replace(oldValue, newValue);
#else
            ref var reference = ref _buffer.GetPinnableReference();
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, (nint)index);
                if (value.Equals(oldValue))
                    value = newValue;
            }
#endif
        }

        /// <summary>
        ///     Determines whether this string instance starts with the specified object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(in T value) => _length > 1 && _buffer[0].Equals(value);

        /// <summary>
        ///     Determines whether this string instance ends with the specified object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(in T value) => _length > 1 && _buffer[_length - 1].Equals(value);

        /// <summary>
        ///     Removes a range of characters from this builder.
        /// </summary>
        /// <remarks>
        ///     This method does not reduce the capacity of this builder.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int startIndex, int length)
        {
            if ((uint)startIndex > (uint)_length || (uint)length > (uint)(_length - startIndex))
                return false;
            if (length > 0)
            {
                ref var reference = ref _buffer.GetPinnableReference();
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)(startIndex + length))), (uint)((_length - startIndex - length) * Unsafe.SizeOf<T>()));
                _length -= length;
            }

            return true;
        }

        /// <summary>
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Reverse() => Text.Reverse();

        /// <summary>
        ///     Fills the contents of this span with the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Fill(in T value) => Text.Fill(value);

        /// <summary>
        ///     Removes all leading occurrences of a specified element from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimStart(in T value)
        {
            if (_length == 0)
                return;
            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            while (start < _length && Unsafe.Add(ref reference, (nint)start).Equals(value))
                start++;
            if (start > 0 && start < _length)
            {
                var count = _length - start;
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(count * Unsafe.SizeOf<T>()));
                _length = count;
            }
            else if (start >= _length)
            {
                _length = 0;
            }
        }

        /// <summary>
        ///     Removes all trailing white-space characters from the memory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimEnd(in T value)
        {
            if (_length == 0)
                return;
            ref var reference = ref _buffer.GetPinnableReference();
            var end = _length - 1;
            while (end >= 0 && Unsafe.Add(ref reference, (nint)end).Equals(value))
                end--;
            _length = end + 1;
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of a specified element from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim(in T value)
        {
            if (_length == 0)
                return;
            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            var end = _length - 1;
            while (start <= end && Unsafe.Add(ref reference, (nint)start).Equals(value))
                start++;
            while (end >= start && Unsafe.Add(ref reference, (nint)end).Equals(value))
                end--;
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(newLength * Unsafe.SizeOf<T>()));
            _length = newLength;
        }

        /// <summary>
        ///     Removes all leading occurrences of a set of elements specified
        ///     in a readonly span from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimStart(ReadOnlySpan<T> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.IsEmpty)
                return;
            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            while (start < _length && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, (nint)start)))
                start++;
            if (start > 0 && start < _length)
            {
                var count = _length - start;
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(count * Unsafe.SizeOf<T>()));
                _length = count;
            }
            else if (start >= _length)
            {
                _length = 0;
            }
        }

        /// <summary>
        ///     Removes all trailing occurrences of a set of elements specified
        ///     in a readonly span from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimEnd(ReadOnlySpan<T> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.IsEmpty)
                return;
            ref var reference = ref _buffer.GetPinnableReference();
            var end = _length - 1;
            while (end >= 0 && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, (nint)end)))
                end--;
            _length = end + 1;
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of a set of characters specified
        ///     in a readonly span from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim(ReadOnlySpan<T> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.IsEmpty)
                return;
            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            var end = _length - 1;
            while (start <= end && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, (nint)start)))
                start++;
            while (end >= start && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, (nint)end)))
                end--;
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(newLength * Unsafe.SizeOf<T>()));
            _length = newLength;
        }

        /// <summary>
        ///     Forms a slice out of the given span, beginning at 'start'.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> Substring(int start) => Text.Slice(start);

        /// <summary>
        ///     Forms a slice out of the given span, beginning at 'start', of given length
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> Substring(int start, int length) => Text.Slice(start, length);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _length = 0;

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(bool clear)
        {
            if (clear)
                _buffer.Clear();
            _length = 0;
        }

        /// <summary>
        ///     Attempts to set the length of this builder.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetLength(int length)
        {
            if ((uint)length > (uint)Capacity)
                return false;
            _length = length;
            return true;
        }

        /// <summary>
        ///     Attempts to advance the length of this builder.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Skip(int length)
        {
            var newLength = _length + length;
            if ((uint)newLength > (uint)Capacity)
                return false;
            _length = newLength;
            return true;
        }

        /// <summary>
        ///     Casts a instance of one primitive type to another primitive type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan());

        /// <summary>
        ///     Casts a instance of one primitive type to another primitive type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start) where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan(start));

        /// <summary>
        ///     Casts a instance of one primitive type to another primitive type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start, int length) where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan(start, length));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeStringBuilder<T> other) => Text.SequenceEqual(other.Text);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(ReadOnlySpan<T> buffer) => Text.SequenceEqual(buffer);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            if (typeof(T) == typeof(char))
                return UnsafeString.GetHashCode(MemoryMarshal.Cast<T, char>(Text));

#if NET10_0_OR_GREATER
            return NativeHashCode.GetHashCode(Text);
#else
            return NativeHashCode.GetHashCode((ReadOnlySpan<T>)Text);
#endif
        }

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString()
        {
            if (typeof(T) == typeof(char))
                return Text.ToString();
            if (typeof(T) == typeof(byte))
                return Encoding.UTF8.GetString(MemoryMarshal.AsBytes(Text));
            return SR.Format("UnsafeStringBuilder<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);
        }

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer) => Text.CopyTo(buffer);

        /// <summary>
        ///     Copies the contents of this span into destination span. If the source
        ///     and destinations overlap, this method behaves as if the original values in
        ///     a temporary location before the destination is overwritten.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> buffer) => Text.TryCopyTo(buffer);

        /// <summary>
        ///     Notifies this that <paramref name="count" /> data items were written to the output.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            var newLength = _length + count;
            ThrowHelpers.ThrowIfGreaterThan((uint)newLength, (uint)Capacity, ExceptionArgument.count);
            _length = newLength;
        }

        /// <summary>
        ///     Returns a <see cref="T:System.Span`1" /> to write to that is at least the requested size (specified by
        ///     <paramref name="sizeHint" />).
        /// </summary>
        /// <param name="sizeHint">The desired length for the slice (exclusive).</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="sizeHint" /> out of range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> GetSpan(int sizeHint = 0) => _buffer.Slice(_length, sizeHint);

        /// <summary>
        ///     Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified
        ///     character,
        ///     for a specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string,
        ///     equal to the number of original characters plus any additional padding characters.
        /// </param>
        /// <param name="padding">A padding character.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PadLeft(int totalWidth, in T padding)
        {
            EnsureCapacity(totalWidth);
            var num = totalWidth - _length;
            if (num <= 0)
                return;
            Text.CopyTo(_buffer.Slice(num));
            _buffer.Slice(0, num).AsSpan().Fill(padding);
            _length = totalWidth;
        }

        /// <summary>
        ///     Returns a new string that left-aligns the characters in this string by padding them on the right with a specified
        ///     character,
        ///     for a specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string,
        ///     equal to the number of original characters plus any additional padding characters.
        /// </param>
        /// <param name="padding">A padding character.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PadRight(int totalWidth, in T padding)
        {
            EnsureCapacity(totalWidth);
            var num = totalWidth - _length;
            if (num <= 0)
                return;
            _buffer.Slice(_length, num).AsSpan().Fill(padding);
            _length = totalWidth;
        }

        /// <summary>
        ///     Indicates whether the specified string is <see langword="null" /> or an empty string ("").
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNullOrEmpty() => Unsafe.IsNullRef(ref _buffer.GetPinnableReference()) || _length == 0;

        /// <summary>
        ///     Splits the source span using a single element as the delimiter.
        /// </summary>
        /// <param name="separator">The single element to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplit<T> Split(in T separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using a sequence of elements as the delimiter.
        /// </summary>
        /// <param name="separator">The element sequence to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplit<T> Split(ReadOnlySpan<T> separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using any of the specified single elements as delimiters.
        ///     The first occurrence of any element in the <paramref name="separator" /> set causes a split.
        ///     The resulting enumerator yields <see cref="ReadOnlySpan{T}" /> segments.
        /// </summary>
        /// <param name="separator">A set of single elements, any of which can act as a delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAny<T> SplitAny(ReadOnlySpan<T> separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using a single element as the delimiter, but returns the positional ranges
        ///     (<see cref="Range" />) of each segment instead of the segment slices themselves.
        ///     This is more lightweight when only positions are needed, allowing deferred slicing via <c>Text[range]</c>.
        /// </summary>
        /// <param name="separator">The single element to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<T> SplitRange(in T separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using a sequence of elements as the delimiter, but returns the positional ranges
        ///     (<see cref="Range" />) of each segment instead of the segment slices themselves.
        ///     The entire sequence must match for a split to occur.
        /// </summary>
        /// <param name="separator">The element sequence to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<T> SplitRange(ReadOnlySpan<T> separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using any of the specified single elements as delimiters, but returns the positional ranges
        ///     (<see cref="Range" />) of each segment instead of the segment slices themselves.
        ///     The first occurrence of any element in the <paramref name="separator" /> set causes a split.
        /// </summary>
        /// <param name="separator">A set of single elements, any of which can act as a delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAnyRange<T> SplitAnyRange(ReadOnlySpan<T> separator) => new(Text, separator);

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref UnsafeStringBuilder<T> AsRef() => ref Unsafe.AsRef(in this);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => Text;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => Text;

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (Capacity < capacity)
                Grow(capacity - Capacity);
            return Capacity;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(Capacity * 0.9);
            if (_length < threshold)
                SetCapacity(_length);
            return Capacity;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (capacity < _length || capacity >= Capacity)
                return Capacity;
            SetCapacity(capacity);
            return Capacity;
        }

        /// <summary>
        ///     Sets the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _length, ExceptionArgument.capacity);
            if (capacity != Capacity)
            {
                var destination = new NativeArray<T>(capacity);
                if (_length > 0)
                    _buffer.AsReadOnlySpan(0, _length).CopyTo(destination);
                var array = _array;
                _buffer = _array = destination;
                if (!array.IsCreated)
                    return;
                array.Dispose();
            }
        }

        /// <summary>
        ///     Increases the capacity of this to a new size
        ///     that is at least the specified minimum capacity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int additionalCapacityRequired)
        {
            var minimumLength = BuilderHelpers.GrowCapacity(Capacity, Capacity + additionalCapacityRequired);
            SetCapacity(minimumLength);
        }

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(UnsafeStringBuilder<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(UnsafeStringBuilder<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeStringBuilder<T> left, UnsafeStringBuilder<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeStringBuilder<T> left, UnsafeStringBuilder<T> right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeStringBuilder<T> left, ReadOnlySpan<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeStringBuilder<T> left, ReadOnlySpan<T> right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(ReadOnlySpan<T> left, UnsafeStringBuilder<T> right) => right.Equals(left);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(ReadOnlySpan<T> left, UnsafeStringBuilder<T> right) => !right.Equals(left);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeStringBuilder<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public readonly Span<T>.Enumerator GetEnumerator() => Text.GetEnumerator();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns a <see cref="T:System.Memory`1" /> to write to that is at least the requested size (specified by
        ///     <paramref name="sizeHint" />).
        /// </summary>
        /// <param name="sizeHint">The desired length for the slice (exclusive).</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="sizeHint" /> out of range (&lt;0 or &gt;Length).
        /// </exception>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly Memory<T> IBufferWriter<T>.GetMemory(int sizeHint)
        {
            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }
    }
}