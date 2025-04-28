using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native string
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public unsafe ref struct NativeString
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly Span<char> _buffer;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => Unsafe.AsPointer(ref MemoryMarshal.GetReference(_buffer)) != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _buffer.Length;

        /// <summary>
        ///     Buffer
        /// </summary>
        public readonly Span<char> Buffer => _buffer;

        /// <summary>
        ///     Text
        /// </summary>
        public readonly Span<char> Text => _buffer.Slice(0, _length);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeString(Span<char> buffer)
        {
            _buffer = buffer;
            _length = buffer.Length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeString(Span<char> buffer, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (length > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeLessOrEqual");
            _buffer = buffer;
            _length = length;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendSpanFormattable<T>(in T obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            if (obj.TryFormat(_buffer.Slice(_length), out var charsWritten, format, provider))
            {
                _length += charsWritten;
                return true;
            }

            return false;
        }
#endif

        /// <summary>
        ///     Append
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Appended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Append(ReadOnlySpan<char> buffer)
        {
            if (_length + buffer.Length > Capacity)
                return false;
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), _length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * sizeof(char)));
            _length += buffer.Length;
            return true;
        }

        /// <summary>
        ///     Append line
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Appended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendLine(ReadOnlySpan<char> buffer)
        {
            var newLine = NewLine;
            if (_length + buffer.Length + newLine.Length > Capacity)
                return false;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, _length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * sizeof(char)));
            _length += buffer.Length;
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, _length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
            _length += newLine.Length;
            return true;
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(ReadOnlySpan<char> buffer) => Text.IndexOf(buffer);

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(ReadOnlySpan<char> buffer) => Text.LastIndexOf(buffer);

        /// <summary>
        ///     Index of any
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAny(ReadOnlySpan<char> buffer) => Text.IndexOfAny(buffer);

        /// <summary>
        ///     Last index of any
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOfAny(ReadOnlySpan<char> buffer) => Text.LastIndexOfAny(buffer);

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ReadOnlySpan<char> buffer) => Text.IndexOf(buffer) >= 0;

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(ReadOnlySpan<char> buffer)
        {
            var index = Text.IndexOf(buffer);
            return index >= 0 && Remove(index, buffer.Length);
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Inserted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, ReadOnlySpan<char> buffer)
        {
            if (startIndex < 0 || startIndex > _length || _length + buffer.Length > Capacity)
                return false;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var count = _length - startIndex;
            if (count > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, startIndex + buffer.Length)), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, startIndex)), (uint)(count * sizeof(char)));
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, startIndex)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * sizeof(char)));
            _length += buffer.Length;
            return true;
        }

        /// <summary>
        ///     Replace
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns>Replaced</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue)
        {
            var index = Text.IndexOf(oldValue);
            if (index < 0)
                return InsertResult.None;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var oldLength = oldValue.Length;
            var newLength = newValue.Length;
            var count = newLength - oldLength;
            if (count > 0 && _length + count > Capacity)
                return InsertResult.InsufficientCapacity;
            if (count != 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, index + newLength)), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, index + oldLength)), (uint)((_length - index - oldLength) * sizeof(char)));
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, index)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newValue)), (uint)(newLength * sizeof(char)));
            _length += count;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Starts with
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(ReadOnlySpan<char> buffer) => Text.StartsWith(buffer);

        /// <summary>
        ///     Ends with
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(ReadOnlySpan<char> buffer) => Text.EndsWith(buffer);

        /// <summary>
        ///     Sequence compare to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SequenceCompareTo(ReadOnlySpan<char> buffer) => Text.SequenceCompareTo(buffer);

        /// <summary>
        ///     Append
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Appended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Append(char value)
        {
            if (_length + 1 > Capacity)
                return false;
            _buffer[_length++] = value;
            return true;
        }

        /// <summary>
        ///     Append line
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Appended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendLine(char value)
        {
            var newLine = NewLine;
            if (_length + 1 + newLine.Length > Capacity)
                return false;
            _buffer[_length++] = value;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, _length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
            _length += newLine.Length;
            return true;
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(char value) => Text.IndexOf(value);

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(char value) => Text.LastIndexOf(value);

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(char value) => Text.IndexOf(value) >= 0;

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(char value)
        {
            var index = Text.IndexOf(value);
            return index >= 0 && Remove(index, 1);
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="value">Value</param>
        /// <returns>Inserted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, char value)
        {
            if (startIndex < 0 || startIndex > _length || _length + 1 > Capacity)
                return false;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var count = _length - startIndex;
            if (count > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, startIndex + 1)), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, startIndex)), (uint)(count * sizeof(char)));
            _buffer[startIndex] = value;
            ++_length;
            return true;
        }

        /// <summary>
        ///     Replace
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns>Replaced</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult Replace(char oldValue, char newValue)
        {
            var index = Text.IndexOf(oldValue);
            if (index < 0)
                return InsertResult.None;
            _buffer[index] = newValue;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Starts with
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool StartsWith(char value) => _length > 1 && _buffer[0] == value;

        /// <summary>
        ///     Ends with
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EndsWith(char value) => _length > 1 && _buffer[_length - 1] == value;

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="length">Length</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int startIndex, int length)
        {
            if (startIndex < 0 || length < 0 || _length - startIndex < length)
                return false;
            if (length > 0)
            {
                ref var reference = ref MemoryMarshal.GetReference(_buffer);
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, startIndex)), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, startIndex + length)), (uint)((_length - startIndex - length) * sizeof(char)));
                _length -= length;
            }

            return true;
        }

        /// <summary>
        ///     Reverse
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse() => Text.Reverse();

        /// <summary>
        ///     Fill
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(char value) => Text.Fill(value);

        /// <summary>
        ///     Trim start
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimStart()
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            while (start < _length && char.IsWhiteSpace(Unsafe.Add(ref reference, start)))
                start++;
            if (start > 0 && start < _length)
            {
                var count = _length - start;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(count * sizeof(char)));
                _length = count;
            }
            else if (start >= _length)
            {
                _length = 0;
            }
        }

        /// <summary>
        ///     Trim end
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimEnd()
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var end = _length - 1;
            while (end >= 0 && char.IsWhiteSpace(Unsafe.Add(ref reference, end)))
                end--;
            _length = end + 1;
        }

        /// <summary>
        ///     Trim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            var end = _length - 1;
            while (start <= end && char.IsWhiteSpace(Unsafe.Add(ref reference, start)))
                start++;
            while (end >= start && char.IsWhiteSpace(Unsafe.Add(ref reference, end)))
                end--;
            if (start <= end)
            {
                _length = end - start + 1;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(_length * sizeof(char)));
            }
            else
            {
                _length = 0;
            }
        }

        /// <summary>
        ///     Slice
        /// </summary>
        /// <param name="start">Start</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeString Slice(int start) => new(Text.Slice(start));

        /// <summary>
        ///     Slice
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeString Slice(int start, int length) => new(Text.Slice(start, length));

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _length = 0;

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(bool zeroed)
        {
            if (zeroed)
                _buffer.Clear();
            _length = 0;
        }

        /// <summary>
        ///     Set length
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetLength(int length)
        {
            if (length < 0 || length > Capacity)
                return false;
            _length = length;
            return true;
        }

        /// <summary>
        ///     Skip
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Skipped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Skip(int length)
        {
            var newLength = _length + length;
            if (newLength < 0 || newLength > Capacity)
                return false;
            _length = newLength;
            return true;
        }

        /// <summary>
        ///     Cast
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<char, TTo>(AsSpan());

        /// <summary>
        ///     Cast
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TTo> Cast<TTo>(int start) where TTo : unmanaged => MemoryMarshal.Cast<char, TTo>(AsSpan(start));

        /// <summary>
        ///     Cast
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TTo> Cast<TTo>(int start, int length) where TTo : unmanaged => MemoryMarshal.Cast<char, TTo>(AsSpan(start, length));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <returns>Equals</returns>
        public bool Equals(ReadOnlySpan<char> buffer) => Text.SequenceEqual(buffer);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("CannotCallEquals");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => throw new NotSupportedException("CannotCallGetHashCode");

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => Text.ToString();

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<char> buffer) => Text.CopyTo(buffer);

        /// <summary>
        ///     Try copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<char> buffer) => Text.TryCopyTo(buffer);

        /// <summary>
        ///     Advance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            var result = _length + count;
            if ((uint)result > Capacity)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeLessOrEqual");
            _length = result;
        }

        /// <summary>
        ///     GetSpan
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> GetSpan(int sizeHint = 0) => _buffer.Slice(_length, sizeHint);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> AsSpan() => Text;

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> AsSpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> AsSpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<char> AsReadOnlySpan() => Text;

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<char> AsReadOnlySpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<char> AsReadOnlySpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<char>(in NativeString nativeString) => nativeString.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<char>(in NativeString nativeString) => nativeString.AsReadOnlySpan();

        /// <summary>
        ///     As native string
        /// </summary>
        /// <returns>NativeString</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeString(Span<char> buffer) => new(buffer);

        /// <summary>
        ///     New line
        /// </summary>
        public static ReadOnlySpan<char> NewLine => Environment.NewLine;

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeString Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        public Span<char>.Enumerator GetEnumerator() => Text.GetEnumerator();
    }
}