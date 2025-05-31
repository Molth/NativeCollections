#if NET6_0_OR_GREATER
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

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
    internal static unsafe class DefaultInterpolatedStringHandlerHelpers
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
                var result = @string.Append(text);
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
            var result = @string.Append(text);
            return result;
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(in NativeStringBuilder<char> builder, ref DefaultInterpolatedStringHandler message, bool clear)
        {
            if (GetText != null)
            {
                var text = GetText(ref message);
                fixed (NativeStringBuilder<char>* ptr = &builder)
                {
                    ptr->Append(text);
                }

                if (clear)
                    Clear!(ref message);
                return;
            }

            AppendFormattedFallback(builder, ref message, clear);
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendFormattedFallback(in NativeStringBuilder<char> builder, ref DefaultInterpolatedStringHandler message, bool clear)
        {
            ReadOnlySpan<char> text = clear ? message.ToStringAndClear() : message.ToString();
            fixed (NativeStringBuilder<char>* ptr = &builder)
            {
                ptr->Append(text);
            }
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormatted(in NativeStringBuilder<byte> builder, ref DefaultInterpolatedStringHandler message, bool clear)
        {
            if (GetText != null)
            {
                var text = GetText(ref message);
                fixed (NativeStringBuilder<byte>* ptr = &builder)
                {
                    ptr->Append(text);
                }

                if (clear)
                    Clear!(ref message);
                return;
            }

            AppendFormattedFallback(builder, ref message, clear);
        }

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendFormattedFallback(in NativeStringBuilder<byte> builder, ref DefaultInterpolatedStringHandler message, bool clear)
        {
            ReadOnlySpan<char> text = clear ? message.ToStringAndClear() : message.ToString();
            fixed (NativeStringBuilder<byte>* ptr = &builder)
            {
                ptr->Append(text);
            }
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