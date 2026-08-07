using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
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
    ///     This class represents a mutable string.  It is convenient for situations in
    ///     which it is desirable to modify a string, perhaps by removing, replacing, or
    ///     inserting characters, without creating a new String subsequent to
    ///     each modification.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    [IsReferenceOrContainsReferences]
    [BindingType(typeof(ArrayPool<>))]
    [IsAssignableTo(typeof(IIsCreated), typeof(IDisposable), typeof(IEquatable<>), typeof(IEquatable<>), typeof(IReadOnlyCollection<>), typeof(IBufferWriter<>))]
    public unsafe ref struct UnsafeStringBuilder<T>
#if NET9_0_OR_GREATER
        : IIsCreated, IDisposable, IEquatable<UnsafeStringBuilder<T>>, IEquatable<ReadOnlySpan<T>>, IReadOnlyCollection<T>, IBufferWriter<T>
#endif
        where T : unmanaged, IComparable<T>, IEquatable<T>
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private Span<T> _buffer;

        /// <summary>
        ///     Array
        /// </summary>
        private T[]? _array;

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
        ///     Buffer
        /// </summary>
        public readonly Span<T> Buffer => _buffer;

        /// <summary>
        ///     Text
        /// </summary>
        public readonly Span<T> Text => _buffer.Slice(0, _length);

        /// <summary>
        ///     Space
        /// </summary>
        public readonly Span<T> Space => _buffer.Slice(_length);

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[index];
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStringBuilder(Span<T> buffer)
        {
            _buffer = buffer;
            _array = null;
            _length = buffer.Length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStringBuilder(Span<T> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, buffer.Length, ExceptionArgument.length);
            _buffer = buffer;
            _array = null;
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStringBuilder(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            _buffer = _array = ArrayPool<T>.Shared.Rent(capacity);
            _length = 0;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeStringBuilder(int capacity, int length)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(length, capacity, ExceptionArgument.length);
            _buffer = _array = ArrayPool<T>.Shared.Rent(capacity);
            _length = length;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _buffer = new Span<T>();
            _length = 0;
            var array = _array;
            if (array == null)
                return;
            _array = null;
            ArrayPool<T>.Shared.Return(array);
        }

        /// <summary>
        ///     Appends the string representation of a specified read-only character span to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ReadOnlySpan<T> buffer)
        {
            EnsureCapacity(_length + buffer.Length);
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), (nint)_length)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * Unsafe.SizeOf<T>()));
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
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            var elementOffset1 = 0;
            ref var local1 = ref MemoryMarshal.GetReference(_buffer);
            UnsafeValueListBuilder<int> valueListBuilder;
            if (oldValue.Length == 1)
            {
                if (newValue.Length == 1)
                {
                    Replace(oldValue[0], newValue[0]);
                    return true;
                }

                valueListBuilder = new UnsafeValueListBuilder<int>(stackalloc int[128]);
                var obj = oldValue[0];
                while (true)
                {
                    var num = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref local1, (nint)elementOffset1), _length - elementOffset1).IndexOf(obj);
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
            var num1 = _length + (newValue.Length - oldValue.Length) * (long)readOnlySpan.Length;
            if (num1 > int.MaxValue)
                return false;
            var num2 = (int)num1;
            T[]? objArray = null;
            T[]? array = null;
            var elementOffset2 = 0;
            var elementOffset3 = 0;
            ref var local2 = ref MemoryMarshal.GetReference(newValue);
            ref var local3 = ref MemoryMarshal.GetReference(_buffer);
            Span<T> span;
            if (num2 >= _buffer.Length)
            {
                if (num2 == _buffer.Length)
                {
                    objArray = ArrayPool<T>.Shared.Rent(num2);
                }
                else
                {
                    var minimumLength = Math.Max(_buffer.Length != 0 ? _buffer.Length * 2 : 4, num2);
                    if ((uint)minimumLength > ArrayHelpers.MaxLength)
                        minimumLength = Math.Max(Math.Max(_buffer.Length + 1, ArrayHelpers.MaxLength), _buffer.Length);
                    objArray = ArrayPool<T>.Shared.Rent(minimumLength);
                }

                span = (Span<T>)objArray;
            }
            else
            {
                span = num2 <= 512 / Unsafe.SizeOf<T>() ? stackalloc T[num2] : (Span<T>)(array = ArrayPool<T>.Shared.Rent(num2));
            }

            ref var local4 = ref MemoryMarshal.GetReference(span);
            for (var index = 0; index < readOnlySpan.Length; ++index)
            {
                var num3 = readOnlySpan[index];
                var num4 = num3 - elementOffset2;
                if (num4 != 0)
                {
                    SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local4, (nint)elementOffset3)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local3, (nint)elementOffset2)), (uint)(num4 * Unsafe.SizeOf<T>()));
                    elementOffset3 += num4;
                }

                elementOffset2 = num3 + oldValue.Length;
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local4, (nint)elementOffset3)), ref Unsafe.As<T, byte>(ref local2), (uint)(newValue.Length * Unsafe.SizeOf<T>()));
                elementOffset3 += newValue.Length;
            }

            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local4, (nint)elementOffset3)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local3, (nint)elementOffset2)), (uint)((_length - elementOffset2) * Unsafe.SizeOf<T>()));
            if (objArray != null)
            {
                array = _array;
                _buffer = (Span<T>)(_array = objArray);
            }
            else
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref local3), ref Unsafe.As<T, byte>(ref local4), (uint)(num2 * Unsafe.SizeOf<T>()));

            _length = num2;
            valueListBuilder.Dispose();
            if (array != null)
                ArrayPool<T>.Shared.Return(array);
            return true;
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
            _buffer.Slice(_length, repeatCount).Fill(value);
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
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
                ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
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
        public void Trim(ReadOnlySpan<T> buffer)
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
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeStringBuilder<T> other) => Text.SequenceEqual(other.Text);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <returns>Equals</returns>
        public readonly bool Equals(ReadOnlySpan<T> buffer) => Text.SequenceEqual(buffer);

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
        ///     Try copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
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
        ///     Returns a <see cref="T:System.Memory`1" /> to write to that is at least the requested size (specified by
        ///     <paramref name="sizeHint" />).
        /// </summary>
        /// <param name="sizeHint">The desired length for the slice (exclusive).</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="sizeHint" /> out of range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> GetMemory(int sizeHint = 0) => new(_array, _length, sizeHint);

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
            _buffer.Slice(0, num).Fill(padding);
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
            _buffer.Slice(_length, num).Fill(padding);
            _length = totalWidth;
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
        public readonly ref UnsafeStringBuilder<T> AsRef()
        {
#if NET9_0_OR_GREATER
            return ref Unsafe.AsRef(in this);
#else
            fixed (UnsafeStringBuilder<T>* ptr = &this)
            {
                return ref *ptr;
            }
#endif
        }

        /// <summary>
        ///     Returns a pointer to the given by-ref parameter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly UnsafeStringBuilder<T>* AsPointer()
        {
#if NET9_0_OR_GREATER
            return UnsafeHelpers.AsPointer(ref Unsafe.AsRef(in this));
#else
            fixed (UnsafeStringBuilder<T>* ptr = &this)
            {
                return ptr;
            }
#endif
        }

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
        ///     Creates a new memory over the portion of the target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory() => new(_array, 0, _length);

        /// <summary>
        ///     Creates a new memory over the portion of the target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory(int start) => new(_array, start, _length - start);

        /// <summary>
        ///     Creates a new memory over the portion of the target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory(int start, int length) => new(_array, start, length);

        /// <summary>
        ///     Creates a new memory region over the portion of the target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory() => new(_array, 0, _length);

        /// <summary>
        ///     Creates a new memory region over the portion of the target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory(int start) => new(_array, start, _length - start);

        /// <summary>
        ///     Creates a new memory region over the portion of the target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory(int start, int length) => new(_array, start, length);

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
            if (_buffer.Length < capacity)
                Grow(capacity - _buffer.Length);
            return _buffer.Length;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_buffer.Length * 0.9);
            if (_length < threshold)
                SetCapacity(_length);
            return _buffer.Length;
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
            if (capacity < _length || capacity >= _buffer.Length)
                return _buffer.Length;
            SetCapacity(capacity);
            return _buffer.Length;
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
            if (capacity != _buffer.Length)
            {
                var destination = ArrayPool<T>.Shared.Rent(capacity);
                if (_length > 0)
                    SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference((Span<T>)destination)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(_buffer)), (uint)(_length * Unsafe.SizeOf<T>()));
                var array = _array;
                _buffer = (Span<T>)(_array = destination);
                if (array == null)
                    return;
                ArrayPool<T>.Shared.Return(array);
            }
        }

        /// <summary>
        ///     Grow
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int additionalCapacityRequired)
        {
            var minimumLength = Math.Max(_buffer.Length != 0 ? _buffer.Length * 2 : 4, _buffer.Length + additionalCapacityRequired);
            if ((uint)minimumLength > ArrayHelpers.MaxLength)
                minimumLength = Math.Max(Math.Max(_buffer.Length + 1, ArrayHelpers.MaxLength), _buffer.Length);
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
        ///     Creates a new memory over the portion of the target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>(UnsafeStringBuilder<T> value) => value.AsMemory();

        /// <summary>
        ///     Creates a new memory region over the portion of the target array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>(UnsafeStringBuilder<T> value) => value.AsReadOnlyMemory();

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
        ///     Empty
        /// </summary>
        public static UnsafeStringBuilder<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public readonly Span<T>.Enumerator GetEnumerator() => Text.GetEnumerator();

#if NET9_0_OR_GREATER
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
#endif
    }
}