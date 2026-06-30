// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*

The xxHash32 implementation is based on the code published by Yann Collet:
https://raw.githubusercontent.com/Cyan4973/xxHash/5c174cfa4e45a42f94082dc0d4539b39696afea1/xxhash.c

  xxHash - Fast Hash algorithm
  Copyright (C) 2012-2016, Yann Collet

  BSD 2-Clause License (http://www.opensource.org/licenses/bsd-license.php)

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions are
  met:

  * Redistributions of source code must retain the above copyright
  notice, this list of conditions and the following disclaimer.
  * Redistributions in binary form must reproduce the above
  copyright notice, this list of conditions and the following disclaimer
  in the documentation and/or other materials provided with the
  distribution.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

  You can contact the author at :
  - xxHash homepage: http://www.xxhash.com
  - xxHash source repository : https://github.com/Cyan4973/xxHash

*/

using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    // xxHash32 is used for the hash code.
    // https://github.com/Cyan4973/xxHash
    internal static class XxHash32
    {
        /// <summary>Computes the XxHash32 hash of the provided data.</summary>
        /// <param name="startAddress">The data to hash.</param>
        /// <param name="byteCount">The data's length to hash.</param>
        /// <param name="seed">The seed value for this hash computation. The default is zero.</param>
        /// <returns>The computed XxHash32 hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int HashToInt32(ref byte startAddress, nuint byteCount, uint seed)
        {
            const nint byteOffset1 = 16;
            const nint byteOffset2 = 4;
            const nint byteOffset3 = 8;
            const nint byteOffset4 = 12;
            const nint byteOffset5 = 1;
            uint num1 = 0;
            uint num2 = 0;
            uint num3 = 0;
            uint num4 = 0;
            uint num5 = 0;
            uint num6 = 0;
            uint num7 = 0;
            uint num8 = 0;
            ref var endAddress = ref Unsafe.AddByteOffset(ref startAddress, (nint)byteCount);
            if (byteCount >= 16)
            {
                num1 = (uint)((int)seed - 1640531535 - 2048144777);
                num2 = seed + 2246822519U;
                num3 = seed;
                num4 = seed - 2654435761U;
                for (ref var local3 = ref Unsafe.SubtractByteOffset(ref endAddress, Unsafe.ByteOffset(ref startAddress, ref endAddress) % byteOffset1); Unsafe.IsAddressLessThan(ref startAddress, ref local3); startAddress = ref Unsafe.AddByteOffset(ref startAddress, byteOffset1))
                {
                    var num9 = num1 + Unsafe.ReadUnaligned<uint>(ref startAddress) * 2246822519U;
                    num1 = (uint)((((int)num9 << 13) | (int)(num9 >> 19)) * -1640531535);
                    var num10 = num2 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref startAddress, byteOffset2)) * 2246822519U;
                    num2 = (uint)((((int)num10 << 13) | (int)(num10 >> 19)) * -1640531535);
                    var num11 = num3 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref startAddress, byteOffset3)) * 2246822519U;
                    num3 = (uint)((((int)num11 << 13) | (int)(num11 >> 19)) * -1640531535);
                    var num12 = num4 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref startAddress, byteOffset4)) * 2246822519U;
                    num4 = (uint)((((int)num12 << 13) | (int)(num12 >> 19)) * -1640531535);
                    num8 += 4U;
                }
            }

            for (; Unsafe.ByteOffset(ref startAddress, ref endAddress) >= byteOffset2; startAddress = ref Unsafe.AddByteOffset(ref startAddress, byteOffset2))
            {
                var num13 = (uint)Unsafe.ReadUnaligned<int>(ref startAddress);
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

            for (; Unsafe.IsAddressLessThan(ref startAddress, ref endAddress); startAddress = ref Unsafe.AddByteOffset(ref startAddress, byteOffset5))
            {
                uint num19 = startAddress;
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