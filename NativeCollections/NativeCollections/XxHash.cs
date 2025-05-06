using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS1591
#pragma warning disable CS8625
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     https://github.com/Cyan4973/xxHash
    /// </summary>
    public static unsafe class XxHash
    {
        /// <summary>
        ///     Default seed
        /// </summary>
        public static readonly uint DefaultSeed;

        /// <summary>
        ///     Structure
        /// </summary>
        static XxHash()
        {
            try
            {
                var field = typeof(HashCode).GetField("s_seed", BindingFlags.Static | BindingFlags.NonPublic);
                if (field != null && field.FieldType == typeof(uint))
                {
                    var value = field.GetValue(null);
                    if (value != null)
                    {
                        DefaultSeed = Unsafe.Unbox<uint>(value);
                        return;
                    }
                }
            }
            catch
            {
                //
            }

            uint num;
            NativeRandom.Next(&num, 4);
            DefaultSeed = num;
        }

#if !NET6_0_OR_GREATER
        /// <summary>
        ///     Add bytes
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddBytes(ref this HashCode hashCode, ReadOnlySpan<byte> buffer)
        {
            ref var local1 = ref MemoryMarshal.GetReference(buffer);
            int length;
            for (length = buffer.Length; length >= 4; length -= 4)
                hashCode.Add(Unsafe.ReadUnaligned<int>(ref Unsafe.AddByteOffset(ref local1, (nint)(buffer.Length - length))));
            ref var local2 = ref Unsafe.AddByteOffset(ref local1, (nint)(buffer.Length - length));
            for (var byteOffset = 0; byteOffset < length; ++byteOffset)
                hashCode.Add((int)Unsafe.AddByteOffset(ref local2, (nint)byteOffset));
        }
#endif

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash32<T>(in T obj) where T : unmanaged => ComputeHash32(obj, DefaultSeed);

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash32<T>(in T obj, uint seed) where T : unmanaged => ComputeHash32(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in obj)), sizeof(T)), seed);

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash32<T>(ReadOnlySpan<T> buffer) where T : unmanaged => ComputeHash32(buffer, DefaultSeed);

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash32<T>(ReadOnlySpan<T> buffer, uint seed) where T : unmanaged => ComputeHash32(MemoryMarshal.Cast<T, byte>(buffer), seed);

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash32(ReadOnlySpan<byte> buffer) => ComputeHash32(buffer, DefaultSeed);

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash32(ReadOnlySpan<byte> buffer, uint seed)
        {
            uint num1 = 0;
            uint num2 = 0;
            uint num3 = 0;
            uint num4 = 0;
            uint num5 = 0;
            uint num6 = 0;
            uint num7 = 0;
            uint num8 = 0;
            ref var local1 = ref MemoryMarshal.GetReference(buffer);
#if NET6_0_OR_GREATER
            ref var local2 = ref Unsafe.Add(ref local1, buffer.Length);
#else
            ref var local2 = ref Unsafe.Add(ref local1, (nint)buffer.Length);
#endif
            if (buffer.Length >= 16)
            {
                num1 = (uint)((int)seed - 1640531535 - 2048144777);
                num2 = seed + 2246822519U;
                num3 = seed;
                num4 = seed - 2654435761U;
                const nint elementOffset1 = 16;
                for (ref var local3 = ref Unsafe.Subtract(ref local2, Unsafe.ByteOffset(ref local1, ref local2) % elementOffset1); Unsafe.IsAddressLessThan(ref local1, ref local3); local1 = ref Unsafe.Add(ref local1, elementOffset1))
                {
                    var num9 = num1 + Unsafe.ReadUnaligned<uint>(ref local1) * 2246822519U;
                    num1 = (uint)((((int)num9 << 13) | (int)(num9 >> 19)) * -1640531535);
                    var num10 = num2 + Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref local1, 4)) * 2246822519U;
                    num2 = (uint)((((int)num10 << 13) | (int)(num10 >> 19)) * -1640531535);
                    var num11 = num3 + Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref local1, 8)) * 2246822519U;
                    num3 = (uint)((((int)num11 << 13) | (int)(num11 >> 19)) * -1640531535);
                    var num12 = num4 + Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref local1, 12)) * 2246822519U;
                    num4 = (uint)((((int)num12 << 13) | (int)(num12 >> 19)) * -1640531535);
                    num8 += 4U;
                }
            }

            const nint elementOffset2 = 4;
            for (; Unsafe.ByteOffset(ref local1, ref local2) >= elementOffset2; local1 = ref Unsafe.Add(ref local1, elementOffset2))
            {
                var num13 = (uint)Unsafe.ReadUnaligned<int>(ref local1);
                var num14 = num8++;
                switch (num14 % 4U)
                {
                    case 0:
                        num5 = num13;
                        break;
                    case 1:
                        num6 = num13;
                        break;
                    case 2:
                        num7 = num13;
                        break;
                    default:
                        if (num14 == 3U)
                        {
                            num1 = (uint)((int)seed - 1640531535 - 2048144777);
                            num2 = seed + 2246822519U;
                            num3 = seed;
                            num4 = seed - 2654435761U;
                        }

                        var num15 = num1 + num5 * 2246822519U;
                        num1 = (uint)((((int)num15 << 13) | (int)(num15 >> 19)) * -1640531535);
                        var num16 = num2 + num6 * 2246822519U;
                        num2 = (uint)((((int)num16 << 13) | (int)(num16 >> 19)) * -1640531535);
                        var num17 = num3 + num7 * 2246822519U;
                        num3 = (uint)((((int)num17 << 13) | (int)(num17 >> 19)) * -1640531535);
                        var num18 = num4 + num13 * 2246822519U;
                        num4 = (uint)((((int)num18 << 13) | (int)(num18 >> 19)) * -1640531535);
                        break;
                }
            }

            for (; Unsafe.IsAddressLessThan(ref local1, ref local2); local1 = ref Unsafe.Add(ref local1, 1))
            {
                uint num19 = local1;
                var num20 = num8++;
                switch (num20 % 4U)
                {
                    case 0:
                        num5 = num19;
                        break;
                    case 1:
                        num6 = num19;
                        break;
                    case 2:
                        num7 = num19;
                        break;
                    default:
                        if (num20 == 3U)
                        {
                            num1 = (uint)((int)seed - 1640531535 - 2048144777);
                            num2 = seed + 2246822519U;
                            num3 = seed;
                            num4 = seed - 2654435761U;
                        }

                        var num21 = num1 + num5 * 2246822519U;
                        num1 = (uint)((((int)num21 << 13) | (int)(num21 >> 19)) * -1640531535);
                        var num22 = num2 + num6 * 2246822519U;
                        num2 = (uint)((((int)num22 << 13) | (int)(num22 >> 19)) * -1640531535);
                        var num23 = num3 + num7 * 2246822519U;
                        num3 = (uint)((((int)num23 << 13) | (int)(num23 >> 19)) * -1640531535);
                        var num24 = num4 + num19 * 2246822519U;
                        num4 = (uint)((((int)num24 << 13) | (int)(num24 >> 19)) * -1640531535);
                        break;
                }
            }

            var num25 = num8;
            var num26 = num25 % 4U;
            var num27 = (uint)((num25 < 4U ? (int)seed + 374761393 : (((int)num1 << 1) | (int)(num1 >> 31)) + (((int)num2 << 7) | (int)(num2 >> 25)) + (((int)num3 << 12) | (int)(num3 >> 20)) + (((int)num4 << 18) | (int)(num4 >> 14))) + (int)num25 * 4);
            if (num26 > 0U)
            {
                var num28 = num27 + num5 * 3266489917U;
                num27 = (uint)((((int)num28 << 17) | (int)(num28 >> 15)) * 668265263);
                if (num26 > 1U)
                {
                    var num29 = num27 + num6 * 3266489917U;
                    num27 = (uint)((((int)num29 << 17) | (int)(num29 >> 15)) * 668265263);
                    if (num26 > 2U)
                    {
                        var num30 = num27 + num7 * 3266489917U;
                        num27 = (uint)((((int)num30 << 17) | (int)(num30 >> 15)) * 668265263);
                    }
                }
            }

            var num31 = (int)num27;
