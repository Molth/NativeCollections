using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
#if NET9_0_OR_GREATER
using System.Collections;
#endif

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9081 // A result of a stackalloc expression of this type in this context may be exposed outside of the containing method

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
    [IsReferenceOrContainsReferences]
    [IsAssignableTo(typeof(IIsCreated), typeof(IEquatable<>), typeof(IEquatable<>), typeof(IReadOnlyCollection<char>))]
    [Customizable("public static int GetHashCode(ReadOnlySpan<char> buffer)")]
    public unsafe ref struct UnsafeString
#if NET9_0_OR_GREATER
        : IIsCreated, IEquatable<UnsafeString>, IEquatable<ReadOnlySpan<char>>, IReadOnlyCollection<char>
#endif
    {
        /// <summary>
        ///     Default seed value used for hash code calculation.
        /// </summary>
        private static readonly ulong DefaultSeed = NativeRandom.NextU64();

        /// <summary>
        ///     Custom get hash code handler.
        /// </summary>
        private static delegate* managed<ReadOnlySpan<char>, int> _getHashCode;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly Span<char> _buffer;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        private int _length;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !Unsafe.IsNullRef(ref MemoryMarshal.GetReference(_buffer));

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
        public readonly Span<char> Buffer => _buffer;

        /// <summary>
        ///     Gets the portion of the buffer that contains the current string content.
        /// </summary>
        /// <remarks>
        ///     This span represents the characters that are currently considered part of the string.
        ///     Its length equals <see cref="Length" />.
        /// </remarks>
        public readonly Span<char> Text => _buffer.Slice(0, _length);

        /// <summary>
        ///     Gets the unused portion of the buffer available for appending new characters.
        /// </summary>
        /// <remarks>
        ///     This span represents the free space after the current content.
        ///     Its length equals <see cref="Capacity" /> - <see cref="Length" />.
        /// </remarks>
        public readonly Span<char> Space => _buffer.Slice(_length);

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[index];
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     that wraps an existing span,
        ///     setting the initial length to the full length of the span.
        /// </summary>
        /// <param name="buffer">The underlying span to use as storage.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString(Span<char> buffer)
        {
            _buffer = buffer;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeString(Span<char> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, buffer.Length, ExceptionArgument.length);
            _buffer = buffer;
            _length = length;
        }

        /// <summary>
        ///     Appends the string representation of a specified read-only character span to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Append(ReadOnlySpan<char> buffer)
        {
            if (_length + buffer.Length > Capacity)
                return false;
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), (nint)_length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * Unsafe.SizeOf<char>()));
            _length += buffer.Length;
            return true;
        }

        /// <summary>
        ///     Appends the default line terminator to the end of the current <see cref="T:System.Text.StringBuilder" /> object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendLine()
        {
            var newLine = NewLine;
            if (_length + newLine.Length > Capacity)
                return false;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)_length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * Unsafe.SizeOf<char>()));
            _length += newLine.Length;
            return true;
        }

        /// <summary>
        ///     Appends the specified interpolated string followed by the default line terminator to the end of the current.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendLine(ReadOnlySpan<char> buffer)
        {
            var newLine = NewLine;
            if (_length + buffer.Length + newLine.Length > Capacity)
                return false;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)_length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * Unsafe.SizeOf<char>()));
            _length += buffer.Length;
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)_length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * Unsafe.SizeOf<char>()));
            _length += newLine.Length;
            return true;
        }

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its first occurrence.
        ///     If not found, returns -1.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(ReadOnlySpan<char> buffer) => Text.IndexOf(buffer);

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its last occurrence.
        ///     If not found, returns -1.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(ReadOnlySpan<char> buffer) => Text.LastIndexOf(buffer);

        /// <summary>
        ///     Searches for the first index of any of the specified values similar to calling IndexOf several times with the
        ///     logical OR operator.
        ///     If not found, returns -1.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOfAny(ReadOnlySpan<char> buffer) => Text.IndexOfAny(buffer);

        /// <summary>
        ///     Searches for the last index of any of the specified values similar to calling LastIndexOf several times with the
        ///     logical OR operator.
        ///     If not found, returns -1.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOfAny(ReadOnlySpan<char> buffer) => Text.LastIndexOfAny(buffer);

        /// <summary>
        ///     Searches for the specified values and returns true if found.
        ///     If not found, returns false.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(ReadOnlySpan<char> buffer) => Text.IndexOf(buffer) >= 0;

        /// <summary>
        ///     Replaces all occurrences of a specified string in this instance with <see langword="null" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ReadOnlySpan<char> buffer) => Replace(buffer, "");

        /// <summary>
        ///     Inserts the sequence of characters into this instance at the specified character position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, ReadOnlySpan<char> buffer)
        {
            if ((uint)startIndex > (uint)_length || _length + buffer.Length > Capacity)
                return false;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var count = _length - startIndex;
            if (count > 0)
                SpanHelpers.Move(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)(startIndex + buffer.Length))), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), (uint)(count * Unsafe.SizeOf<char>()));
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * Unsafe.SizeOf<char>()));
            _length += buffer.Length;
            return true;
        }

        /// <summary>
        ///     Replaces all occurrences of a specified string in this instance with another specified string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue)
        {
            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(oldValue)) || oldValue.IsEmpty)
                return false;
            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(newValue)))
            {
                if (newValue.Length != 0)
                    return false;
                newValue = "";
            }

            UnsafeValueListBuilder<int> valueListBuilder;
            var elementOffset1 = 0;
            ref var local1 = ref MemoryMarshal.GetReference(_buffer);
            if (oldValue.Length == 1)
            {
                if (newValue.Length == 1)
                {
                    Replace(oldValue[0], newValue[0]);
                    return true;
                }

                valueListBuilder = new UnsafeValueListBuilder<int>(stackalloc int[128]);
                var ch = oldValue[0];
                while (true)
                {
                    var num = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref local1, (nint)elementOffset1), _length - elementOffset1).IndexOf(ch);
                    if (num >= 0)
                    {
                        valueListBuilder.Append(elementOffset1 + num);
                        elementOffset1 += num + 1;
                    }
                    else
                        break;
                }
            }
            else
            {
                valueListBuilder = new UnsafeValueListBuilder<int>(stackalloc int[128]);
                while (true)
                {
                    var num = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref local1, (nint)elementOffset1), _length - elementOffset1).IndexOf(oldValue);
                    if (num >= 0)
                    {
                        valueListBuilder.Append(elementOffset1 + num);
                        elementOffset1 += num + oldValue.Length;
                    }
                    else
                        break;
                }
            }

            if (valueListBuilder.IsEmpty)
                return true;
            var readOnlySpan = valueListBuilder.AsReadOnlySpan();
            var minimumLength = _length + (newValue.Length - oldValue.Length) * readOnlySpan.Length;
            if ((uint)minimumLength > (uint)Capacity)
            {
                valueListBuilder.Dispose();
                return false;
            }

            char[]? array = null;
            ref var local2 = ref MemoryMarshal.GetReference(minimumLength <= 256 ? stackalloc char[minimumLength] : (Span<char>)(array = ArrayPool<char>.Shared.Rent(minimumLength)));
            var elementOffset2 = 0;
            var elementOffset3 = 0;
            ref var local3 = ref MemoryMarshal.GetReference(newValue);
            for (var index = 0; index < readOnlySpan.Length; ++index)
            {
                var num1 = readOnlySpan[index];
                var num2 = num1 - elementOffset2;
                if (num2 != 0)
                {
                    SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local2, (nint)elementOffset3)), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local1, (nint)elementOffset2)), (uint)(num2 * 2));
                    elementOffset3 += num2;
                }

                elementOffset2 = num1 + oldValue.Length;
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local2, (nint)elementOffset3)), ref Unsafe.As<char, byte>(ref local3), (uint)(newValue.Length * 2));
                elementOffset3 += newValue.Length;
            }

            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local2, (nint)elementOffset3)), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local1, (nint)elementOffset2)), (uint)((_length - elementOffset2) * 2));
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref local1), ref Unsafe.As<char, byte>(ref local2), (uint)(minimumLength * 2));
            _length = minimumLength;
            valueListBuilder.Dispose();
            if (array != null)
                ArrayPool<char>.Shared.Return(array);
            return true;
        }

        /// <summary>
        ///     Determines whether the specified sequence appears at the start of the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(ReadOnlySpan<char> buffer) => Text.StartsWith(buffer);

        /// <summary>
        ///     Determines whether the specified sequence appears at the end of the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(ReadOnlySpan<char> buffer) => Text.EndsWith(buffer);

        /// <summary>
        ///     Determines the relative order of the sequences being compared by comparing the elements using
        ///     IComparable{T}.CompareTo(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Compare(ReadOnlySpan<char> buffer) => Text.SequenceCompareTo(buffer);

        /// <summary>
        ///     Appends the string representation of a specified object to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Append(char value)
        {
            if (_length + 1 > Capacity)
                return false;
            _buffer[_length++] = value;
            return true;
        }

        /// <summary>
        ///     Appends a specified number of copies of the string representation of a specified object to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Append(char value, int repeatCount)
        {
            if (_length + repeatCount > Capacity)
                return false;
            _buffer.Slice(_length, repeatCount).Fill(value);
            _length += repeatCount;
            return true;
        }

        /// <summary>
        ///     Appends the specified interpolated string followed by the default line terminator to the end of the current.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendLine(char value)
        {
            var newLine = NewLine;
            if (_length + 1 + newLine.Length > Capacity)
                return false;
            _buffer[_length++] = value;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)_length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * Unsafe.SizeOf<char>()));
            _length += newLine.Length;
            return true;
        }

        /// <summary>
        ///     Searches for the specified value and returns the index of its first occurrence.
        ///     If not found, returns -1.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(char value) => Text.IndexOf(value);

        /// <summary>
        ///     Searches for the specified value and returns the index of its last occurrence.
        ///     If not found, returns -1.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(char value) => Text.LastIndexOf(value);

        /// <summary>
        ///     Searches for the specified value and returns true if found.
        ///     If not found, returns false.
        ///     Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(char value) => SpanHelpers.Contains(Text, value);

        /// <summary>
        ///     Replaces all occurrences of a specified string in this instance with <see langword="null" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(char value)
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var newLength = 0;
            for (var index = 0; index < _length; ++index)
            {
                var ch = Unsafe.Add(ref reference, (nint)index);
                if (ch != value)
                    Unsafe.Add(ref reference, (nint)newLength++) = ch;
            }

            _length = newLength;
        }

        /// <summary>
        ///     Inserts the string representation of a specified object into this instance at the specified character position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, char value)
        {
            if ((uint)startIndex > (uint)_length || _length + 1 > Capacity)
                return false;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var count = _length - startIndex;
            if (count > 0)
                SpanHelpers.Move(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)(startIndex + 1))), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), (uint)(count * Unsafe.SizeOf<char>()));
            _buffer[startIndex] = value;
            ++_length;
            return true;
        }

        /// <summary>
        ///     Replaces all occurrences of a specified string in this instance with another specified string.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Replace(char oldValue, char newValue)
        {
#if NET8_0_OR_GREATER
            Text.Replace(oldValue, newValue);
#else
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, (nint)index);
                if (value == oldValue)
                    value = newValue;
            }
