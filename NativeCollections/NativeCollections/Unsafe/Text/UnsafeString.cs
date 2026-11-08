using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     This class represents a mutable string. It is convenient for situations in which
    ///     it is desirable to modify a string, perhaps by removing, replacing, or inserting
    ///     characters, without creating a new String subsequent to each modification.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    [IsAssignableTo(typeof(IEquatable<>))]
    [Customizable("public static int GetHashCode(ReadOnlySpan<char> buffer)")]
    public unsafe struct UnsafeString : IIsCreated, IEquatable<UnsafeString>, IReadOnlyCollection<char>
#if NET9_0_OR_GREATER
        , IEquatable<ReadOnlySpan<char>>
#endif
    {
        /// <summary>
        ///     Custom get hash code handler.
        /// </summary>
        private static delegate* managed<ReadOnlySpan<char>, int> _getHashCode;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly NativeArray<char> _buffer;

        /// <summary>
        ///     Gets the number of characters in this.
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
        ///     Gets the number of characters in this.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Gets the number of characters contained in this.
        /// </summary>
        public readonly int Count => _length;

        /// <summary>
        ///     Gets the total number of characters the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _buffer.Length;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public readonly Span<char> Buffer => _buffer.AsSpan();

        /// <summary>
        ///     Gets the portion of the buffer that contains the current string content.
        /// </summary>
        /// <remarks>
        ///     This span represents the characters that are currently considered
        ///     part of the string. Its length equals <see cref="Length" />.
        /// </remarks>
        public readonly Span<char> Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.AsSpan().Slice(0, _length);
        }

        /// <summary>
        ///     Gets the unused portion of the buffer available for appending new characters.
        /// </summary>
        /// <remarks>
        ///     This span represents the free space after the current content.
        ///     Its length equals <see cref="Capacity" /> - <see cref="Length" />.
        /// </remarks>
        public readonly Span<char> Space
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.AsSpan().Slice(_length);
        }

        /// <summary>
        ///     Gets a reference to the character at the specified index.
        /// </summary>
        /// <param name="index">The zero-based character index within the underlying buffer.</param>
        /// <exception cref="IndexOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is outside the bounds of the underlying buffer.
        /// </exception>
        /// <remarks>
        ///     The index is validated against <see cref="Capacity" /> by the underlying <see cref="Span{T}" /> indexer
        ///     rather than against <see cref="Length" />; characters beyond the current text are therefore accessible.
        /// </remarks>
        public readonly ref char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer.AsSpan()[index];
        }

        /// <summary>
        ///     Initializes a new instance of this class that wraps an existing
        ///     span, setting the initial length to the full length of the span.
        /// </summary>
        /// <param name="buffer">The underlying span to use as storage.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeString([MustBePinned] Span<char> buffer)
        {
            _buffer = NativeArray<char>.From(buffer);
            _length = buffer.Length;
        }

        /// <summary>
        ///     Initializes a new instance of this class that wraps an existing span, with a specified initial length.
        /// </summary>
        /// <param name="buffer">The underlying span to use as storage.</param>
        /// <param name="length">
        ///     The initial number of characters considered in use.
        ///     Must be between 0 and <paramref name="buffer" /> length.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative or exceeds the length of <paramref name="buffer" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeString([MustBePinned] Span<char> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, buffer.Length, ExceptionArgument.length);
            _buffer = NativeArray<char>.From(buffer);
            _length = length;
        }

        /// <summary>
        ///     Appends the specified read-only character span to this instance.
        /// </summary>
        /// <param name="buffer">The characters to append.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Append(ReadOnlySpan<char> buffer)
        {
            if (I64.FromI32Unchecked(_length).WrappingAdd(I64.FromI32Unchecked(buffer.Length)) > I64.FromI32Unchecked(Capacity))
                return false;

            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref _buffer.GetPinnableReference(), Isize.FromI32Unchecked(_length))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(buffer.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            _length = _length.WrappingAdd(buffer.Length);
            return true;
        }

        /// <summary>
        ///     Appends the default line terminator to this instance.
        /// </summary>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        public bool AppendLine()
        {
            var newLine = NewLine;
            if (I64.FromI32Unchecked(_length).WrappingAdd(I64.FromI32Unchecked(newLine.Length)) > I64.FromI32Unchecked(Capacity))
                return false;

            ref var reference = ref _buffer.GetPinnableReference();
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(_length))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), Usize.FromI32Unchecked(newLine.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            _length = _length.WrappingAdd(newLine.Length);
            return true;
        }

        /// <summary>
        ///     Appends the specified character span followed by the default line terminator to this instance.
        /// </summary>
        /// <param name="buffer">The characters to append before the line terminator.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        public bool AppendLine(ReadOnlySpan<char> buffer)
        {
            var newLine = NewLine;
            if (I64.FromI32Unchecked(_length).WrappingAdd(I64.FromI32Unchecked(buffer.Length)).WrappingAdd(I64.FromI32Unchecked(newLine.Length)) > I64.FromI32Unchecked(Capacity))
                return false;

            ref var reference = ref _buffer.GetPinnableReference();
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(_length))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(buffer.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            _length = _length.WrappingAdd(buffer.Length);
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(_length))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), Usize.FromI32Unchecked(newLine.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            _length = _length.WrappingAdd(newLine.Length);
            return true;
        }

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its first occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>The zero-based index of the first occurrence of the specified sequence; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(ReadOnlySpan<char> buffer) => Text.IndexOf(buffer);

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its last occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>The zero-based index of the last occurrence of the specified sequence; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(ReadOnlySpan<char> buffer) => Text.LastIndexOf(buffer);

        /// <summary>
        ///     Searches for the first index of any of the specified values similar to calling
        ///     IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     The zero-based index of the first occurrence of any of the specified values; otherwise, -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOfAny(ReadOnlySpan<char> buffer) => Text.IndexOfAny(buffer);

        /// <summary>
        ///     Searches for the last index of any of the specified values similar to calling
        ///     LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     The zero-based index of the last occurrence of any of the specified values; otherwise, -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOfAny(ReadOnlySpan<char> buffer) => Text.LastIndexOfAny(buffer);

        /// <summary>
        ///     Searches for the specified values and returns <see langword="true" /> if found. If not
        ///     found, returns <see langword="false" />. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(ReadOnlySpan<char> buffer) => Text.IndexOf(buffer) >= 0;

        /// <summary>
        ///     Removes all occurrences of the specified character sequence from this instance.
        /// </summary>
        /// <param name="buffer">The character sequence to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ReadOnlySpan<char> buffer) => Replace(buffer, "");

        /// <summary>
        ///     Inserts the sequence of characters into this instance at the specified character position.
        /// </summary>
        /// <param name="startIndex">The zero-based index at which to start the operation.</param>
        /// <param name="buffer">The sequence to insert.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, [MustNotSelfAlias] ReadOnlySpan<char> buffer)
        {
            if (U32.FromI32Unchecked(startIndex) > U32.FromI32Unchecked(_length) || I64.FromI32Unchecked(_length).WrappingAdd(I64.FromI32Unchecked(buffer.Length)) > I64.FromI32Unchecked(Capacity))
                return false;

            ref var reference = ref _buffer.GetPinnableReference();
            var count = _length.WrappingSub(startIndex);
            if (count > 0)
                SpanHelpers.Move(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex.WrappingAdd(buffer.Length)))), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));

            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(buffer.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            _length = _length.WrappingAdd(buffer.Length);
            return true;
        }

        /// <summary>
        ///     Replaces all occurrences of the specified character sequence
        ///     in this instance with another specified character sequence.
        /// </summary>
        /// <param name="oldValue">The character sequence to replace.</param>
        /// <param name="newValue">The replacement character sequence.</param>
        /// <returns>
        ///     <see langword="true" /> if the replacement operation succeeded; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the result of the replacement would exceed <see cref="int.MaxValue" /> characters.
        /// </exception>
        [SkipLocalsInit]
        public bool Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue)
        {
            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(oldValue)) || oldValue.IsEmpty)
                return false;

            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(newValue)))
            {
                if (newValue.Length != 0)
                    return false;

                newValue = (ReadOnlySpan<char>)"";
            }

            using (var replacementIndices = UnsafeListBuilder<int>.From(stackalloc int[128]))
            {
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
                        var position = AsReadOnlySpan().Slice(i).IndexOf(c);
                        if (position < 0)
                            break;

                        replacementIndices.Append(i.WrappingAdd(position));
                        i = i.WrappingAdd(position.WrappingAdd(1));
                    }
                }
                else
                {
                    var i = 0;
                    while (true)
                    {
                        var position = AsReadOnlySpan().Slice(i).IndexOf(oldValue);
                        if (position < 0)
                            break;

                        replacementIndices.Append(i.WrappingAdd(position));
                        i = i.WrappingAdd(position.WrappingAdd(oldValue.Length));
                    }
                }

                if (replacementIndices.Length == 0)
                    return true;

                if (ReplaceHelper(oldValue.Length, newValue, replacementIndices.AsReadOnlySpan(), out var dst))
                {
                    _length = dst.Length;
                    dst.AsReadOnlySpan().CopyTo(AsSpan());
                    dst.Dispose();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Replaces all occurrences of the specified character sequence
        ///     in this instance with another specified character sequence.
        /// </summary>
        /// <param name="oldValueLength">The length of the sequence being replaced.</param>
        /// <param name="newValue">The sequence to replace each occurrence with.</param>
        /// <param name="indices">The starting indices of the occurrences to replace.</param>
        /// <param name="dst">
        ///     When this method returns, contains the newly allocated array that holds the
        ///     result, or the default value if the result exceeds <see cref="Capacity" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the result fits in <see cref="Capacity" /> and
        ///     was written to <paramref name="dst" />; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private readonly bool ReplaceHelper(int oldValueLength, ReadOnlySpan<char> newValue, ReadOnlySpan<int> indices, out NativeArray<char> dst)
        {
            var dstLength = I64.FromI32Unchecked(Length).WrappingAdd(I64.FromI32Unchecked(newValue.Length.WrappingSub(oldValueLength)).WrappingMul(I64.FromI32Unchecked(indices.Length)));
            ThrowHelpers.ThrowIfGreaterThan(dstLength, I64.FromI32Unchecked(int.MaxValue), ExceptionArgument._dummy);
            if (dstLength > I64.FromI32Unchecked(Capacity))
            {
                dst = default;
                return false;
            }

            dst = NativeArray<char>.Create(I32.FromI64Unchecked(dstLength));
            var dstSpan = dst.AsSpan();
            var thisIdx = 0;
            var dstIdx = 0;
            for (var r = 0; r < indices.Length; r = r.WrappingAdd(1))
            {
                var replacementIdx = indices[r];
                var count = replacementIdx.WrappingSub(thisIdx);
                if (count != 0)
                {
                    AsReadOnlySpan().Slice(thisIdx, count).CopyTo(dstSpan.Slice(dstIdx));
                    dstIdx = dstIdx.WrappingAdd(count);
                }

                thisIdx = replacementIdx.WrappingAdd(oldValueLength);
                newValue.CopyTo(dstSpan.Slice(dstIdx));
                dstIdx = dstIdx.WrappingAdd(newValue.Length);
            }

            AsReadOnlySpan().Slice(thisIdx).CopyTo(dstSpan.Slice(dstIdx));
            return true;
        }

        /// <summary>
        ///     Determines whether the specified sequence appears at the start of the span.
        /// </summary>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if this starts with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(ReadOnlySpan<char> buffer) => Text.StartsWith(buffer);

        /// <summary>
        ///     Determines whether the specified sequence appears at the end of the span.
        /// </summary>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if this ends with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(ReadOnlySpan<char> buffer) => Text.EndsWith(buffer);

        /// <summary>
        ///     Determines the relative order of the sequences being compared
        ///     by comparing the elements using IComparable{T}.CompareTo(T).
        /// </summary>
        /// <param name="buffer">The sequence to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the sequences.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Compare(ReadOnlySpan<char> buffer) => Text.SequenceCompareTo(buffer);

        /// <summary>
        ///     Appends the specified character to this instance.
        /// </summary>
        /// <param name="value">The character to append.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Append(char value)
        {
            if (_length >= Capacity)
                return false;

            _buffer[(_length = _length.WrappingAdd(1)).WrappingSub(1)] = value;
            return true;
        }

        /// <summary>
        ///     Appends the specified number of copies of the specified character to this instance.
        /// </summary>
        /// <param name="value">The character to append.</param>
        /// <param name="repeatCount">The number of times to append the character.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="repeatCount" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Append(char value, int repeatCount)
        {
            if (I64.FromI32Unchecked(_length).WrappingAdd(I64.FromI32Unchecked(repeatCount)) > I64.FromI32Unchecked(Capacity))
                return false;

            _buffer.Slice(_length, repeatCount).AsSpan().Fill(value);
            _length = _length.WrappingAdd(repeatCount);
            return true;
        }

        /// <summary>
        ///     Appends the specified character followed by the default line terminator to this instance.
        /// </summary>
        /// <param name="value">The character to append before the line terminator.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        public bool AppendLine(char value)
        {
            var newLine = NewLine;
            if (I64.FromI32Unchecked(_length).WrappingAdd(I64.FromI32Unchecked(1)).WrappingAdd(I64.FromI32Unchecked(newLine.Length)) > I64.FromI32Unchecked(Capacity))
                return false;

            _buffer[(_length = _length.WrappingAdd(1)).WrappingSub(1)] = value;
            ref var reference = ref _buffer.GetPinnableReference();
            SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(_length))), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), Usize.FromI32Unchecked(newLine.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
            _length = _length.WrappingAdd(newLine.Length);
            return true;
        }

        /// <summary>
        ///     Searches for the specified value and returns the index of its first occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="value">The character to search for.</param>
        /// <returns>The zero-based index of the first occurrence of the specified value; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(char value) => Text.IndexOf(value);

        /// <summary>
        ///     Searches for the specified value and returns the index of its last occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="value">The character to search for.</param>
        /// <returns>The zero-based index of the last occurrence of the specified value; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(char value) => Text.LastIndexOf(value);

        /// <summary>
        ///     Searches for the specified value and returns <see langword="true" /> if found. If not
        ///     found, returns <see langword="false" />. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="value">The character to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified value; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(char value) => SpanHelpers.Contains(Text, value);

        /// <summary>
        ///     Removes all occurrences of the specified character from this instance.
        /// </summary>
        /// <param name="value">The character to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(char value)
        {
            ref var reference = ref _buffer.GetPinnableReference();
            var newLength = 0;
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                var ch = Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                if (I32.FromCharUnchecked(ch) != I32.FromCharUnchecked(value))
                    Unsafe.Add(ref reference, Isize.FromI32Unchecked((newLength = newLength.WrappingAdd(1)).WrappingSub(1))) = ch;
            }

            _length = newLength;
        }

        /// <summary>
        ///     Inserts the specified character into this instance at the specified character position.
        /// </summary>
        /// <param name="startIndex">The zero-based index at which to start the operation.</param>
        /// <param name="value">The character to insert.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, char value)
        {
            if (U32.FromI32Unchecked(startIndex) > U32.FromI32Unchecked(_length) || _length >= Capacity)
                return false;

            ref var reference = ref _buffer.GetPinnableReference();
            var count = _length.WrappingSub(startIndex);
            if (count > 0)
                SpanHelpers.Move(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex.WrappingAdd(1)))), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));

            _buffer[startIndex] = value;
            _length = _length.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Replaces all occurrences of the specified character in this instance with another specified character.
        /// </summary>
        /// <param name="oldValue">The character to replace.</param>
        /// <param name="newValue">The replacement character.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Replace(char oldValue, char newValue)
        {
#if NET8_0_OR_GREATER
            Text.Replace(oldValue, newValue);
#else
            ref var reference = ref _buffer.GetPinnableReference();
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                ref var value = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                if (I32.FromCharUnchecked(value) == I32.FromCharUnchecked(oldValue))
                    value = newValue;
            }