#if NET7_0_OR_GREATER
            var num32 = (num31 ^ (num31 >>> 15)) * -2048144777;
            var num33 = (num32 ^ (num32 >>> 13)) * -1028477379;
            return num33 ^ (num33 >>> 16);
#else
            var num32 = (num31 ^ (int)((uint)num31 >> 15)) * -2048144777;
            var num33 = (num32 ^ (int)((uint)num32 >> 13)) * -1028477379;
            return num33 ^ (int)((uint)num33 >> 16);
#endif
        }

        /// <summary>
        ///     Hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash32<T>(in T obj) where T : unmanaged => Hash32(obj, DefaultSeed);

        /// <summary>
        ///     Hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash32<T>(in T obj, uint seed) where T : unmanaged => Hash32(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in obj)), sizeof(T)), seed);

        /// <summary>
        ///     Hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash32<T>(ReadOnlySpan<T> buffer) where T : unmanaged => Hash32(buffer, DefaultSeed);

        /// <summary>
        ///     Hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash32<T>(ReadOnlySpan<T> buffer, uint seed) where T : unmanaged => Hash32(MemoryMarshal.Cast<T, byte>(buffer), seed);

        /// <summary>
        ///     Hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash32(ReadOnlySpan<byte> buffer) => Hash32(buffer, DefaultSeed);

        /// <summary>
        ///     Hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash32(ReadOnlySpan<byte> buffer, uint seed)
        {
            var length = buffer.Length;
            ref var local1 = ref MemoryMarshal.GetReference(buffer);
            uint num1;
            if (buffer.Length >= 16)
            {
                var num2 = seed + 606290984U;
                var num3 = seed + 2246822519U;
                var num4 = seed;
                var num5 = seed - 2654435761U;
                for (; length >= 16; length -= 16)
                {
                    const nint elementOffset1 = 4;
                    const nint elementOffset2 = 8;
                    const nint elementOffset3 = 12;
                    nint byteOffset = buffer.Length - length;
                    ref var local2 = ref Unsafe.AddByteOffset(ref local1, byteOffset);
                    var num6 = num2 + Unsafe.ReadUnaligned<uint>(ref local2) * 2246822519U;
                    num2 = (uint)((((int)num6 << 13) | (int)(num6 >> 19)) * -1640531535);
                    var num7 = num3 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local2, elementOffset1)) * 2246822519U;
                    num3 = (uint)((((int)num7 << 13) | (int)(num7 >> 19)) * -1640531535);
                    var num8 = num4 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local2, elementOffset2)) * 2246822519U;
                    num4 = (uint)((((int)num8 << 13) | (int)(num8 >> 19)) * -1640531535);
                    var num9 = num5 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local2, elementOffset3)) * 2246822519U;
                    num5 = (uint)((((int)num9 << 13) | (int)(num9 >> 19)) * -1640531535);
                }

                num1 = (uint)((((int)num2 << 1) | (int)(num2 >> 31)) + (((int)num3 << 7) | (int)(num3 >> 25)) + (((int)num4 << 12) | (int)(num4 >> 20)) + (((int)num5 << 18) | (int)(num5 >> 14)) + buffer.Length);
            }
            else
                num1 = (uint)((int)seed + 374761393 + buffer.Length);

            for (; length >= 4; length -= 4)
            {
                nint byteOffset = buffer.Length - length;
                var num10 = Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local1, byteOffset));
                var num11 = num1 + num10 * 3266489917U;
                num1 = (uint)((((int)num11 << 17) | (int)(num11 >> 15)) * 668265263);
            }

            nint byteOffset1 = buffer.Length - length;
            ref var local3 = ref Unsafe.AddByteOffset(ref local1, byteOffset1);
            for (var index = 0; index < length; ++index)
            {
                nint byteOffset2 = index;
                uint num12 = Unsafe.AddByteOffset(ref local3, byteOffset2);
                var num13 = num1 + num12 * 374761393U;
                num1 = (uint)((((int)num13 << 11) | (int)(num13 >> 21)) * -1640531535);
            }