#endif
        }

        /// <summary>
        ///     Determines whether this string instance starts with the specified object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(char value) => _length > 1 && _buffer[0] == value;

        /// <summary>
        ///     Determines whether this string instance ends with the specified object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(char value) => _length > 1 && _buffer[_length - 1] == value;

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
                ref var reference = ref MemoryMarshal.GetReference(_buffer);
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)(startIndex + length))), (uint)((_length - startIndex - length) * Unsafe.SizeOf<char>()));
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
        public readonly void Fill(char value) => Text.Fill(value);

        /// <summary>
        ///     Removes all leading white-space characters from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimStart()
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            while (start < _length && char.IsWhiteSpace(Unsafe.Add(ref reference, (nint)start)))
                start++;
            if (start > 0 && start < _length)
            {
                var count = _length - start;
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(count * Unsafe.SizeOf<char>()));
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
        public void TrimEnd()
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var end = _length - 1;
            while (end >= 0 && char.IsWhiteSpace(Unsafe.Add(ref reference, (nint)end)))
                end--;
            _length = end + 1;
        }

        /// <summary>
        ///     Removes all leading and trailing white-space characters from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            var end = _length - 1;
            while (start <= end && char.IsWhiteSpace(Unsafe.Add(ref reference, (nint)start)))
                start++;
            while (end >= start && char.IsWhiteSpace(Unsafe.Add(ref reference, (nint)end)))
                end--;
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(newLength * Unsafe.SizeOf<char>()));
            _length = newLength;
        }

        /// <summary>
        ///     Removes all leading occurrences of a specified element from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimStart(char value)
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            while (start < _length && Unsafe.Add(ref reference, (nint)start) == value)
                start++;
            if (start > 0 && start < _length)
            {
                var count = _length - start;
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(count * Unsafe.SizeOf<char>()));
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
        public void TrimEnd(char value)
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var end = _length - 1;
            while (end >= 0 && Unsafe.Add(ref reference, (nint)end) == value)
                end--;
            _length = end + 1;
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of a specified element from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim(char value)
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            var end = _length - 1;
            while (start <= end && Unsafe.Add(ref reference, (nint)start) == value)
                start++;
            while (end >= start && Unsafe.Add(ref reference, (nint)end) == value)
                end--;
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(newLength * Unsafe.SizeOf<char>()));
            _length = newLength;
        }

        /// <summary>
        ///     Removes all leading occurrences of a set of elements specified
        ///     in a readonly span from the span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimStart(ReadOnlySpan<char> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.IsEmpty)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            while (start < _length && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, (nint)start)))
                start++;
            if (start > 0 && start < _length)
            {
                var count = _length - start;
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(count * Unsafe.SizeOf<char>()));
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
        public void TrimEnd(ReadOnlySpan<char> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.IsEmpty)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
        public void Trim(ReadOnlySpan<char> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.IsEmpty)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(newLength * Unsafe.SizeOf<char>()));
            _length = newLength;
        }

        /// <summary>
        ///     Forms a slice out of the given span, beginning at 'start'.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafeString Substring(int start) => new(Text.Slice(start));

        /// <summary>
        ///     Forms a slice out of the given span, beginning at 'start', of given length
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafeString Substring(int start, int length) => new(Text.Slice(start, length));

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
        public readonly Span<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<char, TTo>(AsSpan());

        /// <summary>
        ///     Casts a instance of one primitive type to another primitive type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start) where TTo : unmanaged => MemoryMarshal.Cast<char, TTo>(AsSpan(start));

        /// <summary>
        ///     Casts a instance of one primitive type to another primitive type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start, int length) where TTo : unmanaged => MemoryMarshal.Cast<char, TTo>(AsSpan(start, length));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeString other) => Text.SequenceEqual(other.Text);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <returns>Equals</returns>
        public readonly bool Equals(ReadOnlySpan<char> buffer) => Text.SequenceEqual(buffer);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <returns>Equals</returns>
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
        public readonly override int GetHashCode() => GetHashCode(Text);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => Text.ToString();

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<char> buffer) => Text.CopyTo(buffer);

        /// <summary>
        ///     Copies the contents of this span into destination span. If the source
        ///     and destinations overlap, this method behaves as if the original values in
        ///     a temporary location before the destination is overwritten.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<char> buffer) => Text.TryCopyTo(buffer);

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
        public readonly Span<char> GetSpan(int sizeHint = 0) => _buffer.Slice(_length, sizeHint);

        /// <summary>
        ///     Returns a copy of this string converted to lowercase.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToLower()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, (nint)index);
                value = char.ToLower(value);
            }
        }

        /// <summary>
        ///     Returns a copy of this string converted to lowercase, using the casing rules of the specified culture.
        /// </summary>
        /// <param name="culture">
        ///     An object that supplies culture-specific casing rules.
        ///     If <paramref name="culture" /> is <see langword="null" />, the current culture is used.
        /// </param>
        public readonly void ToLower(CultureInfo? culture)
        {
            var textInfo = (culture ?? CultureInfo.CurrentCulture).TextInfo;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, (nint)index);
                value = textInfo.ToLower(value);
            }
        }

        /// <summary>
        ///     Returns a copy of this string converted to lowercase using the casing rules of the invariant culture.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToLowerInvariant()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, (nint)index);
                value = char.ToLowerInvariant(value);
            }
        }

        /// <summary>
        ///     Returns a copy of this string converted to uppercase.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToUpper()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, (nint)index);
                value = char.ToUpper(value);
            }
        }

        /// <summary>
        ///     Returns a copy of this string converted to uppercase, using the casing rules of the specified culture.
        /// </summary>
        /// <param name="culture">
        ///     An object that supplies culture-specific casing rules.
        ///     If <paramref name="culture" /> is <see langword="null" />, the current culture is used.
        /// </param>
        public readonly void ToUpper(CultureInfo? culture)
        {
            var textInfo = (culture ?? CultureInfo.CurrentCulture).TextInfo;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, (nint)index);
                value = textInfo.ToUpper(value);
            }
        }

        /// <summary>
        ///     Returns a copy of this string converted to uppercase using the casing rules of the invariant culture.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToUpperInvariant()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, (nint)index);
                value = char.ToUpperInvariant(value);
            }
        }

        /// <summary>
        ///     Returns a new string that right-aligns the characters in this instance by padding them with spaces on the left,
        ///     for a specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string,
        ///     equal to the number of original characters plus any additional padding characters.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadLeft(int totalWidth) => PadLeft(totalWidth, ' ');

        /// <summary>
        ///     Returns a new string that right-aligns the characters in this instance by padding them on the left with a specified
        ///     character,
        ///     for a specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string,
        ///     equal to the number of original characters plus any additional padding characters.
        /// </param>
        /// <param name="paddingChar">A padding character.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadLeft(int totalWidth, char paddingChar)
        {
            if ((uint)totalWidth > (uint)Capacity)
                return false;
            var num = totalWidth - _length;
            if (num <= 0)
                return true;
            Text.CopyTo(_buffer.Slice(num));
            _buffer.Slice(0, num).Fill(paddingChar);
            _length = totalWidth;
            return true;
        }

        /// <summary>
        ///     Returns a new string that left-aligns the characters in this string by padding them with spaces on the right,
        ///     for a specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string,
        ///     equal to the number of original characters plus any additional padding characters.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadRight(int totalWidth) => PadRight(totalWidth, ' ');

        /// <summary>
        ///     Returns a new string that left-aligns the characters in this string by padding them on the right with a specified
        ///     character,
        ///     for a specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string,
        ///     equal to the number of original characters plus any additional padding characters.
        /// </param>
        /// <param name="paddingChar">A padding character.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadRight(int totalWidth, char paddingChar)
        {
            if ((uint)totalWidth > (uint)Capacity)
                return false;
            var num = totalWidth - _length;
            if (num <= 0)
                return true;
            _buffer.Slice(_length, num).Fill(paddingChar);
            _length = totalWidth;
            return true;
        }

        /// <summary>
        ///     Indicates whether a specified string is <see langword="null" />, empty, or consists only of white-space characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNullOrWhiteSpace()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            if (Unsafe.IsNullRef(ref reference))
                return true;
            for (var index = 0; index < _length; ++index)
            {
                if (!char.IsWhiteSpace(Unsafe.Add(ref reference, (nint)index)))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Indicates whether the specified string is <see langword="null" /> or an empty string ("").
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNullOrEmpty() => Unsafe.IsNullRef(ref MemoryMarshal.GetReference(_buffer)) || _length == 0;

        /// <summary>
        ///     Splits the source span using a single element as the delimiter.
        /// </summary>
        /// <param name="separator">The single element to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplit<char> Split(in char separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using a sequence of elements as the delimiter.
        /// </summary>
        /// <param name="separator">The element sequence to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplit<char> Split(ReadOnlySpan<char> separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using any of the specified single elements as delimiters.
        ///     The first occurrence of any element in the <paramref name="separator" /> set causes a split.
        ///     The resulting enumerator yields <see cref="ReadOnlySpan{T}" /> segments.
        /// </summary>
        /// <param name="separator">A set of single elements, any of which can act as a delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAny<char> SplitAny(ReadOnlySpan<char> separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using a single element as the delimiter, but returns the positional ranges
        ///     (<see cref="Range" />) of each segment instead of the segment slices themselves.
        ///     This is more lightweight when only positions are needed, allowing deferred slicing via <c>Text[range]</c>.
        /// </summary>
        /// <param name="separator">The single element to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<char> SplitRange(in char separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using a sequence of elements as the delimiter, but returns the positional ranges
        ///     (<see cref="Range" />) of each segment instead of the segment slices themselves.
        ///     The entire sequence must match for a split to occur.
        /// </summary>
        /// <param name="separator">The element sequence to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<char> SplitRange(ReadOnlySpan<char> separator) => new(Text, separator);

        /// <summary>
        ///     Splits the source span using any of the specified single elements as delimiters, but returns the positional ranges
        ///     (<see cref="Range" />) of each segment instead of the segment slices themselves.
        ///     The first occurrence of any element in the <paramref name="separator" /> set causes a split.
        /// </summary>
        /// <param name="separator">A set of single elements, any of which can act as a delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAnyRange<char> SplitAnyRange(ReadOnlySpan<char> separator) => new(Text, separator);

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref UnsafeString AsRef()
        {
#if NET9_0_OR_GREATER
            return ref Unsafe.AsRef(in this);
#else
            fixed (UnsafeString* ptr = &this)
            {
                return ref *ptr;
            }
#endif
        }

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafeString* AsPointer()
        {
#if NET9_0_OR_GREATER
            return UnsafeHelpers.AsPointer(ref Unsafe.AsRef(in this));
#else
            fixed (UnsafeString* ptr = &this)
            {
                return ptr;
            }
#endif
        }

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> AsSpan() => Text;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> AsSpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> AsSpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<char> AsReadOnlySpan() => Text;

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<char> AsReadOnlySpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<char> AsReadOnlySpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<char>(UnsafeString value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<char>(UnsafeString value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeString(Span<char> value) => new(value);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeString left, UnsafeString right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeString left, UnsafeString right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeString left, ReadOnlySpan<char> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeString left, ReadOnlySpan<char> right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(ReadOnlySpan<char> left, UnsafeString right) => right.Equals(left);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(ReadOnlySpan<char> left, UnsafeString right) => !right.Equals(left);

        /// <summary>
        ///     Gets the newline string defined for this environment.
        /// </summary>
        private static readonly char[] _NewLine = Environment.NewLine.ToCharArray();

        /// <summary>
        ///     Gets the newline string defined for this environment.
        /// </summary>
        private static readonly byte[] _NewLineUtf8 = Encoding.UTF8.GetBytes(Environment.NewLine);

        /// <summary>
        ///     Gets the all newline chars defined for this environment.
        /// </summary>
        private static readonly char[] _NewLineChars = "\r\f\u0085\u2028\u2029\n".ToCharArray();

        /// <summary>
        ///     Gets the all white space chars defined for this environment.
        /// </summary>
        private static readonly char[] _WhiteSpaceChars = "\t\n\v\f\r\u0020\u0085\u00a0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u2028\u2029\u202f\u205f\u3000".ToCharArray();

        /// <summary>
        ///     Gets the newline string defined for this environment.
        /// </summary>
        public static ReadOnlySpan<char> NewLine => _NewLine;

        /// <summary>
        ///     Gets the newline string defined for this environment.
        /// </summary>
        public static ReadOnlySpan<byte> NewLineUtf8 => _NewLineUtf8;

        /// <summary>
        ///     Gets the all newline chars defined for this environment.
        /// </summary>
        public static ReadOnlySpan<char> NewLineChars => _NewLineChars;

        /// <summary>
        ///     Gets the all white space chars defined for this environment.
        /// </summary>
        public static ReadOnlySpan<char> WhiteSpaceChars => _WhiteSpaceChars;

        /// <summary>
        ///     Configures custom get hash code handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<ReadOnlySpan<char>, int> getHashCode) => _getHashCode = getHashCode;

        /// <summary>
        ///     Diffuses the hash code returned by the specified chars.
        /// </summary>
        [Customizable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<char> buffer)
        {
            var getHashCode = _getHashCode;
            if (getHashCode != null)
                return getHashCode(buffer);

#if NET5_0_OR_GREATER
            return string.GetHashCode(buffer) + (int)DefaultSeed;
#else
            return MarvinHelpers.ComputeHash32(MemoryMarshal.AsBytes(buffer), DefaultSeed);
#endif
        }

        /// <summary>
        ///     Diffuses the hash code returned by the specified chars.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="charCount" /> is less than 0.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(char* ptr, int charCount)
        {
            ThrowHelpers.ThrowIfNegative(charCount, ExceptionArgument.charCount);
            return GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<char>(ptr), charCount));
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeString Create(ReadOnlySpan<char> buffer) => new(MemoryMarshalHelpers.AsSpan(buffer));

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeString Create(ReadOnlySpan<char> buffer, int length) => new(MemoryMarshalHelpers.AsSpan(buffer), length);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeString Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public readonly Span<char>.Enumerator GetEnumerator() => Text.GetEnumerator();

#if NET9_0_OR_GREATER
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
#endif

        /// <summary>
        ///     Appends the string returned by processing a composite format string,
        ///     which contains zero or more format items, to this instance.
        ///     Each format item is replaced by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormat<T>(T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : struct => !obj.HasValue || AppendFormat(obj.GetValueOrDefault(), format, provider);

        /// <summary>
        ///     Appends the string returned by processing a composite format string,
        ///     which contains zero or more format items, to this instance.
        ///     Each format item is replaced by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormat<T>(T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (FormatHelpers.TryFormat(obj, Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(bool obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            if (obj.TryFormat(Space, out var charsWritten))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Appends the string representation of a specified read-only character span to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(ref DefaultInterpolatedStringHandler handler, bool clear = true)
        {
#if NET10_0_OR_GREATER
            var result = Append(handler.Text);
            if (clear)
                handler.Clear();
            return result;
#else
            ReadOnlySpan<char> buffer = clear ? handler.ToStringAndClear() : handler.ToString();
            return Append(buffer);
#endif
        }

        /// <summary>
        ///     Appends the string representation of a specified read-only character span to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(IFormatProvider? provider, [InterpolatedStringHandlerArgument("provider")] ref DefaultInterpolatedStringHandler handler, bool clear = true)
        {
#if NET10_0_OR_GREATER
            var result = Append(handler.Text);
            if (clear)
                handler.Clear();
            return result;
#else
            ReadOnlySpan<char> buffer = clear ? handler.ToStringAndClear() : handler.ToString();
            return Append(buffer);
#endif
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable<T>(in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }
#else
        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(decimal obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(DateTime obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(byte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(DateTimeOffset obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(double obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(Guid obj, ReadOnlySpan<char> format = default, IFormatProvider? _ = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(Half obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }
#endif

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(short obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(int obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(long obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(sbyte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(float obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(TimeSpan obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(ushort obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(uint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(ulong obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) => Environment.Is64BitProcess ? AppendFormattable((long)obj, format, provider) : AppendFormattable((int)obj, format, provider);

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) => Environment.Is64BitProcess ? AppendFormattable((ulong)obj, format, provider) : AppendFormattable((uint)obj, format, provider);

        /// <summary>
        ///     Format the value of the current instance into the provided span of characters.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(Version obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            if (obj.TryFormat(Space, out var charsWritten))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }
#endif
    }
}