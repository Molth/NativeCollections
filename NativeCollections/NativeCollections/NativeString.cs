using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CA2208
#pragma warning disable CS8632
#pragma warning disable CS9081

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native string
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    [IsAssignableTo(typeof(IEquatable<>), typeof(IReadOnlyCollection<char>))]
    [Customizable("public static int GetHashCode(ReadOnlySpan<char> buffer)")]
    public unsafe ref struct NativeString
    {
        /// <summary>
        ///     GetHashCode
        /// </summary>
        private static delegate* managed<ReadOnlySpan<char>, int> _getHashCode;

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
        public bool IsCreated => !Unsafe.IsNullRef(ref MemoryMarshal.GetReference(_buffer));

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
        ///     Space
        /// </summary>
        public readonly Span<char> Space => _buffer.Slice(_length);

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref char this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[index];
        }

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
        /// <returns>Appended</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendLine()
        {
            var newLine = NewLine;
            if (_length + newLine.Length > Capacity)
                return false;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, _length)), ref Unsafe.As<char, byte>(ref MemoryMarshal.GetReference(newLine)), (uint)(newLine.Length * sizeof(char)));
            _length += newLine.Length;
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
        public void Remove(ReadOnlySpan<char> buffer) => Replace(buffer, string.Empty);

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Inserted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, ReadOnlySpan<char> buffer)
        {
            if ((uint)startIndex > (uint)_length || _length + buffer.Length > Capacity)
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
        public bool Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue)
        {
            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(oldValue)) || oldValue.Length == 0)
                return false;
            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(newValue)))
            {
                if (newValue.Length != 0)
                    return false;
                newValue = (ReadOnlySpan<char>)string.Empty;
            }

            NativeValueListBuilder<int> valueListBuilder;
            var elementOffset1 = 0;
            ref var local1 = ref MemoryMarshal.GetReference(_buffer);
            if (oldValue.Length == 1)
            {
                if (newValue.Length == 1)
                {
                    Replace(oldValue[0], newValue[0]);
                    return true;
                }

                valueListBuilder = new NativeValueListBuilder<int>(stackalloc int[128]);
                var ch = oldValue[0];
                while (true)
                {
                    var num = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref local1, elementOffset1), _length - elementOffset1).IndexOf(ch);
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
                valueListBuilder = new NativeValueListBuilder<int>(stackalloc int[128]);
                while (true)
                {
                    var num = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref local1, elementOffset1), _length - elementOffset1).IndexOf(oldValue);
                    if (num >= 0)
                    {
                        valueListBuilder.Append(elementOffset1 + num);
                        elementOffset1 += num + oldValue.Length;
                    }
                    else
                        break;
                }
            }

            if (valueListBuilder.Length == 0)
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
                    Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local2, elementOffset3)), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local1, elementOffset2)), (uint)(num2 * 2));
                    elementOffset3 += num2;
                }

                elementOffset2 = num1 + oldValue.Length;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local2, elementOffset3)), ref Unsafe.As<char, byte>(ref local3), (uint)(newValue.Length * 2));
                elementOffset3 += newValue.Length;
            }

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local2, elementOffset3)), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref local1, elementOffset2)), (uint)((_length - elementOffset2) * 2));
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref local1), ref Unsafe.As<char, byte>(ref local2), (uint)(minimumLength * 2));
            _length = minimumLength;
            valueListBuilder.Dispose();
            if (array != null)
                ArrayPool<char>.Shared.Return(array);
            return true;
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
        ///     Compare
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ReadOnlySpan<char> buffer) => Text.SequenceCompareTo(buffer);

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
        public void Remove(char value)
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var newLength = 0;
            for (var index = 0; index < _length; ++index)
            {
                var ch = Unsafe.Add(ref reference, index);
                if (ch != value)
                    Unsafe.Add(ref reference, newLength++) = ch;
            }

            _length = newLength;
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
            if ((uint)startIndex > (uint)_length || _length + 1 > Capacity)
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
        public void Replace(char oldValue, char newValue)
        {
#if NET8_0_OR_GREATER
            Text.Replace(oldValue, newValue);
#else
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, index);
                if (value == oldValue)
                    value = newValue;
            }
