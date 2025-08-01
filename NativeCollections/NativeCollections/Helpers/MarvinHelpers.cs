using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8625
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Marvin helpers
    /// </summary>
    internal static class MarvinHelpers
    {
        /// <summary>
        ///     Default seed
        /// </summary>
        public static readonly ulong DefaultSeed = NativeRandom.NextUInt64();

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash32(ReadOnlySpan<byte> data, ulong seed) => ComputeHash32(ref MemoryMarshal.GetReference(data), (uint)data.Length, (uint)seed, (uint)(seed >> 32));

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeHash32(ref byte data, uint count, uint p0, uint p1)
        {
            uint num1;
            if (count < 8U)
            {
                if (count < 4U)
                {
                    num1 = BitConverter.IsLittleEndian ? 128U : 2147483648U;
                    if (((int)count & 1) != 0)
                    {
                        uint num2 = Unsafe.AddByteOffset(ref data, (nint)((nuint)count & 2));
                        num1 = BitConverter.IsLittleEndian ? num2 | 32768U : (uint)(((int)num2 << 24) | 8388608);
                    }

                    if (((int)count & 2) != 0)
                        num1 = !BitConverter.IsLittleEndian ? BitOperationsHelpers.RotateLeft(num1 | Unsafe.ReadUnaligned<ushort>(ref data), 16) : (num1 << 16) | Unsafe.ReadUnaligned<ushort>(ref data);
                    goto label_2;
                }
            }
            else
            {
                var num3 = count / 8U;
                do
                {
                    p0 += Unsafe.ReadUnaligned<uint>(ref data);
                    var num4 = Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref data, new IntPtr(4)));
                    Block(ref p0, ref p1);
                    p0 += num4;
                    Block(ref p0, ref p1);
                    data = ref Unsafe.AddByteOffset(ref data, new IntPtr(8));
                } while (--num3 > 0U);

                if (((int)count & 4) == 0)
                    goto label_1;
            }

            p0 += Unsafe.ReadUnaligned<uint>(ref data);
            Block(ref p0, ref p1);
            label_1:
            var num5 = Unsafe.ReadUnaligned<uint>(ref Unsafe.Subtract(ref Unsafe.AddByteOffset(ref data, (nint)((nuint)count & 7)), 4));
            count = (uint)(~(int)count << 3);
            num1 = BitConverter.IsLittleEndian ? ((num5 >> 8) | 2147483648U) >> (int)count : (uint)(((int)num5 << 8) | 128) << (int)count;
            label_2:
            p0 += num1;
            Block(ref p0, ref p1);
            Block(ref p0, ref p1);
            return (int)p1 ^ (int)p0;
        }

        /// <summary>
        ///     Block
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Block(ref uint rp0, ref uint rp1)
        {
            var num1 = rp0;
            var num2 = rp1 ^ num1;
            var num3 = BitOperationsHelpers.RotateLeft(num1, 20) + num2;
            var num4 = BitOperationsHelpers.RotateLeft(num2, 9) ^ num3;
            var num5 = BitOperationsHelpers.RotateLeft(num3, 27) + num4;
            var num6 = BitOperationsHelpers.RotateLeft(num4, 19);
            rp0 = num5;
            rp1 = num6;
        }
    }
}