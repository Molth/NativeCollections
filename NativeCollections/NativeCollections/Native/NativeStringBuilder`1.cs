﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CA2208
#pragma warning disable CS8500
#pragma warning disable CS8632
#pragma warning disable CS9081

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native string builder
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    [IsAssignableTo(typeof(IDisposable), typeof(IEquatable<>), typeof(IReadOnlyCollection<>))]
    public unsafe ref struct NativeStringBuilder<T> where T : unmanaged, IComparable<T>, IEquatable<T>
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
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !Unsafe.IsNullRef(ref MemoryMarshal.GetReference(_buffer));

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _length == 0;

        /// <summary>
        ///     Length
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Capacity
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
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
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
        public NativeStringBuilder(Span<T> buffer)
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
        public NativeStringBuilder(Span<T> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, nameof(length));
            ThrowHelpers.ThrowIfGreaterThan(length, buffer.Length, nameof(length));
            _buffer = buffer;
            _array = null;
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeStringBuilder(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            _buffer = _array = ArrayPool<T>.Shared.Rent(capacity);
            _length = 0;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeStringBuilder(int capacity, int length)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            ThrowHelpers.ThrowIfNegative(length, nameof(length));
            ThrowHelpers.ThrowIfGreaterThan(length, capacity, nameof(length));
            _buffer = _array = ArrayPool<T>.Shared.Rent(capacity);
            _length = length;
        }

        /// <summary>
        ///     Dispose
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
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ReadOnlySpan<T> buffer)
        {
            EnsureCapacity(_length + buffer.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), (nint)_length)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * sizeof(T)));
            _length += buffer.Length;
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(ReadOnlySpan<T> buffer) => Text.IndexOf(buffer);

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(ReadOnlySpan<T> buffer) => Text.LastIndexOf(buffer);

        /// <summary>
        ///     Index of any
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOfAny(ReadOnlySpan<T> buffer) => Text.IndexOfAny(buffer);

        /// <summary>
        ///     Last index of any
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOfAny(ReadOnlySpan<T> buffer) => Text.LastIndexOfAny(buffer);

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(ReadOnlySpan<T> buffer) => Text.IndexOf(buffer) >= 0;

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ReadOnlySpan<T> buffer) => Replace(buffer, (ReadOnlySpan<T>)Array.Empty<T>());

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Inserted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, ReadOnlySpan<T> buffer)
        {
            if ((uint)startIndex > (uint)_length)
                return false;
            EnsureCapacity(_length + buffer.Length);
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var count = _length - startIndex;
            if (count > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)(startIndex + buffer.Length))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), (uint)(count * sizeof(T)));
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(buffer.Length * sizeof(T)));
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
        public bool Replace(ReadOnlySpan<T> oldValue, ReadOnlySpan<T> newValue)
        {
            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(oldValue)) || oldValue.Length == 0)
                return false;
            if (Unsafe.IsNullRef(ref MemoryMarshal.GetReference(newValue)))
            {
                if (newValue.Length != 0)
                    return false;
                newValue = (ReadOnlySpan<T>)Array.Empty<T>();
            }

            var elementOffset1 = 0;
            ref var local1 = ref MemoryMarshal.GetReference(_buffer);
            NativeValueListBuilder<int> valueListBuilder;
            if (oldValue.Length == 1)
            {
                if (newValue.Length == 1)
                {
                    Replace(in oldValue[0], in newValue[0]);
                    return true;
                }

                valueListBuilder = new NativeValueListBuilder<int>(stackalloc int[128]);
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
                valueListBuilder = new NativeValueListBuilder<int>(stackalloc int[128]);
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

            if (valueListBuilder.Length == 0)
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
                    if ((uint)minimumLength > 2147483591U)
                        minimumLength = Math.Max(Math.Max(_buffer.Length + 1, 2147483591), _buffer.Length);
                    objArray = ArrayPool<T>.Shared.Rent(minimumLength);
                }

                span = (Span<T>)objArray;
            }
            else
            {
                span = num2 <= 512 / sizeof(T) ? stackalloc T[num2] : (Span<T>)(array = ArrayPool<T>.Shared.Rent(num2));
            }

            ref var local4 = ref MemoryMarshal.GetReference(span);
            for (var index = 0; index < readOnlySpan.Length; ++index)
            {
                var num3 = readOnlySpan[index];
                var num4 = num3 - elementOffset2;
                if (num4 != 0)
                {
                    Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local4, (nint)elementOffset3)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local3, (nint)elementOffset2)), (uint)(num4 * sizeof(T)));
                    elementOffset3 += num4;
                }

                elementOffset2 = num3 + oldValue.Length;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local4, (nint)elementOffset3)), ref Unsafe.As<T, byte>(ref local2), (uint)(newValue.Length * sizeof(T)));
                elementOffset3 += newValue.Length;
            }

            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local4, (nint)elementOffset3)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref local3, (nint)elementOffset2)), (uint)((_length - elementOffset2) * sizeof(T)));
            if (objArray != null)
            {
                array = _array;
                _buffer = (Span<T>)(_array = objArray);
            }
            else
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref local3), ref Unsafe.As<T, byte>(ref local4), (uint)(num2 * sizeof(T)));

            _length = num2;
            valueListBuilder.Dispose();
            if (array != null)
                ArrayPool<T>.Shared.Return(array);
            return true;
        }

        /// <summary>
        ///     Starts with
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(ReadOnlySpan<T> buffer) => Text.StartsWith(buffer);

        /// <summary>
        ///     Ends with
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(ReadOnlySpan<T> buffer) => Text.EndsWith(buffer);

        /// <summary>
        ///     Compare
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Compare(ReadOnlySpan<T> buffer) => Text.SequenceCompareTo(buffer);

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in T value)
        {
            EnsureCapacity(_length + 1);
            _buffer[_length++] = value;
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in T value, int repeatCount)
        {
            EnsureCapacity(_length + repeatCount);
            SpanHelpers.Fill(_buffer.Slice(_length, repeatCount), value);
            _length += repeatCount;
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in T value) => Text.IndexOf(value);

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(in T value) => Text.LastIndexOf(value);

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in T value) => SpanHelpers.Contains(Text, value);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
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
        ///     Insert
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="value">Value</param>
        /// <returns>Inserted</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Insert(int startIndex, in T value)
        {
            if ((uint)startIndex > (uint)_length)
                return false;
            EnsureCapacity(_length + 1);
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var count = _length - startIndex;
            if (count > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)(startIndex + 1))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), (uint)(count * sizeof(T)));
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
        ///     Starts with
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool StartsWith(in T value) => _length > 1 && _buffer[0].Equals(value);

        /// <summary>
        ///     Ends with
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool EndsWith(in T value) => _length > 1 && _buffer[_length - 1].Equals(value);

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
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)startIndex)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)(startIndex + length))), (uint)((_length - startIndex - length) * sizeof(T)));
                _length -= length;
            }

            return true;
        }

        /// <summary>
        ///     Reverse
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Reverse() => Text.Reverse();

        /// <summary>
        ///     Fill
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Fill(in T value) => SpanHelpers.Fill(Text, value);

        /// <summary>
        ///     Trim start
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
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(count * sizeof(T)));
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
        ///     Trim
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
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(newLength * sizeof(T)));
            _length = newLength;
        }

        /// <summary>
        ///     Trim start
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimStart(ReadOnlySpan<T> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var start = 0;
            while (start < _length && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, (nint)start)))
                start++;
            if (start > 0 && start < _length)
            {
                var count = _length - start;
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(count * sizeof(T)));
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
        public void TrimEnd(ReadOnlySpan<T> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.Length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(_buffer);
            var end = _length - 1;
            while (end >= 0 && SpanHelpers.Contains(buffer, Unsafe.Add(ref reference, (nint)end)))
                end--;
            _length = end + 1;
        }

        /// <summary>
        ///     Trim
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim(ReadOnlySpan<T> buffer)
        {
            if (_length == 0 || Unsafe.IsNullRef(ref MemoryMarshal.GetReference(buffer)) || buffer.Length == 0)
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
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref reference), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)start)), (uint)(newLength * sizeof(T)));
            _length = newLength;
        }

        /// <summary>
        ///     Slice
        /// </summary>
        /// <param name="start">Start</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> Substring(int start) => Text.Slice(start);

        /// <summary>
        ///     Slice
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> Substring(int start, int length) => Text.Slice(start, length);

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
        public readonly Span<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan());

        /// <summary>
        ///     Cast
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start) where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan(start));

        /// <summary>
        ///     Cast
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<TTo> Cast<TTo>(int start, int length) where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan(start, length));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <returns>Equals</returns>
        public readonly bool Equals(NativeStringBuilder<T> other) => Text.SequenceEqual(other.Text);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <returns>Equals</returns>
        public readonly bool Equals(ReadOnlySpan<T> buffer) => Text.SequenceEqual(buffer);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => typeof(T) == typeof(char) ? NativeString.GetHashCode(MemoryMarshal.Cast<T, char>(Text)) : NativeHashCode.GetHashCode<T>(Text);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString()
        {
            if (typeof(T) == typeof(char))
                return Text.ToString();
            if (typeof(T) == typeof(byte))
                return Encoding.UTF8.GetString(MemoryMarshal.AsBytes(Text));
            return $"NativeStringBuilder<{typeof(T).Name}>[{_length}]";
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer) => Text.CopyTo(buffer);

        /// <summary>
        ///     Try copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryCopyTo(Span<T> buffer) => Text.TryCopyTo(buffer);

        /// <summary>
        ///     Advance
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            var newLength = _length + count;
            ThrowHelpers.ThrowIfGreaterThan((uint)newLength, (uint)Capacity, nameof(count));
            _length = newLength;
        }

        /// <summary>
        ///     GetSpan
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> GetSpan(int sizeHint = 0) => _buffer.Slice(_length, sizeHint);

        /// <summary>
        ///     GetMemory
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> GetMemory(int sizeHint = 0) => new(_array, _length, sizeHint);

        /// <summary>
        ///     Pad left
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PadLeft(int totalWidth, in T paddingT)
        {
            EnsureCapacity(totalWidth);
            var num = totalWidth - _length;
            if (num <= 0)
                return;
            Text.CopyTo(_buffer.Slice(num));
            SpanHelpers.Fill(_buffer.Slice(0, num), paddingT);
            _length = totalWidth;
        }

        /// <summary>
        ///     Pad right
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PadRight(int totalWidth, in T paddingT)
        {
            EnsureCapacity(totalWidth);
            var num = totalWidth - _length;
            if (num <= 0)
                return;
            SpanHelpers.Fill(_buffer.Slice(_length, num), paddingT);
            _length = totalWidth;
        }

        /// <summary>
        ///     Is null or empty
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsNullOrEmpty() => Unsafe.IsNullRef(ref MemoryMarshal.GetReference(_buffer)) || _length == 0;

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplit<T> Split(in T separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplit<T> Split(ReadOnlySpan<T> separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAny<T> SplitAny(ReadOnlySpan<T> separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<T> SplitRange(in T separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitRange<T> SplitRange(ReadOnlySpan<T> separator) => new(Text, separator);

        /// <summary>
        ///     Split
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeSplitAnyRange<T> SplitAnyRange(ReadOnlySpan<T> separator) => new(Text, separator);

        /// <summary>
        ///     As ref
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref NativeStringBuilder<T> AsRef()
        {
            fixed (NativeStringBuilder<T>* ptr = &this)
            {
                return ref *ptr;
            }
        }

        /// <summary>
        ///     As pointer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeStringBuilder<T>* AsPointer()
        {
            fixed (NativeStringBuilder<T>* ptr = &this)
            {
                return ptr;
            }
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => Text;

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => Text;

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     As memory
        /// </summary>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory() => new(_array, 0, _length);

        /// <summary>
        ///     As memory
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory(int start) => new(_array, start, _length - start);

        /// <summary>
        ///     As memory
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Memory<T> AsMemory(int start, int length) => new(_array, start, length);

        /// <summary>
        ///     As readOnly memory
        /// </summary>
        /// <returns>ReadOnlyMemory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory() => new(_array, 0, _length);

        /// <summary>
        ///     As readOnly memory
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlyMemory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory(int start) => new(_array, start, _length - start);

        /// <summary>
        ///     As readOnly memory
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlyMemory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<T> AsReadOnlyMemory(int start, int length) => new(_array, start, length);

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            if (_buffer.Length < capacity)
                Grow(capacity - _buffer.Length);
            return _buffer.Length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_buffer.Length * 0.9);
            if (_length < threshold)
                SetCapacity(_length);
            return _buffer.Length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            if (capacity < _length || capacity >= _buffer.Length)
                return _buffer.Length;
            SetCapacity(capacity);
            return _buffer.Length;
        }

        /// <summary>
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _length, nameof(capacity));
            if (capacity != _buffer.Length)
            {
                var destination = ArrayPool<T>.Shared.Rent(capacity);
                if (_length > 0)
                    Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference((Span<T>)destination)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(_buffer)), (uint)(_length * sizeof(T)));
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
            if ((uint)minimumLength > 2147483591U)
                minimumLength = Math.Max(Math.Max(_buffer.Length + 1, 2147483591), _buffer.Length);
            SetCapacity(minimumLength);
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeStringBuilder<T> nativeStringBuilder) => nativeStringBuilder.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeStringBuilder<T> nativeStringBuilder) => nativeStringBuilder.AsReadOnlySpan();

        /// <summary>
        ///     As memory
        /// </summary>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>(NativeStringBuilder<T> nativeStringBuilder) => nativeStringBuilder.AsMemory();

        /// <summary>
        ///     As readOnly memory
        /// </summary>
        /// <returns>ReadOnlyMemory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>(NativeStringBuilder<T> nativeStringBuilder) => nativeStringBuilder.AsReadOnlyMemory();

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(NativeStringBuilder<T> left, NativeStringBuilder<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(NativeStringBuilder<T> left, NativeStringBuilder<T> right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(NativeStringBuilder<T> left, ReadOnlySpan<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(NativeStringBuilder<T> left, ReadOnlySpan<T> right) => !left.Equals(right);

        /// <summary>
        ///     Equals
        /// </summary>
        public static bool operator ==(ReadOnlySpan<T> left, NativeStringBuilder<T> right) => right.Equals(left);

        /// <summary>
        ///     Not equals
        /// </summary>
        public static bool operator !=(ReadOnlySpan<T> left, NativeStringBuilder<T> right) => !right.Equals(left);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeStringBuilder<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        public readonly Span<T>.Enumerator GetEnumerator() => Text.GetEnumerator();
    }
}