﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe bit array
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeBitArray : IDisposable
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private NativeArray<int> _buffer;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Buffer
        /// </summary>
        public NativeArray<int> Buffer => _buffer;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLength(value);
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_buffer[index >> 5] & (1 << index)) != 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var bitMask = 1 << index;
                ref var segment = ref _buffer[index >> 5];
                if (value)
                    segment |= bitMask;
                else
                    segment &= ~bitMask;
            }
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public bool this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (_buffer[index >> 5] & (1 << (int)index)) != 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var bitMask = 1 << (int)index;
                ref var segment = ref _buffer[index >> 5];
                if (value)
                    segment |= bitMask;
                else
                    segment &= ~bitMask;
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            _buffer = new NativeArray<int>(GetInt32ArrayLengthFromBitLength(length), true);
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray(int length, bool defaultValue)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            _buffer = new NativeArray<int>(GetInt32ArrayLengthFromBitLength(length));
            _length = length;
            if (defaultValue)
            {
                _buffer.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    _buffer[^1] = (1 << extraBits) - 1;
            }
            else
            {
                _buffer.Clear();
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray(int* buffer, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            _buffer = new NativeArray<int>(buffer, GetInt32ArrayLengthFromBitLength(length));
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray(int* buffer, int length, bool defaultValue)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            _buffer = new NativeArray<int>(buffer, GetInt32ArrayLengthFromBitLength(length));
            _length = length;
            if (defaultValue)
            {
                _buffer.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    _buffer[^1] = (1 << extraBits) - 1;
            }
            else
            {
                _buffer.Clear();
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray(NativeArray<int> buffer, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var intCount = GetInt32ArrayLengthFromBitLength(length);
            if (buffer.Length < intCount)
                throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, $"Requires size is {intCount}, but buffer length is {buffer.Length}.");
            _buffer = buffer;
            _length = length;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray(NativeArray<int> buffer, int length, bool defaultValue)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var intCount = GetInt32ArrayLengthFromBitLength(length);
            if (buffer.Length < intCount)
                throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, $"Requires size is {intCount}, but buffer length is {buffer.Length}.");
            _buffer = buffer;
            _length = length;
            if (defaultValue)
            {
                _buffer.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    _buffer[^1] = (1 << extraBits) - 1;
            }
            else
            {
                _buffer.Clear();
            }
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _buffer.Dispose();

        /// <summary>
        ///     Set length
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var newLength = GetInt32ArrayLengthFromBitLength(length);
            if (newLength > _buffer.Length || newLength + 256 < _buffer.Length)
            {
                var buffer = new NativeArray<int>(newLength);
                Unsafe.CopyBlockUnaligned(buffer.Buffer, _buffer.Buffer, (uint)(_buffer.Length * sizeof(int)));
                Unsafe.InitBlockUnaligned(buffer.Buffer + _buffer.Length, 0, (uint)(newLength - _buffer.Length));
                _buffer.Dispose();
                _buffer = buffer;
            }

            if (length > _length)
            {
                var last = (_length - 1) >> 5;
                Div32Rem(_length, out var bits);
                if (bits > 0)
                    _buffer[last] &= (1 << bits) - 1;
                _buffer.AsSpan(last + 1, newLength - last - 1).Clear();
            }

            _length = length;
        }

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index)
        {
            if ((uint)index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return (_buffer[index >> 5] & (1 << index)) != 0;
        }

        /// <summary>
        ///     Set
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value)
        {
            if ((uint)index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            var bitMask = 1 << index;
            ref var segment = ref _buffer[index >> 5];
            if (value)
                segment |= bitMask;
            else
                segment &= ~bitMask;
        }

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(uint index)
        {
            if (index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return (_buffer[index >> 5] & (1 << (int)index)) != 0;
        }

        /// <summary>
        ///     Set
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, bool value)
        {
            if (index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            var bitMask = 1 << (int)index;
            ref var segment = ref _buffer[index >> 5];
            if (value)
                segment |= bitMask;
            else
                segment &= ~bitMask;
        }

        /// <summary>
        ///     Set all
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(bool value)
        {
            var length = GetInt32ArrayLengthFromBitLength(_length);
            var span = _buffer.AsSpan(0, length);
            if (value)
            {
                span.Fill(-1);
                Div32Rem(_length, out var extraBits);
                if (extraBits > 0)
                    span[^1] &= (1 << extraBits) - 1;
            }
            else
            {
                span.Clear();
            }
        }

        /// <summary>
        ///     And
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void And(UnsafeBitArray* value)
        {
            var count = GetInt32ArrayLengthFromBitLength(_length);
            if (_length != value->_length || (uint)count > (uint)_buffer.Length || (uint)count > (uint)value->_buffer.Length)
                throw new ArgumentException("ArrayLengthsDiffer");
            BitOperationsHelpers.And(_buffer, value->_buffer, (uint)count);
        }

        /// <summary>
        ///     Or
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Or(UnsafeBitArray* value)
        {
            var count = GetInt32ArrayLengthFromBitLength(_length);
            if (_length != value->_length || (uint)count > (uint)_buffer.Length || (uint)count > (uint)value->_buffer.Length)
                throw new ArgumentException("ArrayLengthsDiffer");
            BitOperationsHelpers.Or(_buffer, value->_buffer, (uint)count);
        }

        /// <summary>
        ///     Xor
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Xor(UnsafeBitArray* value)
        {
            var count = GetInt32ArrayLengthFromBitLength(_length);
            if (_length != value->_length || (uint)count > (uint)_buffer.Length || (uint)count > (uint)value->_buffer.Length)
                throw new ArgumentException("ArrayLengthsDiffer");
            BitOperationsHelpers.Xor(_buffer, value->_buffer, (uint)count);
        }

        /// <summary>
        ///     Not
        /// </summary>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Not()
        {
            var count = GetInt32ArrayLengthFromBitLength(_length);
            BitOperationsHelpers.Not(_buffer, (uint)count);
        }

        /// <summary>
        ///     Right shift
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RightShift(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeNonNegative");
            if (count == 0)
                return;
            var toIndex = 0;
            var length = GetInt32ArrayLengthFromBitLength(_length);
            if (count < _length)
            {
                var fromIndex = Div32Rem(count, out var shiftCount);
                Div32Rem(_length, out var extraBits);
                if (shiftCount == 0)
                {
                    unchecked
                    {
                        var mask = uint.MaxValue >> (32 - extraBits);
                        _buffer[length - 1] &= (int)mask;
                    }

                    Unsafe.CopyBlockUnaligned(_buffer.Buffer, _buffer.Buffer + fromIndex, (uint)((length - fromIndex) * sizeof(int)));
                    toIndex = length - fromIndex;
                }
                else
                {
                    var lastIndex = length - 1;
                    unchecked
                    {
                        while (fromIndex < lastIndex)
                        {
                            var right = (uint)_buffer[fromIndex] >> shiftCount;
                            var left = _buffer[++fromIndex] << (32 - shiftCount);
                            _buffer[toIndex++] = left | (int)right;
                        }

                        var mask = uint.MaxValue >> (32 - extraBits);
                        mask &= (uint)_buffer[fromIndex];
                        _buffer[toIndex++] = (int)(mask >> shiftCount);
                    }
                }
            }

            _buffer.AsSpan(toIndex, length - toIndex).Clear();
        }

        /// <summary>
        ///     Left shift
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LeftShift(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeNonNegative");
            if (count == 0)
                return;
            int lengthToClear;
            if (count < _length)
            {
                var lastIndex = (_length - 1) >> 5;
                lengthToClear = Div32Rem(count, out var shiftCount);
                if (shiftCount == 0)
                {
                    Unsafe.CopyBlockUnaligned(_buffer.Buffer + lengthToClear, _buffer.Buffer, (uint)((lastIndex + 1 - lengthToClear) * sizeof(int)));
                }
                else
                {
                    var fromIndex = lastIndex - lengthToClear;
                    unchecked
                    {
                        while (fromIndex > 0)
                        {
                            var left = _buffer[fromIndex] << shiftCount;
                            var right = (uint)_buffer[--fromIndex] >> (32 - shiftCount);
                            _buffer[lastIndex] = left | (int)right;
                            lastIndex--;
                        }

                        _buffer[lastIndex] = _buffer[fromIndex] << shiftCount;
                    }
                }
            }
            else
            {
                lengthToClear = GetInt32ArrayLengthFromBitLength(_length);
            }

            _buffer.AsSpan(0, lengthToClear).Clear();
        }

        /// <summary>
        ///     Has all set
        /// </summary>
        /// <returns>All set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllSet()
        {
            Div32Rem(_length, out var extraBits);
            var intCount = GetInt32ArrayLengthFromBitLength(_length);
            if (extraBits != 0)
                intCount--;
            if (SpanHelpers.ContainsAnyExcept(_buffer.AsReadOnlySpan(0, intCount), -1))
                return false;
            if (extraBits == 0)
                return true;
            var mask = (1 << extraBits) - 1;
            return (_buffer[intCount] & mask) == mask;
        }

        /// <summary>
        ///     Has any set
        /// </summary>
        /// <returns>Any set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnySet()
        {
            Div32Rem(_length, out var extraBits);
            var intCount = GetInt32ArrayLengthFromBitLength(_length);
            if (extraBits != 0)
                intCount--;
            if (SpanHelpers.ContainsAnyExcept(_buffer.AsReadOnlySpan(0, intCount), 0))
                return true;
            return extraBits != 0 && (_buffer[intCount] & ((1 << extraBits) - 1)) != 0;
        }

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        public NativeBitArraySlot GetSlot(int index)
        {
            if ((uint)index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return new NativeBitArraySlot(&_buffer.Buffer[index >> 5], 1 << index);
        }

        /// <summary>
        ///     Try get
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="slot">Slot</param>
        /// <returns>Got</returns>
        public bool TryGetSlot(int index, out NativeBitArraySlot slot)
        {
            if ((uint)index >= (uint)_length)
            {
                slot = default;
                return false;
            }

            slot = new NativeBitArraySlot(&_buffer.Buffer[index >> 5], 1 << index);
            return true;
        }

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        public NativeBitArraySlot GetSlot(uint index)
        {
            if (index >= (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return new NativeBitArraySlot(&_buffer.Buffer[index >> 5], 1 << (int)index);
        }

        /// <summary>
        ///     Try get
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="slot">Slot</param>
        /// <returns>Got</returns>
        public bool TryGetSlot(uint index, out NativeBitArraySlot slot)
        {
            if (index >= (uint)_length)
            {
                slot = default;
                return false;
            }

            slot = new NativeBitArraySlot(&_buffer.Buffer[index >> 5], 1 << (int)index);
            return true;
        }

        /// <summary>
        ///     Get int32 buffer length from bit length
        /// </summary>
        /// <param name="n">Bit length</param>
        /// <returns>Int32 buffer length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInt32ArrayLengthFromBitLength(int n)
        {
#if NET7_0_OR_GREATER
            return (n - 1 + (1 << 5)) >>> 5;
#else
            return (int)((uint)(n - 1 + (1 << 5)) >> 5);
#endif
        }

        /// <summary>
        ///     Divide by 32 and get remainder
        /// </summary>
        /// <param name="number">Number</param>
        /// <param name="remainder">Remainder</param>
        /// <returns>Quotient</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Div32Rem(int number, out int remainder)
        {
            var quotient = (uint)number / 32;
            remainder = number & (32 - 1);
            return (int)quotient;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeBitArray Empty => new();
    }
}