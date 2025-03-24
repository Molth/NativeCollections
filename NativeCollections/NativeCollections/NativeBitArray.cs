using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native bit array
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeBitArray : IDisposable, IEquatable<NativeBitArray>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeBitArrayHandle
        {
            /// <summary>
            ///     Array
            /// </summary>
            public NativeArray<int> Array;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Get or set value
            /// </summary>
            /// <param name="index">Index</param>
            public bool this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (Array[index >> 5] & (1 << index)) != 0;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    var bitMask = 1 << index;
                    ref var segment = ref Array[index >> 5];
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
                get => (Array[index >> 5] & (1 << (int)index)) != 0;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    var bitMask = 1 << (int)index;
                    ref var segment = ref Array[index >> 5];
                    if (value)
                        segment |= bitMask;
                    else
                        segment &= ~bitMask;
                }
            }

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
                if (newLength > Array.Length || newLength + 256 < Array.Length)
                {
                    var array = new NativeArray<int>(newLength);
                    Unsafe.CopyBlockUnaligned(array.Array, Array.Array, (uint)(Array.Length * sizeof(int)));
                    Unsafe.InitBlockUnaligned(array.Array + Array.Length, 0, (uint)(newLength - Array.Length));
                    Array.Dispose();
                    Array = array;
                }

                if (length > Length)
                {
                    var last = (Length - 1) >> 5;
                    Div32Rem(Length, out var bits);
                    if (bits > 0)
                        Array[last] &= (1 << bits) - 1;
                    Array.AsSpan(last + 1, newLength - last - 1).Clear();
                }

                Length = length;
            }

            /// <summary>
            ///     Get
            /// </summary>
            /// <param name="index">Index</param>
            /// <returns>Value</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Get(int index)
            {
                if ((uint)index >= (uint)Length)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                return (Array[index >> 5] & (1 << index)) != 0;
            }

            /// <summary>
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Set(int index, bool value)
            {
                if ((uint)index >= (uint)Length)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                var bitMask = 1 << index;
                ref var segment = ref Array[index >> 5];
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
                if (index >= (uint)Length)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                return (Array[index >> 5] & (1 << (int)index)) != 0;
            }

            /// <summary>
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Set(uint index, bool value)
            {
                if (index >= (uint)Length)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
                var bitMask = 1 << (int)index;
                ref var segment = ref Array[index >> 5];
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
                var arrayLength = GetInt32ArrayLengthFromBitLength(Length);
                var span = Array.AsSpan(0, arrayLength);
                if (value)
                {
                    span.Fill(-1);
                    Div32Rem(Length, out var extraBits);
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
            public void And(NativeBitArrayHandle* value)
            {
                var count = GetInt32ArrayLengthFromBitLength(Length);
                if (Length != value->Length || (uint)count > (uint)Array.Length || (uint)count > (uint)value->Array.Length)
                    throw new ArgumentException("ArrayLengthsDiffer");
                BitOperationsHelpers.And(Array, value->Array, (uint)count);
            }

            /// <summary>
            ///     Or
            /// </summary>
            /// <param name="value">Value</param>
            /// <returns>NativeBitArray</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Or(NativeBitArrayHandle* value)
            {
                var count = GetInt32ArrayLengthFromBitLength(Length);
                if (Length != value->Length || (uint)count > (uint)Array.Length || (uint)count > (uint)value->Array.Length)
                    throw new ArgumentException("ArrayLengthsDiffer");
                BitOperationsHelpers.Or(Array, value->Array, (uint)count);
            }

            /// <summary>
            ///     Xor
            /// </summary>
            /// <param name="value">Value</param>
            /// <returns>NativeBitArray</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Xor(NativeBitArrayHandle* value)
            {
                var count = GetInt32ArrayLengthFromBitLength(Length);
                if (Length != value->Length || (uint)count > (uint)Array.Length || (uint)count > (uint)value->Array.Length)
                    throw new ArgumentException("ArrayLengthsDiffer");
                BitOperationsHelpers.Xor(Array, value->Array, (uint)count);
            }

            /// <summary>
            ///     Not
            /// </summary>
            /// <returns>NativeBitArray</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Not()
            {
                var count = GetInt32ArrayLengthFromBitLength(Length);
                BitOperationsHelpers.Not(Array, (uint)count);
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
                var length = GetInt32ArrayLengthFromBitLength(Length);
                if (count < Length)
                {
                    var fromIndex = Div32Rem(count, out var shiftCount);
                    Div32Rem(Length, out var extraBits);
                    if (shiftCount == 0)
                    {
                        unchecked
                        {
                            var mask = uint.MaxValue >> (32 - extraBits);
                            Array[length - 1] &= (int)mask;
                        }

                        Unsafe.CopyBlockUnaligned(Array.Array, Array.Array + fromIndex, (uint)((length - fromIndex) * sizeof(int)));
                        toIndex = length - fromIndex;
                    }
                    else
                    {
                        var lastIndex = length - 1;
                        unchecked
                        {
                            while (fromIndex < lastIndex)
                            {
                                var right = (uint)Array[fromIndex] >> shiftCount;
                                var left = Array[++fromIndex] << (32 - shiftCount);
                                Array[toIndex++] = left | (int)right;
                            }

                            var mask = uint.MaxValue >> (32 - extraBits);
                            mask &= (uint)Array[fromIndex];
                            Array[toIndex++] = (int)(mask >> shiftCount);
                        }
                    }
                }

                Array.AsSpan(toIndex, length - toIndex).Clear();
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
                if (count < Length)
                {
                    var lastIndex = (Length - 1) >> 5;
                    lengthToClear = Div32Rem(count, out var shiftCount);
                    if (shiftCount == 0)
                    {
                        Unsafe.CopyBlockUnaligned(Array.Array + lengthToClear, Array.Array, (uint)((lastIndex + 1 - lengthToClear) * sizeof(int)));
                    }
                    else
                    {
                        var fromIndex = lastIndex - lengthToClear;
                        unchecked
                        {
                            while (fromIndex > 0)
                            {
                                var left = Array[fromIndex] << shiftCount;
                                var right = (uint)Array[--fromIndex] >> (32 - shiftCount);
                                Array[lastIndex] = left | (int)right;
                                lastIndex--;
                            }

                            Array[lastIndex] = Array[fromIndex] << shiftCount;
                        }
                    }
                }
                else
                {
                    lengthToClear = GetInt32ArrayLengthFromBitLength(Length);
                }

                Array.AsSpan(0, lengthToClear).Clear();
            }

            /// <summary>
            ///     Has all set
            /// </summary>
            /// <returns>All set</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool HasAllSet()
            {
                Div32Rem(Length, out var extraBits);
                var intCount = GetInt32ArrayLengthFromBitLength(Length);
                if (extraBits != 0)
                    intCount--;
#if NET8_0_OR_GREATER
                if (Array.AsSpan(0, intCount).ContainsAnyExcept(-1))
                    return false;
#elif NET7_0_OR_GREATER
                if (Array.AsSpan(0, intCount).IndexOfAnyExcept(-1) >= 0)
                    return false;
#else
                for (var i = 0; i < intCount; ++i)
                {
                    if (Array[i] != -1)
                        return false;
                }
#endif
                if (extraBits == 0)
                    return true;
                var mask = (1 << extraBits) - 1;
                return (Array[intCount] & mask) == mask;
            }

            /// <summary>
            ///     Has any set
            /// </summary>
            /// <returns>Any set</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool HasAnySet()
            {
                Div32Rem(Length, out var extraBits);
                var intCount = GetInt32ArrayLengthFromBitLength(Length);
                if (extraBits != 0)
                    intCount--;
#if NET8_0_OR_GREATER
                if (Array.AsSpan(0, intCount).ContainsAnyExcept(0))
                    return true;
#elif NET7_0_OR_GREATER
                if (Array.AsSpan(0, intCount).IndexOfAnyExcept(0) >= 0)
                    return true;
#else
                for (var i = 0; i < intCount; ++i)
                {
                    if (Array[i] != 0)
                        return true;
                }
#endif
                return extraBits != 0 && (Array[intCount] & ((1 << extraBits) - 1)) != 0;
            }
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeBitArrayHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var handle = (NativeBitArrayHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeBitArrayHandle));
            handle->Array = new NativeArray<int>(GetInt32ArrayLengthFromBitLength(length), true);
            handle->Length = length;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(int length, bool defaultValue)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var handle = (NativeBitArrayHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeBitArrayHandle));
            handle->Array = new NativeArray<int>(GetInt32ArrayLengthFromBitLength(length));
            handle->Length = length;
            if (defaultValue)
            {
                handle->Array.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    handle->Array[^1] = (1 << extraBits) - 1;
            }
            else
            {
                handle->Array.Clear();
            }

            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(int* array, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var handle = (NativeBitArrayHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeBitArrayHandle));
            handle->Array = new NativeArray<int>(array, GetInt32ArrayLengthFromBitLength(length));
            handle->Length = length;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(int* array, int length, bool defaultValue)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var handle = (NativeBitArrayHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeBitArrayHandle));
            handle->Array = new NativeArray<int>(array, GetInt32ArrayLengthFromBitLength(length));
            handle->Length = length;
            if (defaultValue)
            {
                handle->Array.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    handle->Array[^1] = (1 << extraBits) - 1;
            }
            else
            {
                handle->Array.Clear();
            }

            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(NativeArray<int> array, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var intCount = GetInt32ArrayLengthFromBitLength(length);
            if (array.Length < intCount)
                throw new ArgumentOutOfRangeException(nameof(array), array.Length, $"Requires size is {intCount}, but buffer length is {array.Length}.");
            var handle = (NativeBitArrayHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeBitArrayHandle));
            handle->Array = array;
            handle->Length = length;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(NativeArray<int> array, int length, bool defaultValue)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var intCount = GetInt32ArrayLengthFromBitLength(length);
            if (array.Length < intCount)
                throw new ArgumentOutOfRangeException(nameof(array), array.Length, $"Requires size is {intCount}, but buffer length is {array.Length}.");
            var handle = (NativeBitArrayHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeBitArrayHandle));
            handle->Array = array;
            handle->Length = length;
            if (defaultValue)
            {
                handle->Array.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    handle->Array[^1] = (1 << extraBits) - 1;
            }
            else
            {
                handle->Array.Clear();
            }

            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Array
        /// </summary>
        public NativeArray<int> Array => _handle->Array;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->SetLength(value);
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (*_handle)[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => (*_handle)[index] = value;
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public bool this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (*_handle)[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => (*_handle)[index] = value;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeBitArray other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeBitArray nativeBitArray && nativeBitArray == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeBitArray";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeBitArray left, NativeBitArray right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeBitArray left, NativeBitArray right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            handle->Array.Dispose();
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index) => _handle->Get(index);

        /// <summary>
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value) => _handle->Set(index, value);

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(uint index) => _handle->Get(index);

        /// <summary>
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, bool value) => _handle->Set(index, value);

        /// <summary>
        ///     Set all
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(bool value) => _handle->SetAll(value);

        /// <summary>
        ///     And
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray And(NativeBitArray value)
        {
            if (!value.IsCreated)
                throw new ArgumentNullException(nameof(value));
            _handle->And(value._handle);
            return this;
        }

        /// <summary>
        ///     Or
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Or(NativeBitArray value)
        {
            if (!value.IsCreated)
                throw new ArgumentNullException(nameof(value));
            _handle->Or(value._handle);
            return this;
        }

        /// <summary>
        ///     Xor
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Xor(NativeBitArray value)
        {
            if (!value.IsCreated)
                throw new ArgumentNullException(nameof(value));
            _handle->Xor(value._handle);
            return this;
        }

        /// <summary>
        ///     Not
        /// </summary>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Not()
        {
            _handle->Not();
            return this;
        }

        /// <summary>
        ///     Right shift
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray RightShift(int count)
        {
            _handle->RightShift(count);
            return this;
        }

        /// <summary>
        ///     Left shift
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray LeftShift(int count)
        {
            _handle->LeftShift(count);
            return this;
        }

        /// <summary>
        ///     Has all set
        /// </summary>
        /// <returns>All set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllSet() => _handle->HasAllSet();

        /// <summary>
        ///     Has any set
        /// </summary>
        /// <returns>Any set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnySet() => _handle->HasAnySet();

        /// <summary>
        ///     Get int32 array length from bit length
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetInt32ArrayLengthFromBitLength(int n)
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
        public static NativeBitArray Empty => new();
    }
}