#endif
        }

        /// <summary>
        ///     Determines whether this string instance starts with the specified object.
        /// </summary>
        /// <param name="value">The character to find at the start of this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this starts with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(char value) => _length > 0 && I32.FromCharUnchecked(_buffer[0]) == I32.FromCharUnchecked(value);

        /// <summary>
        ///     Determines whether this string instance ends with the specified object.
        /// </summary>
        /// <param name="value">The character to find at the end of this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this ends with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(char value) => _length > 0 && I32.FromCharUnchecked(_buffer[_length.WrappingSub(1)]) == I32.FromCharUnchecked(value);

        /// <summary>
        ///     Removes a range of characters from this instance.
        /// </summary>
        /// <param name="startIndex">The zero-based index at which to start the operation.</param>
        /// <param name="length">The number of characters to remove.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        ///     This method does not reduce the capacity of this instance.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int startIndex, int length)
        {
            if (U32.FromI32Unchecked(startIndex) > U32.FromI32Unchecked(_length) || U32.FromI32Unchecked(length) > U32.FromI32Unchecked(_length.WrappingSub(startIndex)))
                return false;

            if (length > 0)
            {
                ref var reference = ref _buffer.GetPinnableReference();
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex))), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex.WrappingAdd(length)))), Usize.FromI32Unchecked(_length.WrappingSub(startIndex).WrappingSub(length)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
                _length = _length.WrappingSub(length);
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
        /// <param name="value">The character to fill the span with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Fill(char value) => Text.Fill(value);

        /// <summary>
        ///     Removes all leading white-space characters from the span.
        /// </summary>
        public void TrimStart()
        {
            if (_length == 0)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            while (start < _length && char.IsWhiteSpace(Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))))
                start = start.WrappingAdd(1);

            if (start > 0 && start < _length)
            {
                var count = _length.WrappingSub(start);
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
                _length = count;
            }
            else if (start >= _length)
            {
                _length = 0;
            }
        }

        /// <summary>
        ///     Removes all trailing white-space characters from this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimEnd()
        {
            if (_length == 0)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var end = _length.WrappingSub(1);
            while (end >= 0 && char.IsWhiteSpace(Unsafe.Add(ref reference, Isize.FromI32Unchecked(end))))
                end = end.WrappingSub(1);

            _length = end.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes all leading and trailing white-space characters from the span.
        /// </summary>
        public void Trim()
        {
            if (_length == 0)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            var end = _length.WrappingSub(1);
            while (start <= end && char.IsWhiteSpace(Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))))
                start = start.WrappingAdd(1);

            while (end >= start && char.IsWhiteSpace(Unsafe.Add(ref reference, Isize.FromI32Unchecked(end))))
                end = end.WrappingSub(1);

            var newLength = end.WrappingSub(start).WrappingAdd(1);
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(newLength).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));

            _length = newLength;
        }

        /// <summary>
        ///     Removes all leading occurrences of a specified element from the span.
        /// </summary>
        /// <param name="value">The character to trim.</param>
        public void TrimStart(char value)
        {
            if (_length == 0)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            while (start < _length && I32.FromCharUnchecked(Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))) == I32.FromCharUnchecked(value))
                start = start.WrappingAdd(1);

            if (start > 0 && start < _length)
            {
                var count = _length.WrappingSub(start);
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
                _length = count;
            }
            else if (start >= _length)
            {
                _length = 0;
            }
        }

        /// <summary>
        ///     Removes all trailing white-space characters from this instance.
        /// </summary>
        /// <param name="value">The character to trim.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimEnd(char value)
        {
            if (_length == 0)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var end = _length.WrappingSub(1);
            while (end >= 0 && I32.FromCharUnchecked(Unsafe.Add(ref reference, Isize.FromI32Unchecked(end))) == I32.FromCharUnchecked(value))
                end = end.WrappingSub(1);

            _length = end.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of a specified element from the span.
        /// </summary>
        /// <param name="value">The character to trim.</param>
        public void Trim(char value)
        {
            if (_length == 0)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            var end = _length.WrappingSub(1);
            while (start <= end && I32.FromCharUnchecked(Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))) == I32.FromCharUnchecked(value))
                start = start.WrappingAdd(1);

            while (end >= start && I32.FromCharUnchecked(Unsafe.Add(ref reference, Isize.FromI32Unchecked(end))) == I32.FromCharUnchecked(value))
                end = end.WrappingSub(1);

            var newLength = end.WrappingSub(start).WrappingAdd(1);
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(newLength).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));

            _length = newLength;
        }

        /// <summary>
        ///     Removes all leading occurrences of a set of elements specified in a readonly span from the span.
        /// </summary>
        /// <param name="buffer">The characters to trim.</param>
        public void TrimStart(ReadOnlySpan<char> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.IsEmpty)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            while (start < _length && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))))
                start = start.WrappingAdd(1);

            if (start > 0 && start < _length)
            {
                var count = _length.WrappingSub(start);
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));
                _length = count;
            }
            else if (start >= _length)
            {
                _length = 0;
            }
        }

        /// <summary>
        ///     Removes all trailing occurrences of a set of elements specified in a readonly span from the span.
        /// </summary>
        /// <param name="buffer">The characters to trim.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimEnd(ReadOnlySpan<char> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.IsEmpty)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var end = _length.WrappingSub(1);
            while (end >= 0 && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, Isize.FromI32Unchecked(end))))
                end = end.WrappingSub(1);

            _length = end.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of a set of
        ///     characters specified in a readonly span from the span.
        /// </summary>
        /// <param name="buffer">The characters to trim.</param>
        public void Trim(ReadOnlySpan<char> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.IsEmpty)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            var end = _length.WrappingSub(1);
            while (start <= end && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))))
                start = start.WrappingAdd(1);

            while (end >= start && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, Isize.FromI32Unchecked(end))))
                end = end.WrappingSub(1);

            var newLength = end.WrappingSub(start).WrappingAdd(1);
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                SpanHelpers.Copy(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(newLength).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<char>())));

            _length = newLength;
        }

        /// <summary>
        ///     Returns a slice of this string beginning at <paramref name="start" />.
        /// </summary>
        /// <param name="start">The zero-based starting character index of the slice.</param>
        /// <returns>A slice of this beginning at <paramref name="start" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="start" /> is negative or greater than <see cref="Length" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafeString Substring(int start) => From(Text.Slice(start));

        /// <summary>
        ///     Returns a slice of this string beginning at <paramref name="start" />
        ///     with the specified <paramref name="length" />.
        /// </summary>
        /// <param name="start">The zero-based starting character index of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>
        ///     A slice of this beginning at <paramref name="start" /> with <paramref name="length" /> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="start" /> or <paramref name="length" /> is negative, or when
        ///     <paramref name="start" /> plus <paramref name="length" /> exceeds <see cref="Length" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafeString Substring(int start, int length) => From(Text.Slice(start, length));

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _length = 0;

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        /// <param name="clear">
        ///     <see langword="true" /> to clear the backing buffer; otherwise, <see langword="false" />.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(bool clear)
        {
            if (clear)
                _buffer.Clear();

            _length = 0;
        }

        /// <summary>
        ///     Attempts to set the length of this instance.
        /// </summary>
        /// <param name="length">The length to set.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetLength(int length)
        {
            if (U32.FromI32Unchecked(length) > U32.FromI32Unchecked(Capacity))
                return false;

            _length = length;
            return true;
        }

        /// <summary>
        ///     Attempts to advance the length of this instance.
        /// </summary>
        /// <param name="length">The number of characters.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Skip(int length)
        {
            var newLength = _length.WrappingAdd(length);
            if (U32.FromI32Unchecked(newLength) > U32.FromI32Unchecked(Capacity))
                return false;

            _length = newLength;
            return true;
        }

        /// <summary>
        ///     Casts an instance of one primitive type to another primitive type.
        /// </summary>
        /// <typeparam name="TTo">The type to reinterpret the characters as.</typeparam>
        /// <returns>A span over the characters currently considered part of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<char, TTo>(AsSpan());

        /// <summary>
        ///     Casts an instance of one primitive type to another primitive type.
        /// </summary>
        /// <typeparam name="TTo">The type to reinterpret the characters as.</typeparam>
        /// <param name="start">The starting element index.</param>
        /// <returns>
        ///     A span over the characters currently considered part of this, starting at <paramref name="start" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start) where TTo : unmanaged => MemoryMarshal.Cast<char, TTo>(AsSpan().Slice(start));

        /// <summary>
        ///     Casts an instance of one primitive type to another primitive type.
        /// </summary>
        /// <typeparam name="TTo">The type to reinterpret the characters as.</typeparam>
        /// <param name="start">The starting element index.</param>
        /// <param name="length">The number of elements to include in the span.</param>
        /// <returns>A span over the specified portion of the characters currently considered part of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start, int length) where TTo : unmanaged => MemoryMarshal.Cast<char, TTo>(AsSpan().Slice(start, length));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(UnsafeString other) => Text.SequenceEqual(other.Text);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="buffer">The characters to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ReadOnlySpan<char> buffer) => Text.SequenceEqual(buffer);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
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
        /// <returns>The hash code for this instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => GetHashCode(Text);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => Text.ToString();

        /// <summary>
        ///     Copies all characters from this into a destination span. The span must
        ///     have a length at least equal to the current number of characters in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all characters are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the characters of this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<char> buffer) => Text.CopyTo(buffer);

        /// <summary>
        ///     Copies the contents of this string into the destination span. If the source
        ///     and destination overlap, this method behaves as if the original characters
        ///     were copied to a temporary location before the destination is overwritten.
        /// </summary>
        /// <param name="buffer">The destination span to which all characters are copied.</param>
        /// <returns>
        ///     <see langword="true" /> if the contents were copied to
        ///     <paramref name="buffer" />; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<char> buffer) => Text.TryCopyTo(buffer);

        /// <summary>
        ///     Advances the length of this by <paramref name="count" /> characters.
        /// </summary>
        /// <param name="count">The number of characters by which to advance the length.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="count" /> is negative or
        ///     would make the length exceed <see cref="Capacity" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            var newLength = _length.WrappingAdd(count);
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(newLength), U32.FromI32Unchecked(Capacity), ExceptionArgument.count);
            _length = newLength;
        }

        /// <summary>
        ///     Returns a span starting at the current length of this with the specified size.
        /// </summary>
        /// <param name="sizeHint">The length of the returned span.</param>
        /// <returns>A span starting at the current length of this with <paramref name="sizeHint" /> elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sizeHint" /> is negative
        ///     or does not fit in the unused portion of the buffer.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> GetSpan(int sizeHint = 0) => _buffer.AsSpan().Slice(_length, sizeHint);

        /// <summary>
        ///     Converts the characters in this instance to lowercase.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToLower()
        {
            ref var reference = ref _buffer.AsSpan().GetPinnableReference();
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                ref var value = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                value = char.ToLower(value);
            }
        }

        /// <summary>
        ///     Converts the characters in this instance to lowercase, using the casing rules of the specified culture.
        /// </summary>
        /// <param name="culture">
        ///     An object that supplies culture-specific casing rules. If
        ///     <paramref name="culture" /> is <see langword="null" />, the current culture is used.
        /// </param>
        public readonly void ToLower(CultureInfo? culture)
        {
            var textInfo = (culture ?? CultureInfo.CurrentCulture).TextInfo;
            ref var reference = ref _buffer.GetPinnableReference();
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                ref var value = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                value = textInfo.ToLower(value);
            }
        }

        /// <summary>
        ///     Converts the characters in this instance to lowercase using the casing rules of the invariant culture.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToLowerInvariant()
        {
            ref var reference = ref _buffer.GetPinnableReference();
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                ref var value = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                value = char.ToLowerInvariant(value);
            }
        }

        /// <summary>
        ///     Converts the characters in this instance to uppercase.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToUpper()
        {
            ref var reference = ref _buffer.GetPinnableReference();
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                ref var value = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                value = char.ToUpper(value);
            }
        }

        /// <summary>
        ///     Converts the characters in this instance to uppercase, using the casing rules of the specified culture.
        /// </summary>
        /// <param name="culture">
        ///     An object that supplies culture-specific casing rules. If
        ///     <paramref name="culture" /> is <see langword="null" />, the current culture is used.
        /// </param>
        public readonly void ToUpper(CultureInfo? culture)
        {
            var textInfo = (culture ?? CultureInfo.CurrentCulture).TextInfo;
            ref var reference = ref _buffer.GetPinnableReference();
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                ref var value = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                value = textInfo.ToUpper(value);
            }
        }

        /// <summary>
        ///     Converts the characters in this instance to uppercase using the casing rules of the invariant culture.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void ToUpperInvariant()
        {
            ref var reference = ref _buffer.GetPinnableReference();
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                ref var value = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                value = char.ToUpperInvariant(value);
            }
        }

        /// <summary>
        ///     Pads this instance on the left with spaces to reach the specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The total length of the padded instance.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the instance was padded or already
        ///     had the requested length; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadLeft(int totalWidth) => PadLeft(totalWidth, ' ');

        /// <summary>
        ///     Pads this instance on the left with the specified character to reach the specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The total length of the padded instance.
        /// </param>
        /// <param name="paddingChar">The character with which to pad the instance.</param>
        /// <returns>
        ///     <see langword="true" /> if the instance was padded or already
        ///     had the requested length; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadLeft(int totalWidth, char paddingChar)
        {
            if (U32.FromI32Unchecked(totalWidth) > U32.FromI32Unchecked(Capacity))
                return false;

            var num = totalWidth.WrappingSub(_length);
            if (num <= 0)
                return true;

            Text.CopyTo(_buffer.AsSpan().Slice(num));
            _buffer.AsSpan().Slice(0, num).Fill(paddingChar);
            _length = totalWidth;
            return true;
        }

        /// <summary>
        ///     Pads this instance on the right with spaces to reach the specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The total length of the padded instance.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the instance was padded or already
        ///     had the requested length; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadRight(int totalWidth) => PadRight(totalWidth, ' ');

        /// <summary>
        ///     Pads this instance on the right with the specified character to reach the specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The total length of the padded instance.
        /// </param>
        /// <param name="paddingChar">The character with which to pad the instance.</param>
        /// <returns>
        ///     <see langword="true" /> if the instance was padded or already
        ///     had the requested length; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadRight(int totalWidth, char paddingChar)
        {
            if (U32.FromI32Unchecked(totalWidth) > U32.FromI32Unchecked(Capacity))
                return false;

            var num = totalWidth.WrappingSub(_length);
            if (num <= 0)
                return true;

            _buffer.Slice(_length, num).AsSpan().Fill(paddingChar);
            _length = totalWidth;
            return true;
        }

        /// <summary>
        ///     Indicates whether this is uninitialized, empty, or consists only of white-space characters.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if this is uninitialized, empty, or consists
        ///     only of white-space characters; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNullOrWhiteSpace()
        {
            ref var reference = ref _buffer.GetPinnableReference();
            if (Unsafe.IsNullRef(ref reference))
                return true;

            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                if (!char.IsWhiteSpace(Unsafe.Add(ref reference, Isize.FromI32Unchecked(index))))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Indicates whether this is uninitialized or contains no characters.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if this is uninitialized or
        ///     contains no characters; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNullOrEmpty() => Unsafe.IsNullRef(ref _buffer.GetPinnableReference()) || _length == 0;

        /// <summary>
        ///     Splits the source span using a single element as the delimiter.
        /// </summary>
        /// <param name="separator">The single element to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplit<char> Split(char separator) => NativeSplit<char>.From(ref MemoryMarshal.GetReference(Text), Text.Length, separator);

        /// <summary>
        ///     Splits the source span using a sequence of elements as the delimiter.
        /// </summary>
        /// <param name="separator">The element sequence to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplit<char> Split(ReadOnlySpan<char> separator) => NativeSplit<char>.From(ref MemoryMarshal.GetReference(Text), Text.Length, ref MemoryMarshal.GetReference(separator), separator.Length);

        /// <summary>
        ///     Splits the source span using any of the specified single elements as delimiters.
        ///     The first occurrence of any element in the <paramref name="separator" /> set causes
        ///     a split. The resulting enumerator yields <see cref="ReadOnlySpan{T}" /> segments.
        /// </summary>
        /// <param name="separator">A set of single elements, any of which can act as a delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAny<char> SplitAny(ReadOnlySpan<char> separator) => NativeSplitAny<char>.From(ref MemoryMarshal.GetReference(Text), Text.Length, ref MemoryMarshal.GetReference(separator), separator.Length);

        /// <summary>
        ///     Splits the source span using a single element as the delimiter, but returns the positional ranges
        ///     (<see cref="Range" />) of each segment instead of the segment slices themselves. This is more
        ///     lightweight when only positions are needed, allowing deferred slicing via <c>Text[range]</c>.
        /// </summary>
        /// <param name="separator">The single element to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<char> SplitRange(char separator) => NativeSplitRange<char>.From(ref MemoryMarshal.GetReference(Text), Text.Length, separator);

        /// <summary>
        ///     Splits the source span using a sequence of elements as the delimiter, but returns
        ///     the positional ranges (<see cref="Range" />) of each segment instead of the
        ///     segment slices themselves. The entire sequence must match for a split to occur.
        /// </summary>
        /// <param name="separator">The element sequence to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<char> SplitRange(ReadOnlySpan<char> separator) => NativeSplitRange<char>.From(ref MemoryMarshal.GetReference(Text), Text.Length, ref MemoryMarshal.GetReference(separator), separator.Length);

        /// <summary>
        ///     Splits the source span using any of the specified single elements as delimiters, but returns the
        ///     positional ranges (<see cref="Range" />) of each segment instead of the segment slices themselves.
        ///     The first occurrence of any element in the <paramref name="separator" /> set causes a split.
        /// </summary>
        /// <param name="separator">A set of single elements, any of which can act as a delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAnyRange<char> SplitAnyRange(ReadOnlySpan<char> separator) => NativeSplitAnyRange<char>.From(ref MemoryMarshal.GetReference(Text), Text.Length, ref MemoryMarshal.GetReference(separator), separator.Length);

        /// <summary>
        ///     Returns a reference to this instance.
        /// </summary>
        /// <returns>A reference to this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref UnsafeString AsRef() => ref Unsafe.AsRef(in this);

        /// <summary>
        ///     Returns a span over the characters currently considered part of this.
        /// </summary>
        /// <returns>A span over the characters currently considered part of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> AsSpan() => Text;

        /// <summary>
        ///     Returns a read-only span over the characters currently considered part of this.
        /// </summary>
        /// <returns>A read-only span over the characters currently considered part of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<char> AsReadOnlySpan() => Text;

        /// <summary>
        ///     Converts the specified span to an <see cref="UnsafeString" />.
        /// </summary>
        /// <param name="value">The span to wrap; its full length is considered in use.</param>
        /// <returns>A string that wraps <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeString From(Span<char> value) => new(value);

        /// <summary>
        ///     Creates a string view over the specified span with the given initial length.
        /// </summary>
        /// <param name="buffer">The underlying span to use as storage.</param>
        /// <param name="length">The initial number of characters considered in use.</param>
        /// <returns>
        ///     A string that wraps <paramref name="buffer" /> with
        ///     <paramref name="length" /> characters considered in use.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeString From([MustBePinned] Span<char> buffer, int length) => new(buffer, length);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeString left, UnsafeString right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeString left, UnsafeString right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeString left, ReadOnlySpan<char> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeString left, ReadOnlySpan<char> right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(ReadOnlySpan<char> left, UnsafeString right) => right.Equals(left);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
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
        ///     Gets all the newline chars defined for this environment.
        /// </summary>
        private static readonly char[] _NewLineChars = "\r\f\u0085\u2028\u2029\n".ToCharArray();

        /// <summary>
        ///     Gets all the white space chars defined for this environment.
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
        ///     Gets all the newline chars defined for this environment.
        /// </summary>
        public static ReadOnlySpan<char> NewLineChars => _NewLineChars;

        /// <summary>
        ///     Gets all the white space chars defined for this environment.
        /// </summary>
        public static ReadOnlySpan<char> WhiteSpaceChars => _WhiteSpaceChars;

        /// <summary>
        ///     Configures custom get hash code handler.
        /// </summary>
        /// <param name="getHashCode">The function to use to compute hash codes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<ReadOnlySpan<char>, int> getHashCode) => _getHashCode = getHashCode;

        /// <summary>
        ///     Diffuses the hash code returned by the specified chars.
        /// </summary>
        /// <param name="buffer">The characters to hash.</param>
        /// <returns>The hash code for the specified characters.</returns>
        [Customizable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<char> buffer)
        {
            var getHashCode = _getHashCode;
            if (getHashCode != null)
                return getHashCode(buffer);

            return NativeHashCode.GetHashCode(buffer);
        }

        /// <summary>
        ///     Diffuses the hash code returned by the specified chars.
        /// </summary>
        /// <param name="ptr">A pointer to the characters to hash.</param>
        /// <param name="charCount">The number of characters to hash.</param>
        /// <returns>The hash code for the specified characters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="charCount" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(char* ptr, int charCount)
        {
            ThrowHelpers.ThrowIfNegative(charCount, ExceptionArgument.charCount);
            return GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<char>(ptr), charCount));
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public readonly Span<char>.Enumerator GetEnumerator() => Text.GetEnumerator();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="obj" /> is <see langword="null" /> or the formatted
        ///     value was appended; otherwise, <see langword="false" /> if the remaining space is insufficient.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormat<T>(T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : struct => !obj.HasValue || AppendFormat(obj.GetValueOrDefault(), format, provider);

        /// <summary>
        ///     Appends the string returned by processing a composite format string, which
        ///     contains zero or more format items, to this instance. Each format item is replaced
        ///     by the string representation of a corresponding argument in a parameter span.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the formatted value was appended; otherwise,
        ///     <see langword="false" /> if the remaining space is insufficient.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormat<T>(T? obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (FormatHelpers.TryFormat(obj, Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="_">The format string, which is ignored by this overload.</param>
        /// <param name="__">The format provider, which is ignored by this overload.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(bool obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            if (obj.TryFormat(Space, out var charsWritten))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Appends the formatted string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="handler">The interpolated string handler to append.</param>
        /// <param name="clear">
        ///     <see langword="true" /> to clear the handler after appending; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the formatted value was appended; otherwise, <see langword="false" />.
        /// </returns>
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
        ///     Appends the formatted string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        /// <param name="clear">
        ///     <see langword="true" /> to clear the handler after appending; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the formatted value was appended; otherwise, <see langword="false" />.
        /// </returns>
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
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable<T>(T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }
#else
        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(decimal obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(DateTime obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(byte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(DateTimeOffset obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(double obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="_">The format provider, which is ignored by this overload.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(Guid obj, ReadOnlySpan<char> format = default, IFormatProvider? _ = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(Half obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }
#endif

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(short obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(int obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(long obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(sbyte obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(float obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(TimeSpan obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(ushort obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(uint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(ulong obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
        {
            if (obj.TryFormat(Space, out var charsWritten, format, provider))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) => Environment.Is64BitProcess ? AppendFormattable(I64.FromIsizeUnchecked(obj), format, provider) : AppendFormattable(I32.FromIsizeUnchecked(obj), format, provider);

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="format">The format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) => Environment.Is64BitProcess ? AppendFormattable(U64.FromUsizeUnchecked(obj), format, provider) : AppendFormattable(U32.FromUsizeUnchecked(obj), format, provider);

        /// <summary>
        ///     Appends the string representation of the specified value to this instance.
        /// </summary>
        /// <param name="obj">The value to format.</param>
        /// <param name="_">The format string, which is ignored by this overload.</param>
        /// <param name="__">The format provider, which is ignored by this overload.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was successfully
        ///     formatted and appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(Version obj, ReadOnlySpan<char> _ = default, IFormatProvider? __ = null)
        {
            if (obj.TryFormat(Space, out var charsWritten))
            {
                _length = _length.WrappingAdd(charsWritten);
                return true;
            }

            return false;
        }
#endif
    }
}