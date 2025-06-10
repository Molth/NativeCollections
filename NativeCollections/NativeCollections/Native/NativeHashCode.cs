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
    ///     Native hashCode
    /// </summary>
    [Customizable("public static int GetHashCode(ReadOnlySpan<byte> buffer)")]
    public static unsafe class NativeHashCode
    {
        /// <summary>
        ///     Default seed
        /// </summary>
        private static readonly uint DefaultSeed;

        /// <summary>
        ///     GetHashCode
        /// </summary>
        private static delegate* managed<ReadOnlySpan<byte>, int> _getHashCode;

        /// <summary>
        ///     Structure
        /// </summary>
        static NativeHashCode()
        {
            try
            {
                var type = typeof(HashCode).Assembly.GetType("System.HashCode");
                if (type != null)
                {
                    var field = type.GetField("s_seed", BindingFlags.Static | BindingFlags.NonPublic);
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
            }
            catch
            {
                //
            }

            uint num;
            NativeRandom.Next(&num, 4);
            DefaultSeed = num;
        }

        /// <summary>
        ///     Custom GetHashCode
        /// </summary>
        /// <param name="getHashCode">GetHashCode</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<ReadOnlySpan<byte>, int> getHashCode) => _getHashCode = getHashCode;

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(in T obj) where T : unmanaged => GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in obj)), sizeof(T)));

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(ReadOnlySpan<T> buffer) where T : unmanaged => GetHashCode(MemoryMarshal.Cast<T, byte>(buffer));

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(void* ptr, int byteCount) => GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref *(byte*)ptr, byteCount));

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<byte> buffer)
        {
            var getHashCode = _getHashCode;
            if (getHashCode != null)
                return getHashCode(buffer);

            return ComputeHash32(buffer, DefaultSeed);
        }

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ComputeHash32(ReadOnlySpan<byte> buffer, uint seed)
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
            nint byteOffset1 = buffer.Length;
            ref var local2 = ref Unsafe.AddByteOffset(ref local1, byteOffset1);
            if (buffer.Length >= 16)
            {
                num1 = (uint)((int)seed - 1640531535 - 2048144777);
                num2 = seed + 2246822519U;
                num3 = seed;
                num4 = seed - 2654435761U;
                const nint byteOffset2 = 16;
                for (ref var local3 = ref Unsafe.SubtractByteOffset(ref local2, Unsafe.ByteOffset(ref local1, ref local2) % byteOffset2); Unsafe.IsAddressLessThan(ref local1, ref local3); local1 = ref Unsafe.AddByteOffset(ref local1, byteOffset2))
                {
                    const nint byteOffset3 = 4;
                    const nint byteOffset4 = 8;
                    const nint byteOffset5 = 12;
                    var num9 = num1 + Unsafe.ReadUnaligned<uint>(ref local1) * 2246822519U;
                    num1 = (uint)((((int)num9 << 13) | (int)(num9 >> 19)) * -1640531535);
                    var num10 = num2 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local1, byteOffset3)) * 2246822519U;
                    num2 = (uint)((((int)num10 << 13) | (int)(num10 >> 19)) * -1640531535);
                    var num11 = num3 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local1, byteOffset4)) * 2246822519U;
                    num3 = (uint)((((int)num11 << 13) | (int)(num11 >> 19)) * -1640531535);
                    var num12 = num4 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local1, byteOffset5)) * 2246822519U;
                    num4 = (uint)((((int)num12 << 13) | (int)(num12 >> 19)) * -1640531535);
                    num8 += 4U;
                }
            }

            const nint byteOffset6 = 4;
            for (; Unsafe.ByteOffset(ref local1, ref local2) >= byteOffset6; local1 = ref Unsafe.AddByteOffset(ref local1, byteOffset6))
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

            const nint byteOffset7 = 1;
            for (; Unsafe.IsAddressLessThan(ref local1, ref local2); local1 = ref Unsafe.AddByteOffset(ref local1, byteOffset7))
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
    }
}