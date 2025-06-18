using System;
using System.Runtime.CompilerServices;
#if !NET7_0_OR_GREATER
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
    }
}