#if NET7_0_OR_GREATER
            var num14 = ((int)num1 ^ (int)(num1 >> 15)) * -2048144777;
            var num15 = (num14 ^ (num14 >>> 13)) * -1028477379;
            return num15 ^ (num15 >>> 16);
#else
            var num14 = ((int)num1 ^ (int)(num1 >> 15)) * -2048144777;
            var num15 = (num14 ^ (int)((uint)num14 >> 13)) * -1028477379;
            return num15 ^ (int)((uint)num15 >> 16);
#endif
        }

        /// <summary>
        ///     Hash 64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Hash64<T>(in T obj) where T : unmanaged => Hash64(obj, DefaultSeed);

        /// <summary>
        ///     Hash 64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Hash64<T>(in T obj, ulong seed) where T : unmanaged => Hash64(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in obj)), sizeof(T)), seed);

        /// <summary>
        ///     Hash 64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Hash64<T>(ReadOnlySpan<T> buffer) where T : unmanaged => Hash64(buffer, DefaultSeed);

        /// <summary>
        ///     Hash 64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Hash64<T>(ReadOnlySpan<T> buffer, ulong seed) where T : unmanaged => Hash64(MemoryMarshal.Cast<T, byte>(buffer), seed);

        /// <summary>
        ///     Hash 64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Hash64(ReadOnlySpan<byte> buffer) => Hash64(buffer, DefaultSeed);

        /// <summary>
        ///     Hash 64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Hash64(ReadOnlySpan<byte> buffer, ulong seed)
        {
            ref var local1 = ref MemoryMarshal.GetReference(buffer);
            var length = buffer.Length;
            ulong num1;
            if (buffer.Length >= 32)
            {
                var num2 = seed + 6983438078262162902UL;
                var num3 = seed + 14029467366897019727UL;
                var num4 = seed;
                var num5 = seed - 11400714785074694791UL;
                for (; length >= 32; length -= 32)
                {
                    const nint elementOffset1 = 8;
                    const nint elementOffset2 = 16;
                    const nint elementOffset3 = 24;
                    nint byteOffset = buffer.Length - length;
                    ref var local2 = ref Unsafe.AddByteOffset(ref local1, byteOffset);
                    var num6 = num2 + Unsafe.ReadUnaligned<ulong>(ref local2) * 14029467366897019727UL;
                    num2 = (ulong)((((long)num6 << 31) | (long)(num6 >> 33)) * -7046029288634856825L);
                    var num7 = num3 + Unsafe.ReadUnaligned<ulong>(ref Unsafe.AddByteOffset(ref local2, elementOffset1)) * 14029467366897019727UL;
                    num3 = (ulong)((((long)num7 << 31) | (long)(num7 >> 33)) * -7046029288634856825L);
                    var num8 = num4 + Unsafe.ReadUnaligned<ulong>(ref Unsafe.AddByteOffset(ref local2, elementOffset2)) * 14029467366897019727UL;
                    num4 = (ulong)((((long)num8 << 31) | (long)(num8 >> 33)) * -7046029288634856825L);
                    var num9 = num5 + Unsafe.ReadUnaligned<ulong>(ref Unsafe.AddByteOffset(ref local2, elementOffset3)) * 14029467366897019727UL;
                    num5 = (ulong)((((long)num9 << 31) | (long)(num9 >> 33)) * -7046029288634856825L);
                }

                var num10 = (((long)num2 << 1) | (long)(num2 >> 63)) + (((long)num3 << 7) | (long)(num3 >> 57)) + (((long)num4 << 12) | (long)(num4 >> 52)) + (((long)num5 << 18) | (long)(num5 >> 46));
                var num11 = num2 * 14029467366897019727UL;
                var num12 = (((long)num11 << 31) | (long)(num11 >> 33)) * -7046029288634856825L;
                var num13 = (num10 ^ num12) * -7046029288634856825L + -8796714831421723037L;
                var num14 = num3 * 14029467366897019727UL;
                var num15 = (((long)num14 << 31) | (long)(num14 >> 33)) * -7046029288634856825L;
                var num16 = (num13 ^ num15) * -7046029288634856825L + -8796714831421723037L;
                var num17 = num4 * 14029467366897019727UL;
                var num18 = (((long)num17 << 31) | (long)(num17 >> 33)) * -7046029288634856825L;
                var num19 = (num16 ^ num18) * -7046029288634856825L + -8796714831421723037L;
                var num20 = num5 * 14029467366897019727UL;
                var num21 = (((long)num20 << 31) | (long)(num20 >> 33)) * -7046029288634856825L;
                num1 = (ulong)((num19 ^ num21) * -7046029288634856825L + -8796714831421723037L);
            }
            else
                num1 = seed + 2870177450012600261UL;

            var num22 = num1 + (ulong)buffer.Length;
            for (; length >= 8; length -= 8)
            {
                nint byteOffset = buffer.Length - length;
                var num23 = Unsafe.ReadUnaligned<ulong>(ref Unsafe.AddByteOffset(ref local1, byteOffset)) * 14029467366897019727UL;
                var num24 = (ulong)((((long)num23 << 31) | (long)(num23 >> 33)) * -7046029288634856825L);
                var num25 = num22 ^ num24;
                num22 = (ulong)((((long)num25 << 27) | (long)(num25 >> 37)) * -7046029288634856825L + -8796714831421723037L);
            }

            if (length >= 4)
            {
                nint byteOffset = buffer.Length - length;
                ulong num26 = Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local1, byteOffset));
                var num27 = num22 ^ (num26 * 11400714785074694791UL);
                num22 = (ulong)((((long)num27 << 23) | (long)(num27 >> 41)) * -4417276706812531889L + 1609587929392839161L);
                length -= 4;
            }

            nint byteOffset1 = buffer.Length - length;
            ref var local3 = ref Unsafe.AddByteOffset(ref local1, byteOffset1);
            for (var index = 0; index < length; ++index)
            {
                nint byteOffset2 = index;
                ulong num28 = Unsafe.AddByteOffset(ref local3, byteOffset2);
                var num29 = num22 ^ (num28 * 2870177450012600261UL);
                num22 = (ulong)((((long)num29 << 11) | (long)(num29 >> 53)) * -7046029288634856825L);
            }

            var num30 = (long)num22;
#if NET7_0_OR_GREATER
            var num31 = (num30 ^ (num30 >>> 33)) * -4417276706812531889L;
            var num32 = (num31 ^ (num31 >>> 29)) * 1609587929392839161L;
            return num32 ^ (num32 >>> 32);
#else
            var num31 = (num30 ^ (long)((ulong)num30 >> 33)) * -4417276706812531889L;
            var num32 = (num31 ^ (long)((ulong)num31 >> 29)) * 1609587929392839161L;
            return num32 ^ (long)((ulong)num32 >> 32);
#endif
        }
    }
}