#endif
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
            if ((uint)startIndex > (uint)_length || (uint)length > (uint)(_length - startIndex))
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
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(newLength * sizeof(char)));
            _length = newLength;
        }

        /// <summary>
        ///     Trim start
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimStart(char value)
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            while (start < _length && Unsafe.Add(ref reference, start) == value)
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
        public void TrimEnd(char value)
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var end = _length - 1;
            while (end >= 0 && Unsafe.Add(ref reference, end) == value)
                end--;
            _length = end + 1;
        }

        /// <summary>
        ///     Trim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim(char value)
        {
            if (_length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            var end = _length - 1;
            while (start <= end && Unsafe.Add(ref reference, start) == value)
                start++;
            while (end >= start && Unsafe.Add(ref reference, end) == value)
                end--;
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(newLength * sizeof(char)));
            _length = newLength;
        }

        /// <summary>
        ///     Trim start
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimStart(ReadOnlySpan<char> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            while (start < _length && buffer.IndexOf(Unsafe.Add(ref reference, start)) >= 0)
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
        public void TrimEnd(ReadOnlySpan<char> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var end = _length - 1;
            while (end >= 0 && buffer.IndexOf(Unsafe.Add(ref reference, end)) >= 0)
                end--;
            _length = end + 1;
        }

        /// <summary>
        ///     Trim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim(ReadOnlySpan<char> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            var end = _length - 1;
            while (start <= end && buffer.IndexOf(Unsafe.Add(ref reference, start)) >= 0)
                start++;
            while (end >= start && buffer.IndexOf(Unsafe.Add(ref reference, end)) >= 0)
                end--;
            var newLength = end - start + 1;
            if (newLength <= 0)
            {
                _length = 0;
                return;
            }

            if (start > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<char, byte>(ref reference), ref Unsafe.As<char, byte>(ref Unsafe.Add(ref reference, start)), (uint)(newLength * sizeof(char)));
            _length = newLength;
        }

        /// <summary>
        ///     Slice
        /// </summary>
        /// <param name="start">Start</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeString Substring(int start) => new(Text.Slice(start));

        /// <summary>
        ///     Slice
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeString Substring(int start, int length) => new(Text.Slice(start, length));

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _length = 0;

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(bool clear)
        {
            if (clear)
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
            if ((uint)length > (uint)Capacity)
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
            if ((uint)newLength > (uint)Capacity)
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
        public bool Equals(NativeString other) => Text.SequenceEqual(other.Text);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => GetHashCode(Text);

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
            var newLength = _length + count;
            if ((uint)newLength > (uint)Capacity)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeLessOrEqual");
            _length = newLength;
        }

        /// <summary>
        ///     GetSpan
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<char> GetSpan(int sizeHint = 0) => _buffer.Slice(_length, sizeHint);

        /// <summary>
        ///     To upper
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToUpper()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, index);
                value = char.ToUpper(value);
            }
        }

        /// <summary>
        ///     To lower
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToLower()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, index);
                value = char.ToLower(value);
            }
        }

        /// <summary>
        ///     To upper
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToUpperInvariant()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, index);
                value = char.ToUpperInvariant(value);
            }
        }

        /// <summary>
        ///     To lower
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToLowerInvariant()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            for (var index = 0; index < _length; ++index)
            {
                ref var value = ref Unsafe.Add(ref reference, index);
                value = char.ToLowerInvariant(value);
            }
        }

        /// <summary>
        ///     Pad left
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadLeft(int totalWidth) => PadLeft(totalWidth, ' ');

        /// <summary>
        ///     Pad left
        /// </summary>
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
        ///     Pad right
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PadRight(int totalWidth) => PadRight(totalWidth, ' ');

        /// <summary>
        ///     Pad right
        /// </summary>
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
        ///     Is null or white space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullOrWhiteSpace()
        {
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            if (Unsafe.IsNullRef(ref reference))
                return true;
            for (var index = 0; index < _length; ++index)
            {
                if (!char.IsWhiteSpace(Unsafe.Add(ref reference, index)))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Is null or empty
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullOrEmpty() => Unsafe.IsNullRef(ref MemoryMarshal.GetReference(_buffer)) || _length == 0;

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSplit<char> Split(in char separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSplit<char> Split(ReadOnlySpan<char> separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSplitAny<char> SplitAny(ReadOnlySpan<char> separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSplitRange<char> SplitRange(in char separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSplitRange<char> SplitRange(ReadOnlySpan<char> separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSplitAnyRange<char> SplitAnyRange(ReadOnlySpan<char> separator) => new(Text, separator);

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
        ///     Equals
        /// </summary>
        public static bool operator ==(NativeString left, NativeString right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(NativeString left, NativeString right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(NativeString left, ReadOnlySpan<char> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(NativeString left, ReadOnlySpan<char> right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(ReadOnlySpan<char> left, NativeString right) => right.Equals(left);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(ReadOnlySpan<char> left, NativeString right) => !right.Equals(left);

        /// <summary>
        ///     New line
        /// </summary>
        private static readonly char[] _NewLine = Environment.NewLine.ToCharArray();

        /// <summary>
        ///     New line
        /// </summary>
        private static readonly byte[] _NewLineUtf8 = Encoding.UTF8.GetBytes(Environment.NewLine);

        /// <summary>
        ///     New line
        /// </summary>
        public static ReadOnlySpan<char> NewLine => _NewLine;

        /// <summary>
        ///     New line
        /// </summary>
        public static ReadOnlySpan<byte> NewLineUtf8 => _NewLineUtf8;

        /// <summary>
        ///     New line chars
        /// </summary>
        public static ReadOnlySpan<char> NewLineChars => "\r\f\u0085\u2028\u2029\n";

        /// <summary>
        ///     WhiteSpace chars
        /// </summary>
        public static ReadOnlySpan<char> WhiteSpaceChars => "\t\n\v\f\r\u0020\u0085\u00a0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a\u2028\u2029\u202f\u205f\u3000";

        /// <summary>
        ///     Custom GetHashCode
        /// </summary>
        /// <param name="getHashCode">GetHashCode</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<ReadOnlySpan<char>, int> getHashCode) => _getHashCode = getHashCode;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<char> buffer)
        {
            var getHashCode = _getHashCode;
            if (getHashCode != null)
                return getHashCode(buffer);

#if NETCOREAPP3_0_OR_GREATER
            return string.GetHashCode(buffer);
#else
            return MarvinHelpers.ComputeHash32(MemoryMarshal.Cast<char, byte>(buffer), MarvinHelpers.DefaultSeed);
#endif
        }

        /// <summary>
        ///     Create
        /// </summary>
        public static NativeString Create(ReadOnlySpan<char> buffer) => new(MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(buffer), buffer.Length));

        /// <summary>
        ///     Create
        /// </summary>
        public static NativeString Create(ReadOnlySpan<char> buffer, int length) => new(MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(buffer), buffer.Length), length);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeString Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        public Span<char>.Enumerator GetEnumerator() => Text.GetEnumerator();

        /// <summary>
        ///     Append formattable
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
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormatted(ref DefaultInterpolatedStringHandler message, bool clear = true) => DefaultInterpolatedStringHandlerHelpers.AppendFormatted(ref this, ref message, clear);

        /// <summary>
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
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
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(nint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) => sizeof(nint) == 8 ? AppendFormattable((long)obj, format, provider) : AppendFormattable((int)obj, format, provider);

        /// <summary>
        ///     Append formattable
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AppendFormattable(nuint obj, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) => sizeof(nint) == 8 ? AppendFormattable((ulong)obj, format, provider) : AppendFormattable((uint)obj, format, provider);

        /// <summary>
        ///     Append formattable
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