#if NET6_0_OR_GREATER
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     DefaultInterpolatedStringHandler helpers
    /// </summary>
    internal static class DefaultInterpolatedStringHandlerHelpers
    {
        /// <summary>
        ///     Get text
        /// </summary>
        private static readonly GetTextFunc? GetText;

        /// <summary>
        ///     Clear
        /// </summary>
        private static readonly ClearAction? Clear;

        /// <summary>
        ///     Structure
        /// </summary>
        static DefaultInterpolatedStringHandlerHelpers()
        {
            var getTextProperty = typeof(DefaultInterpolatedStringHandler).GetProperty("Text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (getTextProperty == null)
                return;

            var getTextMethod = getTextProperty.GetMethod;
            if (getTextMethod == null)
                return;

            var clearMethod = typeof(DefaultInterpolatedStringHandler).GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (clearMethod == null)
                return;

            if (getTextMethod.CreateDelegate(typeof(GetTextFunc), null) is not GetTextFunc getText || clearMethod.CreateDelegate(typeof(ClearAction), null) is not ClearAction clear)
                return;

            GetText = getText;
            Clear = clear;
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AppendFormatted(ref NativeString @string, ref DefaultInterpolatedStringHandler message, bool clear)
        {
            if (GetText != null)
            {
                var text = GetText(ref message);
                var buffer = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(text), text.Length);
                var result = @string.Append(buffer);
                if (clear)
                    Clear!(ref message);
                return result;
            }

            return AppendFormattedFallback(ref @string, ref message, clear);
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AppendFormattedFallback(ref NativeString @string, ref DefaultInterpolatedStringHandler message, bool clear)
        {
            ReadOnlySpan<char> text = clear ? message.ToStringAndClear() : message.ToString();
            var buffer = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(text), text.Length);
            var result = @string.Append(buffer);
            return result;
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(ref NativeStringBuilder<char> builder, ref DefaultInterpolatedStringHandler message, bool clear)
        {
            if (GetText != null)
            {
                var text = GetText(ref message);
                var buffer = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(text), text.Length);
                builder.Append(buffer);
                if (clear)
                    Clear!(ref message);
                return;
            }

            AppendFormattedFallback(ref builder, ref message, clear);
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendFormattedFallback(ref NativeStringBuilder<char> builder, ref DefaultInterpolatedStringHandler message, bool clear)
        {
            ReadOnlySpan<char> text = clear ? message.ToStringAndClear() : message.ToString();
            var buffer = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(text), text.Length);
            builder.Append(buffer);
        }

        /// <summary>
        ///     Get text
        /// </summary>
        private delegate ReadOnlySpan<char> GetTextFunc(ref DefaultInterpolatedStringHandler message);

        /// <summary>
        ///     Clear
        /// </summary>
        private delegate void ClearAction(ref DefaultInterpolatedStringHandler message);
    }
}
#endif