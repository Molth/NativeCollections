using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     This struct represents a mutable sequence of values. It is convenient for situations
    ///     in which it is desirable to modify a sequence, perhaps by removing, replacing, or
    ///     inserting values, without creating a new sequence subsequent to each modification.
    /// </summary>
    /// <typeparam name="T">The unmanaged type of the elements in the builder.</typeparam>
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
        ///     Gets the number of elements in this.
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
        ///     Gets the number of elements in this.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _length;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _buffer.Length;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public readonly Span<T> Buffer => _buffer.AsSpan();

        /// <summary>
        ///     Gets the portion of the buffer that contains the current sequence content.
        /// </summary>
        /// <remarks>
        ///     This span represents the values that are currently considered
        ///     part of the sequence. Its length equals <see cref="Length" />.
        /// </remarks>
        public readonly Span<T> Text
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.AsSpan().Slice(0, _length);
        }

        /// <summary>
        ///     Gets the unused portion of the buffer available for appending new values.
        /// </summary>
        /// <remarks>
        ///     This span represents the free space after the current content.
        ///     Its length equals <see cref="Capacity" /> - <see cref="Length" />.
        /// </remarks>
        public readonly Span<T> Space
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.AsSpan().Slice(_length);
        }

        /// <summary>
        ///     Gets a reference to the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to access.</param>
        /// <exception cref="IndexOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is outside the bounds of the underlying buffer.
        /// </exception>
        /// <remarks>
        ///     The index is validated against <see cref="Capacity" /> by the underlying
        ///     <see cref="Span{T}" /> indexer rather than against <see cref="Count" />.
        /// </remarks>
        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer.AsSpan()[index];
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="buffer">The underlying buffer used as storage.</param>
        /// <param name="array">
        ///     The array that owns the storage, or a default value when the storage is externally provided.
        /// </param>
        /// <param name="length">The number of elements in use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeStringBuilder(NativeArray<T> buffer, NativeArray<T> array, int length)
        {
            _buffer = buffer;
            _array = array;
            _length = length;
        }

        /// <summary>
        ///     Creates a builder over the specified span with the full span length in use.
        /// </summary>
        /// <param name="buffer">The underlying span to use as storage.</param>
        /// <returns>
        ///     A builder that wraps <paramref name="buffer" /> with the full span length considered in use.
        /// </returns>
        /// <remarks>
        ///     If the builder grows, it allocates separate storage and stops referring to the original span.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeStringBuilder<T> From([MustBePinned] Span<T> buffer) => new(NativeArray<T>.From(buffer), default, buffer.Length);

        /// <summary>
        ///     Creates a builder over the specified span with the given initial length.
        /// </summary>
        /// <param name="buffer">The underlying span to use as storage.</param>
        /// <param name="length">The initial number of elements considered in use.</param>
        /// <returns>
        ///     A builder that wraps <paramref name="buffer" /> with
        ///     <paramref name="length" /> elements considered in use.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative or greater than the length of
        ///     <paramref name="buffer" />.
        /// </exception>
        /// <remarks>
        ///     If the builder grows, it allocates separate storage and stops referring to the original span.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeStringBuilder<T> From([MustBePinned] Span<T> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, buffer.Length, ExceptionArgument.length);
            return new UnsafeStringBuilder<T>(NativeArray<T>.From(buffer), default, length);
        }

        /// <summary>
        ///     Creates a builder with the specified capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial storage capacity. Must be non‑negative.
        /// </param>
        /// <returns>A newly allocated builder with an empty sequence.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeStringBuilder<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var array = NativeArray<T>.Create(capacity);
            return new UnsafeStringBuilder<T>(array, array, 0);
        }

        /// <summary>
        ///     Creates a builder with the specified capacity and initial length.
        /// </summary>
        /// <param name="capacity">The initial storage capacity. Must be non‑negative.</param>
        /// <param name="length">
        ///     The initial number of elements considered in use. Must be between 0 and <paramref name="capacity" />.
        /// </param>
        /// <returns>A newly allocated builder containing the specified initial sequence length.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> or <paramref name="length" /> is
        ///     negative or <paramref name="length" /> exceeds <paramref name="capacity" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeStringBuilder<T> Create(int capacity, int length)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, capacity, ExceptionArgument.length);
            var array = NativeArray<T>.Create(capacity);
            return new UnsafeStringBuilder<T>(array, array, length);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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
        ///     Appends the specified read-only span to this instance.
        /// </summary>
        /// <param name="buffer">The values to append to this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append([MustNotSelfAlias] ReadOnlySpan<T> buffer)
        {
            EnsureCapacity(_length.WrappingAdd(buffer.Length));
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref _buffer.GetPinnableReference(), Isize.FromI32Unchecked(_length))), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(buffer.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
            _length = _length.WrappingAdd(buffer.Length);
        }

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its first occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>The zero-based index of the first occurrence of the specified sequence; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(ReadOnlySpan<T> buffer) => Text.IndexOf(buffer);

        /// <summary>
        ///     Searches for the specified sequence and returns the index of its last occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="buffer">The sequence to search for.</param>
        /// <returns>The zero-based index of the last occurrence of the specified sequence; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(ReadOnlySpan<T> buffer) => Text.LastIndexOf(buffer);

        /// <summary>
        ///     Searches for the first index of any of the specified values similar to calling
        ///     IndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="buffer">The values to search for.</param>
        /// <returns>
        ///     The zero-based index of the first occurrence of any of the specified values; otherwise, -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOfAny(ReadOnlySpan<T> buffer) => Text.IndexOfAny(buffer);

        /// <summary>
        ///     Searches for the last index of any of the specified values similar to calling
        ///     LastIndexOf several times with the logical OR operator. If not found, returns -1.
        /// </summary>
        /// <param name="buffer">The values to search for.</param>
        /// <returns>
        ///     The zero-based index of the last occurrence of any of the specified values; otherwise, -1.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOfAny(ReadOnlySpan<T> buffer) => Text.LastIndexOfAny(buffer);

        /// <summary>
        ///     Searches for the specified values and returns <see langword="true" /> if found. If not
        ///     found, returns <see langword="false" />. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="buffer">The values to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(ReadOnlySpan<T> buffer) => Text.IndexOf(buffer) >= 0;

        /// <summary>
        ///     Removes all occurrences of the specified sequence from this instance.
        /// </summary>
        /// <param name="buffer">The sequence to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ReadOnlySpan<T> buffer) => Replace(buffer, (ReadOnlySpan<T>)Array.Empty<T>());

        /// <summary>
        ///     Inserts the specified sequence into this instance at the specified position.
        /// </summary>
        /// <param name="startIndex">The zero-based index at which to insert the buffer.</param>
        /// <param name="buffer">The sequence to insert.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, [MustNotSelfAlias] ReadOnlySpan<T> buffer)
        {
            if (U32.FromI32Unchecked(startIndex) > U32.FromI32Unchecked(_length))
                return false;

            EnsureCapacity(_length.WrappingAdd(buffer.Length));
            ref var reference = ref _buffer.GetPinnableReference();
            var count = _length.WrappingSub(startIndex);
            if (count > 0)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex.WrappingAdd(buffer.Length)))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex))), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(buffer.Length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
            _length = _length.WrappingAdd(buffer.Length);
            return true;
        }

        /// <summary>
        ///     Replaces all occurrences of the specified sequence in this instance with another specified sequence.
        /// </summary>
        /// <param name="oldValue">The sequence to replace.</param>
        /// <param name="newValue">The replacement sequence.</param>
        /// <returns>
        ///     <see langword="true" /> if the replacement operation succeeded; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the result of the replacement would exceed <see cref="int.MaxValue" /> characters.
        /// </exception>
        [SkipLocalsInit]
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

                var dst = ReplaceHelper(oldValue.Length, newValue, replacementIndices.AsReadOnlySpan(), out var dstLength);
                _length = dstLength;
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
        }

        /// <summary>
        ///     Replaces all occurrences of the specified sequence in this instance with another specified sequence.
        /// </summary>
        /// <param name="oldValueLength">The length of the sequence being replaced.</param>
        /// <param name="newValue">The sequence to replace each occurrence with.</param>
        /// <param name="indices">The starting indices of the occurrences to replace.</param>
        /// <param name="dstLength">When this method returns, contains the number of elements in the result.</param>
        /// <returns>
        ///     A newly allocated array that holds the result and whose length is <paramref name="dstLength" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private readonly NativeArray<T> ReplaceHelper(int oldValueLength, ReadOnlySpan<T> newValue, ReadOnlySpan<int> indices, out int dstLength)
        {
            var dstLength64 = I64.FromI32Unchecked(Length).WrappingAdd(I64.FromI32Unchecked(newValue.Length.WrappingSub(oldValueLength)).WrappingMul(I64.FromI32Unchecked(indices.Length)));
            ThrowHelpers.ThrowIfGreaterThan(dstLength64, I64.FromI32Unchecked(int.MaxValue), ExceptionArgument._dummy);
            dstLength = I32.FromI64Unchecked(dstLength64);
            var newCapacity = dstLength <= Capacity ? dstLength : BuilderHelpers.GrowCapacity(Capacity, dstLength);
            var dst = NativeArray<T>.Create(newCapacity);
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
            return dst;
        }

        /// <summary>
        ///     Determines whether the specified sequence appears at the start of the span.
        /// </summary>
        /// <param name="buffer">The sequence to compare with this.</param>
        /// <returns>
        ///     <see langword="true" /> if this starts with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(ReadOnlySpan<T> buffer) => Text.StartsWith(buffer);

        /// <summary>
        ///     Determines whether the specified sequence appears at the end of the span.
        /// </summary>
        /// <param name="buffer">The sequence to compare with this.</param>
        /// <returns>
        ///     <see langword="true" /> if this ends with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(ReadOnlySpan<T> buffer) => Text.EndsWith(buffer);

        /// <summary>
        ///     Determines the relative order of the sequences being compared
        ///     by comparing the elements using IComparable{T}.CompareTo(T).
        /// </summary>
        /// <param name="buffer">The sequence to compare with this.</param>
        /// <returns>A value that indicates the relative order of the sequences.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Compare(ReadOnlySpan<T> buffer) => Text.SequenceCompareTo(buffer);

        /// <summary>
        ///     Appends the specified value to this instance.
        /// </summary>
        /// <param name="value">The value to append to this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(T value)
        {
            EnsureCapacity(_length.WrappingAdd(1));
            _buffer[(_length = _length.WrappingAdd(1)).WrappingSub(1)] = value;
        }

        /// <summary>
        ///     Appends the specified number of copies of the specified value to this instance.
        /// </summary>
        /// <param name="value">The value to append to this.</param>
        /// <param name="repeatCount">The number of times to append the value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="repeatCount" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(T value, int repeatCount)
        {
            ThrowHelpers.ThrowIfNegative(repeatCount, ExceptionArgument.repeatCount);
            EnsureCapacity(_length.WrappingAdd(repeatCount));
            _buffer.Slice(_length, repeatCount).AsSpan().Fill(value);
            _length = _length.WrappingAdd(repeatCount);
        }

        /// <summary>
        ///     Searches for the specified value and returns the index of its first occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>The zero-based index of the first occurrence of the specified value; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T value) => Text.IndexOf(value);

        /// <summary>
        ///     Searches for the specified value and returns the index of its last occurrence.
        ///     If not found, returns -1. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>The zero-based index of the last occurrence of the specified value; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(T value) => Text.LastIndexOf(value);

        /// <summary>
        ///     Searches for the specified value and returns <see langword="true" /> if found. If not
        ///     found, returns <see langword="false" />. Values are compared using IEquatable{T}.Equals(T).
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified value; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(T value) => SpanHelpers.Contains(Text, value);

        /// <summary>
        ///     Removes all occurrences of the specified value from this instance.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T value)
        {
            ref var reference = ref _buffer.GetPinnableReference();
            var newLength = 0;
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                var ch = Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                if (!ch.Equals(value))
                    Unsafe.Add(ref reference, Isize.FromI32Unchecked((newLength = newLength.WrappingAdd(1)).WrappingSub(1))) = ch;
            }

            _length = newLength;
        }

        /// <summary>
        ///     Inserts the specified value into this instance at the specified position.
        /// </summary>
        /// <param name="startIndex">The zero-based index at which to insert the value.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, T value)
        {
            if (U32.FromI32Unchecked(startIndex) > U32.FromI32Unchecked(_length))
                return false;

            EnsureCapacity(_length.WrappingAdd(1));
            ref var reference = ref _buffer.GetPinnableReference();
            var count = _length.WrappingSub(startIndex);
            if (count > 0)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex.WrappingAdd(1)))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

            _buffer[startIndex] = value;
            _length = _length.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Replaces all occurrences of the specified value in this instance with another specified value.
        /// </summary>
        /// <param name="oldValue">The value to replace.</param>
        /// <param name="newValue">The replacement value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Replace(T oldValue, T newValue)
        {
#if NET8_0_OR_GREATER
            Text.Replace(oldValue, newValue);
#else
            ref var reference = ref _buffer.GetPinnableReference();
            for (var index = 0; index < _length; index = index.WrappingAdd(1))
            {
                ref var value = ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index));
                if (value.Equals(oldValue))
                    value = newValue;
            }
