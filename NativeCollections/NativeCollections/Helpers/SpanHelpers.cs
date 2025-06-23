using System;
using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#else
using System.Runtime.InteropServices;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Span helpers
    /// </summary>
    internal static class SpanHelpers
    {
        /// <summary>Searches for any value other than the specified <paramref name="value" />.</summary>
        /// <param name="buffer">The span to search.</param>
        /// <param name="value">The value to exclude from the search.</param>
        /// <typeparam name="T" />
        /// <returns>
        ///     <see langword="true" /> if any value other than <paramref name="value" /> is present in the span.
        ///     If all of the values are <paramref name="value" />, returns <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyExcept<T>(ReadOnlySpan<T> buffer, in T value) where T : unmanaged, IEquatable<T>
        {
#if NET8_0_OR_GREATER
            return buffer.ContainsAnyExcept(value);
#elif NET7_0_OR_GREATER
            return buffer.IndexOfAnyExcept(value) >= 0;
#else
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            for (var i = 0; i < buffer.Length; ++i)
            {
                if (!Unsafe.Add(ref reference, i).Equals(value))
                    return true;
            }

            return false;
#endif
        }

        /// <summary>
        ///     Searches for the specified value and returns true if found. If not found, returns false. Values are compared using
        ///     IEquatable{T}.Equals(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(Span<T> buffer, in T value) where T : unmanaged, IEquatable<T>
        {
#if NET6_0_OR_GREATER
            return buffer.Contains(value);
#else
            return buffer.IndexOf(value) >= 0;
#endif
        }

        /// <summary>
        ///     Searches for the specified value and returns true if found. If not found, returns false. Values are compared using
        ///     IEquatable{T}.Equals(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">The span to search.</param>
        /// <param name="value">The value to search for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(ReadOnlySpan<T> buffer, in T value) where T : unmanaged, IEquatable<T>
        {
#if NET6_0_OR_GREATER
            return buffer.Contains(value);
#else
            return buffer.IndexOf(value) >= 0;
#endif
        }

        /// <summary>
        ///     Determines whether two sequences are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(ref byte left, ref byte right, nuint byteCount)
        {
#if NET7_0_OR_GREATER
            if (byteCount >= (nuint)Unsafe.SizeOf<nuint>())
            {
                if (!Unsafe.AreSame(ref left, ref right))
                {
                    if (Vector128.IsHardwareAccelerated)
                    {
#if NET8_0_OR_GREATER
                        if (Vector512.IsHardwareAccelerated && byteCount >= (nuint)Vector512<byte>.Count)
                        {
                            nuint offset = 0;
                            var lengthToExamine = byteCount - (nuint)Vector512<byte>.Count;
                            if (lengthToExamine != 0)
                            {
                                do
                                {
                                    if (Vector512.LoadUnsafe(ref left, offset) != Vector512.LoadUnsafe(ref right, offset))
                                        return false;
                                    offset += (nuint)Vector512<byte>.Count;
                                } while (lengthToExamine > offset);
                            }

                            return Vector512.LoadUnsafe(ref left, lengthToExamine) == Vector512.LoadUnsafe(ref right, lengthToExamine);
                        }
#endif
                        if (Vector256.IsHardwareAccelerated && byteCount >= (nuint)Vector256<byte>.Count)
                        {
                            nuint offset = 0;
                            var lengthToExamine = byteCount - (nuint)Vector256<byte>.Count;
                            if (lengthToExamine != 0)
                            {
                                do
                                {
                                    if (Vector256.LoadUnsafe(ref left, offset) != Vector256.LoadUnsafe(ref right, offset))
                                        return false;
                                    offset += (nuint)Vector256<byte>.Count;
                                } while (lengthToExamine > offset);
                            }

                            return Vector256.LoadUnsafe(ref left, lengthToExamine) == Vector256.LoadUnsafe(ref right, lengthToExamine);
                        }

                        if (byteCount >= (nuint)Vector128<byte>.Count)
                        {
                            nuint offset = 0;
                            var lengthToExamine = byteCount - (nuint)Vector128<byte>.Count;
                            if (lengthToExamine != 0)
                            {
                                do
                                {
                                    if (Vector128.LoadUnsafe(ref left, offset) != Vector128.LoadUnsafe(ref right, offset))
                                        return false;
                                    offset += (nuint)Vector128<byte>.Count;
                                } while (lengthToExamine > offset);
                            }

                            return Vector128.LoadUnsafe(ref left, lengthToExamine) == Vector128.LoadUnsafe(ref right, lengthToExamine);
                        }
                    }

                    if (Unsafe.SizeOf<nint>() == 8 && Vector128.IsHardwareAccelerated)
                    {
                        var offset = byteCount - (nuint)Unsafe.SizeOf<nuint>();
                        var differentBits = Unsafe.ReadUnaligned<nuint>(ref left) - Unsafe.ReadUnaligned<nuint>(ref right);
                        differentBits |= Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref left, offset)) - Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref right, offset));
                        return differentBits == 0;
                    }
                    else
                    {
                        nuint offset = 0;
                        var lengthToExamine = byteCount - (nuint)Unsafe.SizeOf<nuint>();
                        if (lengthToExamine > 0)
                        {
                            do
                            {
                                if (Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref left, offset)) != Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref right, offset)))
                                    return false;
                                offset += (nuint)Unsafe.SizeOf<nuint>();
                            } while (lengthToExamine > offset);
                        }

                        return Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref left, lengthToExamine)) == Unsafe.ReadUnaligned<nuint>(ref Unsafe.AddByteOffset(ref right, lengthToExamine));
                    }
                }

                return true;
            }

            if (byteCount < sizeof(uint) || Unsafe.SizeOf<nint>() != 8)
            {
                uint differentBits = 0;
                var offset = byteCount & 2;
                if (offset != 0)
                {
                    differentBits = Unsafe.ReadUnaligned<ushort>(ref left);
                    differentBits -= Unsafe.ReadUnaligned<ushort>(ref right);
                }

                if ((byteCount & 1) != 0)
                    differentBits |= Unsafe.AddByteOffset(ref left, offset) - (uint)Unsafe.AddByteOffset(ref right, offset);
                return differentBits == 0;
            }
            else
            {
                var offset = byteCount - sizeof(uint);
                var differentBits = Unsafe.ReadUnaligned<uint>(ref left) - Unsafe.ReadUnaligned<uint>(ref right);
                differentBits |= Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref left, offset)) - Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref right, offset));
                return differentBits == 0;
            }
#else
            var (quotient, remainder) = MathHelpers.DivRem(byteCount, 1073741824);
            for (nuint i = 0; i < quotient; ++i)
            {
                if (!MemoryMarshal.CreateReadOnlySpan(ref left, 1073741824).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref right, 1073741824)))
                    return false;
                left = ref Unsafe.AddByteOffset(ref left, (nint)1073741824);
                right = ref Unsafe.AddByteOffset(ref right, (nint)1073741824);
            }

            return MemoryMarshal.CreateReadOnlySpan(ref left, (int)remainder).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref right, (int)remainder));
#endif
        }
    }
}