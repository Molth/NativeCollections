#if NET6_0_OR_GREATER
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8500
#pragma warning disable CS8632
#pragma warning disable CS9080

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
        public static bool AppendFormatted(ref NativeString @string, ref DefaultInterpolatedStringHandler handler, bool clear)
        {
            if (GetText != null)
            {
                var text = GetText(ref handler);
                var result = @string.Append(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(text), text.Length));
                if (clear)
                    Clear!(ref handler);
                return result;
            }

            return AppendFormattedFallback(ref @string, ref handler, clear);
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AppendFormattedFallback(ref NativeString @string, ref DefaultInterpolatedStringHandler handler, bool clear)
        {
            ReadOnlySpan<char> text = clear ? handler.ToStringAndClear() : handler.ToString();
            var result = @string.Append(text);
            return result;
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(ref NativeStringBuilder<char> builder, ref DefaultInterpolatedStringHandler handler, bool clear)
        {
            if (GetText != null)
            {
                var text = GetText(ref handler);
                builder.Append(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(text), text.Length));
                if (clear)
                    Clear!(ref handler);
                return;
            }

            AppendFormattedFallback(ref builder, ref handler, clear);
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendFormattedFallback(ref NativeStringBuilder<char> builder, ref DefaultInterpolatedStringHandler handler, bool clear)
        {
            ReadOnlySpan<char> text = clear ? handler.ToStringAndClear() : handler.ToString();
            builder.Append(text);
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(ref NativeStringBuilder<byte> builder, ref DefaultInterpolatedStringHandler handler, bool clear)
        {
            if (GetText != null)
            {
                var text = GetText(ref handler);
                builder.Append(text);
                if (clear)
                    Clear!(ref handler);
                return;
            }

            AppendFormattedFallback(ref builder, ref handler, clear);
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendFormattedFallback(ref NativeStringBuilder<byte> builder, ref DefaultInterpolatedStringHandler handler, bool clear)
        {
            ReadOnlySpan<char> text = clear ? handler.ToStringAndClear() : handler.ToString();
            builder.Append(text);
        }

        /// <summary>
        ///     Get text
        /// </summary>
        private delegate ReadOnlySpan<char> GetTextFunc(ref DefaultInterpolatedStringHandler handler);

        /// <summary>
        ///     Clear
        /// </summary>
        private delegate void ClearAction(ref DefaultInterpolatedStringHandler handler);
    }
}
#endif