#endif
        }

        /// <summary>
        ///     Determines whether this sequence starts with the specified value.
        /// </summary>
        /// <param name="value">The value to compare with this.</param>
        /// <returns>
        ///     <see langword="true" /> if this starts with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(T value) => _length > 0 && _buffer[0].Equals(value);

        /// <summary>
        ///     Determines whether this sequence ends with the specified value.
        /// </summary>
        /// <param name="value">The value to compare with this.</param>
        /// <returns>
        ///     <see langword="true" /> if this ends with the specified sequence; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(T value) => _length > 0 && _buffer[_length.WrappingSub(1)].Equals(value);

        /// <summary>
        ///     Removes a range of elements from this builder.
        /// </summary>
        /// <param name="startIndex">The zero-based index at which to start removing elements.</param>
        /// <param name="length">The number of elements to remove.</param>
        /// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        ///     This method does not reduce the capacity of this builder.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int startIndex, int length)
        {
            if (U32.FromI32Unchecked(startIndex) > U32.FromI32Unchecked(_length) || U32.FromI32Unchecked(length) > U32.FromI32Unchecked(_length.WrappingSub(startIndex)))
                return false;

            if (length > 0)
            {
                ref var reference = ref _buffer.GetPinnableReference();
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(startIndex.WrappingAdd(length)))), Usize.FromI32Unchecked(_length.WrappingSub(startIndex).WrappingSub(length)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
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
        /// <param name="value">The value with which to fill this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Fill(T value) => Text.Fill(value);

        /// <summary>
        ///     Removes all leading occurrences of a specified element from the span.
        /// </summary>
        /// <param name="value">The value to remove from the beginning of this.</param>
        public void TrimStart(T value)
        {
            if (_length == 0)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            while (start < _length && Unsafe.Add(ref reference, Isize.FromI32Unchecked(start)).Equals(value))
                start = start.WrappingAdd(1);

            if (start > 0 && start < _length)
            {
                var count = _length.WrappingSub(start);
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
                _length = count;
            }
            else if (start >= _length)
            {
                _length = 0;
            }
        }

        /// <summary>
        ///     Removes all trailing occurrences of the specified value from this builder.
        /// </summary>
        /// <param name="value">The value to remove from the end of this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimEnd(T value)
        {
            if (_length == 0)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var end = _length.WrappingSub(1);
            while (end >= 0 && Unsafe.Add(ref reference, Isize.FromI32Unchecked(end)).Equals(value))
                end = end.WrappingSub(1);

            _length = end.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes all leading and trailing occurrences of a specified element from the span.
        /// </summary>
        /// <param name="value">The value to remove from the beginning and end of this.</param>
        public void Trim(T value)
        {
            if (_length == 0)
                return;

            ref var reference = ref _buffer.GetPinnableReference();
            var start = 0;
            var end = _length.WrappingSub(1);
            while (start <= end && Unsafe.Add(ref reference, Isize.FromI32Unchecked(start)).Equals(value))
                start = start.WrappingAdd(1);

            while (end >= start && Unsafe.Add(ref reference, Isize.FromI32Unchecked(end)).Equals(value))
                end = end.WrappingSub(1);

            var newLength = end.WrappingSub(start).WrappingAdd(1);
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(newLength).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

            _length = newLength;
        }

        /// <summary>
        ///     Removes all leading occurrences of a set of elements specified in a readonly span from the span.
        /// </summary>
        /// <param name="buffer">The values to remove from the beginning of this.</param>
        public void TrimStart(ReadOnlySpan<T> buffer)
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
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
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
        /// <param name="buffer">The values to remove from the end of this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimEnd(ReadOnlySpan<T> buffer)
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
        ///     Removes all leading and trailing occurrences of values specified in a readonly span from this builder.
        /// </summary>
        /// <param name="buffer">The values to remove from the beginning and end of this.</param>
        public void Trim(ReadOnlySpan<T> buffer)
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
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(start))), Usize.FromI32Unchecked(newLength).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

            _length = newLength;
        }

        /// <summary>
        ///     Returns a slice of this sequence beginning at <paramref name="start" />.
        /// </summary>
        /// <param name="start">The zero-based starting element index of the slice.</param>
        /// <returns>A slice of this beginning at <paramref name="start" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> Substring(int start) => Text.Slice(start);

        /// <summary>
        ///     Returns a slice of this sequence beginning at <paramref name="start" />
        ///     with the specified <paramref name="length" />.
        /// </summary>
        /// <param name="start">The zero-based starting element index of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns>
        ///     A slice of this beginning at <paramref name="start" /> with <paramref name="length" /> elements.
        /// </returns>
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
        /// <param name="clear">
        ///     <see langword="true" /> to clear the buffer before resetting
        ///     the length; otherwise, <see langword="false" />.
        /// </param>
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
        ///     Attempts to advance the length of this builder.
        /// </summary>
        /// <param name="length">The number of elements by which to advance the length.</param>
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
        /// <typeparam name="TTo">The type to reinterpret the elements as.</typeparam>
        /// <returns>A span over the characters currently considered part of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan());

        /// <summary>
        ///     Casts an instance of one primitive type to another primitive type.
        /// </summary>
        /// <typeparam name="TTo">The type to reinterpret the elements as.</typeparam>
        /// <param name="start">The starting element index.</param>
        /// <returns>
        ///     A span over the characters currently considered part of this, starting at <paramref name="start" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start) where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan().Slice(start));

        /// <summary>
        ///     Casts an instance of one primitive type to another primitive type.
        /// </summary>
        /// <typeparam name="TTo">The type to reinterpret the elements as.</typeparam>
        /// <param name="start">The starting element index.</param>
        /// <param name="length">The number of elements to include in the span.</param>
        /// <returns>A span over the specified portion of the characters currently considered part of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start, int length) where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan().Slice(start, length));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(UnsafeStringBuilder<T> other) => Text.SequenceEqual(other.Text);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="buffer">The span to compare with this.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ReadOnlySpan<T> buffer) => Text.SequenceEqual(buffer);

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
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString()
        {
            if (typeof(T) == typeof(char))
                return Text.ToString();

            if (typeof(T) == typeof(byte))
                return Encoding.UTF8.GetString(MemoryMarshal.AsBytes(Text));

            return SR.Format("UnsafeStringBuilder<{0}>[{1}]", SR.GetTypeName(typeof(T)), _length);
        }

        /// <summary>
        ///     Copies all elements from this into a destination span. The span must
        ///     have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer) => Text.CopyTo(buffer);

        /// <summary>
        ///     Copies the contents of this span into destination span. If the source and destinations overlap, this
        ///     method behaves as if the original values in a temporary location before the destination is overwritten.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <returns><see langword="true" /> if all elements were copied; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> buffer) => Text.TryCopyTo(buffer);

        /// <summary>
        ///     Advances the length of this by <paramref name="count" /> elements.
        /// </summary>
        /// <param name="count">The number of elements by which to advance the length.</param>
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
        public readonly Span<T> GetSpan(int sizeHint = 0) => _buffer.AsSpan().Slice(_length, sizeHint);

        /// <summary>
        ///     Pads this sequence on the left with the specified value to reach the specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The total length of the padded sequence.
        /// </param>
        /// <param name="padding">The value with which to pad the sequence.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="totalWidth" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PadLeft(int totalWidth, T padding)
        {
            EnsureCapacity(totalWidth);
            var num = totalWidth.WrappingSub(_length);
            if (num <= 0)
                return;

            Text.CopyTo(_buffer.AsSpan().Slice(num));
            _buffer.AsSpan().Slice(0, num).Fill(padding);
            _length = totalWidth;
        }

        /// <summary>
        ///     Pads this sequence on the right with the specified value to reach the specified total length.
        /// </summary>
        /// <param name="totalWidth">
        ///     The total length of the padded sequence.
        /// </param>
        /// <param name="padding">The value with which to pad the sequence.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="totalWidth" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PadRight(int totalWidth, T padding)
        {
            EnsureCapacity(totalWidth);
            var num = totalWidth.WrappingSub(_length);
            if (num <= 0)
                return;

            _buffer.Slice(_length, num).AsSpan().Fill(padding);
            _length = totalWidth;
        }

        /// <summary>
        ///     Indicates whether this is uninitialized or contains no elements.
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
        public readonly NativeSplit<T> Split(T separator) => NativeSplit<T>.From(ref MemoryMarshal.GetReference(Text), Text.Length, separator);

        /// <summary>
        ///     Splits the source span using a sequence of elements as the delimiter.
        /// </summary>
        /// <param name="separator">The element sequence to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplit<T> Split(ReadOnlySpan<T> separator) => NativeSplit<T>.From(ref MemoryMarshal.GetReference(Text), Text.Length, ref MemoryMarshal.GetReference(separator), separator.Length);

        /// <summary>
        ///     Splits the source span using any of the specified single elements as delimiters.
        ///     The first occurrence of any element in the <paramref name="separator" /> set causes
        ///     a split. The resulting enumerator yields <see cref="ReadOnlySpan{T}" /> segments.
        /// </summary>
        /// <param name="separator">A set of single elements, any of which can act as a delimiter.</param>
        /// <returns>An enumerator that enumerates the split segments as <see cref="ReadOnlySpan{T}" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAny<T> SplitAny(ReadOnlySpan<T> separator) => NativeSplitAny<T>.From(ref MemoryMarshal.GetReference(Text), Text.Length, ref MemoryMarshal.GetReference(separator), separator.Length);

        /// <summary>
        ///     Splits the source span using a single element as the delimiter, but returns the positional ranges
        ///     (<see cref="Range" />) of each segment instead of the segment slices themselves. This is more
        ///     lightweight when only positions are needed, allowing deferred slicing via <c>Text[range]</c>.
        /// </summary>
        /// <param name="separator">The single element to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<T> SplitRange(T separator) => NativeSplitRange<T>.From(ref MemoryMarshal.GetReference(Text), Text.Length, separator);

        /// <summary>
        ///     Splits the source span using a sequence of elements as the delimiter, but returns
        ///     the positional ranges (<see cref="Range" />) of each segment instead of the
        ///     segment slices themselves. The entire sequence must match for a split to occur.
        /// </summary>
        /// <param name="separator">The element sequence to use as the delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<T> SplitRange(ReadOnlySpan<T> separator) => NativeSplitRange<T>.From(ref MemoryMarshal.GetReference(Text), Text.Length, ref MemoryMarshal.GetReference(separator), separator.Length);

        /// <summary>
        ///     Splits the source span using any of the specified single elements as delimiters, but returns the
        ///     positional ranges (<see cref="Range" />) of each segment instead of the segment slices themselves.
        ///     The first occurrence of any element in the <paramref name="separator" /> set causes a split.
        /// </summary>
        /// <param name="separator">A set of single elements, any of which can act as a delimiter.</param>
        /// <returns>An enumerator that enumerates the <see cref="Range" /> of each segment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAnyRange<T> SplitAnyRange(ReadOnlySpan<T> separator) => NativeSplitAnyRange<T>.From(ref MemoryMarshal.GetReference(Text), Text.Length, ref MemoryMarshal.GetReference(separator), separator.Length);

        /// <summary>
        ///     Returns a reference to this instance.
        /// </summary>
        /// <returns>A reference to this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref UnsafeStringBuilder<T> AsRef() => ref Unsafe.AsRef(in this);

        /// <summary>
        ///     Returns a span over the elements currently considered part of this.
        /// </summary>
        /// <returns>A span over the characters currently considered part of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => Text;

        /// <summary>
        ///     Returns a read-only span over the elements currently considered part of this.
        /// </summary>
        /// <returns>A read-only span over the characters currently considered part of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => Text;

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than the specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (Capacity < capacity)
                Grow(capacity.WrappingSub(Capacity));

            return Capacity;
        }

        /// <summary>
        ///     Reduces the capacity of this to its current length when
        ///     the current length is less than 90 percent of the capacity.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = I32.FromF64Unchecked(F64.FromI32Unchecked(Capacity) * 0.9);
            if (_length < threshold)
                SetCapacity(_length);

            return Capacity;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
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
        ///     Sets the capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than the current length.
        /// </exception>
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _length, ExceptionArgument.capacity);
            if (capacity != Capacity)
            {
                var destination = NativeArray<T>.Create(capacity);
                if (_length > 0)
                    _buffer.AsReadOnlySpan().Slice(0, _length).CopyTo(destination.AsSpan());

                var array = _array;
                _buffer = _array = destination;
                if (!array.IsCreated)
                    return;

                array.Dispose();
            }
        }

        /// <summary>
        ///     Increases the capacity of this to a new size that is at least the specified minimum capacity.
        /// </summary>
        /// <param name="additionalCapacityRequired">The number of additional elements that must fit in this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int additionalCapacityRequired)
        {
            var minimumLength = BuilderHelpers.GrowCapacity(Capacity, Capacity.WrappingAdd(additionalCapacityRequired));
            SetCapacity(minimumLength);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeStringBuilder<T> left, UnsafeStringBuilder<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeStringBuilder<T> left, UnsafeStringBuilder<T> right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeStringBuilder<T> left, ReadOnlySpan<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeStringBuilder<T> left, ReadOnlySpan<T> right) => !left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(ReadOnlySpan<T> left, UnsafeStringBuilder<T> right) => right.Equals(left);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(ReadOnlySpan<T> left, UnsafeStringBuilder<T> right) => !right.Equals(left);

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
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
        ///     Returns a <see cref="Memory{T}" /> to write to that is at least
        ///     the requested size (specified by <paramref name="sizeHint" />).
        /// </summary>
        /// <param name="sizeHint">The desired length for the slice (exclusive).</param>
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