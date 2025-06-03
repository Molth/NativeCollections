#if !NET7_0_OR_GREATER
using System;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory allocator
    /// </summary>
    [Customizable("public static void* Alloc(uint byteCount)", "public static void* AllocZeroed(uint byteCount)", "public static void Free(void* ptr)")]
    public static unsafe class NativeMemoryAllocator
    {
        /// <summary>
        ///     Alloc
        /// </summary>
        private static delegate* managed<uint, void*> _alloc;

        /// <summary>
        ///     AllocZeroed
        /// </summary>
        private static delegate* managed<uint, void*> _allocZeroed;

        /// <summary>
        ///     Free
        /// </summary>
        private static delegate* managed<void*, void> _free;

        /// <summary>
        ///     Custom allocator
        /// </summary>
        /// <param name="alloc">Alloc</param>
        /// <param name="allocZeroed">AllocZeroed</param>
        /// <param name="free">Free</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<uint, void*> alloc, delegate* managed<uint, void*> allocZeroed, delegate* managed<void*, void> free)
        {
            _alloc = alloc;
            _allocZeroed = allocZeroed;
            _free = free;
        }

        /// <summary>
        ///     Align
        /// </summary>
        /// <param name="size">Size</param>
        /// <returns>Aligned</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Align(nuint size) => AlignUp(size, (nuint)sizeof(nint));

        /// <summary>
        ///     Align up
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="alignment">Alignment</param>
        /// <returns>Aligned size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignUp(nuint size, nuint alignment) => (size + (alignment - 1)) & ~(alignment - 1);

        /// <summary>
        ///     Align down
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="alignment">Alignment</param>
        /// <returns>Aligned size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignDown(nuint size, nuint alignment) => size - (size & (alignment - 1));

        /// <summary>
        ///     Alloc
        /// </summary>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(uint byteCount)
        {
            var alloc = _alloc;
            if (alloc != null)
                return alloc(byteCount);

#if NET6_0_OR_GREATER
            return NativeMemory.Alloc(byteCount);
#else
            return (void*)Marshal.AllocHGlobal((nint)byteCount);
#endif
        }

        /// <summary>
        ///     Alloc zeroed
        /// </summary>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocZeroed(uint byteCount)
        {
            var allocZeroed = _allocZeroed;
            if (allocZeroed != null)
                return allocZeroed(byteCount);

            void* ptr;
            var alloc = _alloc;
            if (alloc != null)
            {
                ptr = alloc(byteCount);
                Unsafe.InitBlockUnaligned(ptr, 0, byteCount);
                return ptr;
            }

#if NET6_0_OR_GREATER
            return NativeMemory.AllocZeroed(byteCount, 1);
#else
            ptr = (void*)Marshal.AllocHGlobal((nint)byteCount);
            Unsafe.InitBlockUnaligned(ptr, 0, byteCount);
            return ptr;
#endif
        }

        /// <summary>
        ///     Free
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* ptr)
        {
            var free = _free;
            if (free != null)
            {
                free(ptr);
                return;
            }

#if NET6_0_OR_GREATER
            NativeMemory.Free(ptr);
#else
            Marshal.FreeHGlobal((nint)ptr);
#endif
        }

        /// <summary>
        ///     Copy
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="source">Source</param>
        /// <param name="byteCount">Byte count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(void* destination, void* source, uint byteCount) => Unsafe.CopyBlockUnaligned(destination, source, byteCount);

        /// <summary>
        ///     Move
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="source">Source</param>
        /// <param name="byteCount">Byte count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(void* destination, void* source, uint byteCount)
        {
#if NET7_0_OR_GREATER
            NativeMemory.Copy(source, destination, byteCount);
#else
            Buffer.MemoryCopy(source, destination, byteCount, byteCount);
#endif
        }

        /// <summary>
        ///     Set
        /// </summary>
        /// <param name="startAddress">Start address</param>
        /// <param name="value">Value</param>
        /// <param name="byteCount">Byte count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(void* startAddress, byte value, uint byteCount) => Unsafe.InitBlockUnaligned(startAddress, value, byteCount);

        /// <summary>
        ///     Compare
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Sequences equal</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(void* left, void* right, uint byteCount)
        {
            ref var local1 = ref Unsafe.AsRef<byte>(left);
            ref var local2 = ref Unsafe.AsRef<byte>(right);
#if NET7_0_OR_GREATER
            nuint length = byteCount;
            if (length >= (nuint)sizeof(nuint))
            {
                if (!Unsafe.AreSame(ref local1, ref local2))
                {
                    if (Vector128.IsHardwareAccelerated)
                    {
#if NET8_0_OR_GREATER
                        if (Vector512.IsHardwareAccelerated && length >= (nuint)Vector512<byte>.Count)
                        {
                            nuint offset = 0;
                            var lengthToExamine = length - (nuint)Vector512<byte>.Count;
                            if (lengthToExamine != 0)
                            {
                                do
                                {
                                    if (Vector512.LoadUnsafe(ref local1, offset) != Vector512.LoadUnsafe(ref local2, offset))
                                        return false;
                                    offset += (nuint)Vector512<byte>.Count;
                                } while (lengthToExamine > offset);
                            }

                            return Vector512.LoadUnsafe(ref local1, lengthToExamine) == Vector512.LoadUnsafe(ref local2, lengthToExamine);
                        }
#endif
                        if (Vector256.IsHardwareAccelerated && length >= (nuint)Vector256<byte>.Count)
                        {
                            nuint offset = 0;
                            var lengthToExamine = length - (nuint)Vector256<byte>.Count;
                            if (lengthToExamine != 0)
                            {
                                do
                                {
                                    if (Vector256.LoadUnsafe(ref local1, offset) != Vector256.LoadUnsafe(ref local2, offset))
                                        return false;
                                    offset += (nuint)Vector256<byte>.Count;
                                } while (lengthToExamine > offset);
                            }

                            return Vector256.LoadUnsafe(ref local1, lengthToExamine) == Vector256.LoadUnsafe(ref local2, lengthToExamine);
                        }

                        if (length >= (nuint)Vector128<byte>.Count)
                        {
                            nuint offset = 0;
                            var lengthToExamine = length - (nuint)Vector128<byte>.Count;
                            if (lengthToExamine != 0)
                            {
                                do
                                {
                                    if (Vector128.LoadUnsafe(ref local1, offset) != Vector128.LoadUnsafe(ref local2, offset))
                                        return false;
                                    offset += (nuint)Vector128<byte>.Count;
                                } while (lengthToExamine > offset);
                            }

                            return Vector128.LoadUnsafe(ref local1, lengthToExamine) == Vector128.LoadUnsafe(ref local2, lengthToExamine);
                        }
                    }

                    if (sizeof(nint) == 8 && Vector128.IsHardwareAccelerated)
                    {
                        var offset = length - (nuint)sizeof(nuint);
                        var differentBits = Unsafe.ReadUnaligned<nuint>(ref local1) - Unsafe.ReadUnaligned<nuint>(ref local2);
                        differentBits |= Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref local1, offset)) - Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref local2, offset));
                        return differentBits == 0;
                    }
                    else
                    {
                        nuint offset = 0;
                        var lengthToExamine = length - (nuint)sizeof(nuint);
                        if (lengthToExamine > 0)
                        {
                            do
                            {
                                if (Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref local1, offset)) != Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref local2, offset)))
                                    return false;
                                offset += (nuint)sizeof(nuint);
                            } while (lengthToExamine > offset);
                        }

                        return Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref local1, lengthToExamine)) == Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref local2, lengthToExamine));
                    }
                }

                return true;
            }

            if (length < sizeof(uint) || sizeof(nint) != 8)
            {
                uint differentBits = 0;
                var offset = length & 2;
                if (offset != 0)
                {
                    differentBits = Unsafe.ReadUnaligned<ushort>(ref local1);
                    differentBits -= Unsafe.ReadUnaligned<ushort>(ref local2);
                }

                if ((length & 1) != 0)
                    differentBits |= Unsafe.AddByteOffset(ref local1, offset) - (uint)Unsafe.AddByteOffset(ref local2, offset);
                return differentBits == 0;
            }
            else
            {
                var offset = length - sizeof(uint);
                var differentBits = Unsafe.ReadUnaligned<uint>(ref local1) - Unsafe.ReadUnaligned<uint>(ref local2);
                differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local1, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local2, offset));
                return differentBits == 0;
            }
#else
            var (quotient, remainder) = MathHelpers.DivRem(byteCount, 1073741824);
            for (uint i = 0; i < quotient; ++i)
            {
                if (!MemoryMarshal.CreateReadOnlySpan(ref local1, 1073741824).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref local2, 1073741824)))
                    return false;
                local1 = ref Unsafe.AddByteOffset(ref local1, (nint)1073741824);
                local2 = ref Unsafe.AddByteOffset(ref local2, (nint)1073741824);
            }

            return MemoryMarshal.CreateReadOnlySpan(ref local1, (int)remainder).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref local2, (int)remainder));
#endif
        }
    }
}