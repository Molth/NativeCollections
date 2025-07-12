#if NET7_0_OR_GREATER
using System.Numerics;
using System.Runtime.Intrinsics;
#else
using System.Runtime.InteropServices;
#endif
using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Span helpers
    /// </summary>
    internal static class SpanHelpers
    {
        /// <summary>
        ///     Fills the contents of this buffer with the given value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(ref T refData, nuint numElements, in T value) where T : unmanaged
        {
#if NET7_0_OR_GREATER
            if (!Vector.IsHardwareAccelerated || Unsafe.SizeOf<T>() > Vector<byte>.Count || !BitOperationsHelpers.IsPow2((uint)Unsafe.SizeOf<T>()))
                goto CannotVectorize;
            if (numElements >= (uint)(Vector<byte>.Count / Unsafe.SizeOf<T>()))
            {
                Vector<byte> vector;
                if (Unsafe.SizeOf<T>() == 1)
                {
                    vector = new Vector<byte>(UnsafeHelpers.BitCast<T, byte>(value));
                }
                else if (Unsafe.SizeOf<T>() == 2)
                {
                    vector = (Vector<byte>)new Vector<ushort>(UnsafeHelpers.BitCast<T, ushort>(value));
                }
                else if (Unsafe.SizeOf<T>() == 4)
                {
                    vector = typeof(T) == typeof(float) ? (Vector<byte>)new Vector<float>(UnsafeHelpers.BitCast<T, float>(value)) : (Vector<byte>)new Vector<uint>(UnsafeHelpers.BitCast<T, uint>(value));
                }
                else if (Unsafe.SizeOf<T>() == 8)
                {
                    vector = typeof(T) == typeof(double) ? (Vector<byte>)new Vector<double>(UnsafeHelpers.BitCast<T, double>(value)) : (Vector<byte>)new Vector<ulong>(UnsafeHelpers.BitCast<T, ulong>(value));
                }
                else if (Unsafe.SizeOf<T>() == Vector<byte>.Count)
                {
                    vector = UnsafeHelpers.BitCast<T, Vector<byte>>(value);
                }
                else if (Unsafe.SizeOf<T>() == 16)
                {
                    if (Vector<byte>.Count == 32)
                    {
#if NET9_0_OR_GREATER
                        vector = Vector256.Create(UnsafeHelpers.BitCast<T, Vector128<byte>>(value)).AsVector();
#else
                        var vector128 = UnsafeHelpers.BitCast<T, Vector128<byte>>(value);
                        vector = Vector256.Create(vector128, vector128).AsVector();
#endif
                    }
#if NET8_0_OR_GREATER
                    else if (Vector<byte>.Count == 64)
                    {
#if NET9_0_OR_GREATER
                        vector = Vector512.Create(UnsafeHelpers.BitCast<T, Vector128<byte>>(value)).AsVector();
#else
                        var vector128 = UnsafeHelpers.BitCast<T, Vector128<byte>>(value);
                        var vector256 = Vector256.Create(vector128, vector128);
                        vector = Vector512.Create(vector256, vector256).AsVector();
#endif
                    }
#endif
                    else
                        goto CannotVectorize;
                }
#if NET8_0_OR_GREATER
                else if (Unsafe.SizeOf<T>() == 32 && Vector<byte>.Count == 64)
                {
#if NET9_0_OR_GREATER
                    vector = Vector512.Create(UnsafeHelpers.BitCast<T, Vector256<byte>>(value)).AsVector();
#else
                    var vector256 = UnsafeHelpers.BitCast<T, Vector256<byte>>(value);
                    vector = Vector512.Create(vector256, vector256).AsVector();
#endif
                }
#endif
                else
                    goto CannotVectorize;

                ref var refDataAsBytes = ref Unsafe.As<T, byte>(ref refData);
                var totalByteLength = numElements * (nuint)Unsafe.SizeOf<T>();
                var stopLoopAtOffset = totalByteLength & (nuint)(2 * -Vector<byte>.Count);
                nuint offset = 0;
                if (numElements >= (uint)(2 * Vector<byte>.Count / Unsafe.SizeOf<T>()))
                {
                    do
                    {
                        Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref refDataAsBytes, offset), vector);
                        Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref refDataAsBytes, offset + (nuint)Vector<byte>.Count), vector);
                        offset += (uint)(2 * Vector<byte>.Count);
                    } while (offset < stopLoopAtOffset);
                }

                if ((totalByteLength & (nuint)Vector<byte>.Count) != 0)
                    Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref refDataAsBytes, offset), vector);
                Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref refDataAsBytes, totalByteLength - (nuint)Vector<byte>.Count), vector);
                return;
            }

            CannotVectorize:
#endif
            nuint i = 0;
            if (numElements >= 8)
            {
                var stopLoopAtOffset = numElements & ~(nuint)7;
                do
                {
                    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 0)), value);
                    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 1)), value);
                    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 2)), value);
                    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 3)), value);
                    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 4)), value);
                    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 5)), value);
                    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 6)), value);
                    Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 7)), value);
                } while ((i += 8) < stopLoopAtOffset);
            }

            if ((numElements & 4) != 0)
            {
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 0)), value);
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 1)), value);
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 2)), value);
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 3)), value);
                i += 4;
            }

            if ((numElements & 2) != 0)
            {
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 0)), value);
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i + 1)), value);
                i += 2;
            }

            if ((numElements & 1) != 0)
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref refData, (nint)i)), value);
        }

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
                if (!Unsafe.Add(ref reference, (nint)i).Equals(value))
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
            var quotient = byteCount >> 30;
            var remainder = byteCount & 1073741823;
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