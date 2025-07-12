using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8500
#pragma warning disable CS8632
#pragma warning disable CS9080

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native string builder extensions
    /// </summary>
    public static partial class NativeStringBuilderExtensions
    {
        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 1)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 2)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 3)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 4)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 5)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 6)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 7)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 8)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 9)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 10)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 11)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 12)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 13)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 14)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 15)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg14, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this NativeStringBuilder<char> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 16)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg14, width, leftJustify);
                        break;
                    case 15:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg15, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 1)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 2)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 3)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 4)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 5)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 6)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 7)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 8)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 9)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 10)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 11)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 12)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 13)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 14)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 15)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg14, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this NativeStringBuilder<char> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 16)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg14, width, leftJustify);
                        break;
                    case 15:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg15, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 1)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 2)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 3)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 4)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 5)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 6)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 7)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 8)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 9)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 10)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 11)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 12)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 13)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 14)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 15)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg14, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this NativeStringBuilder<byte> builder,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 16)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg14, width, leftJustify);
                        break;
                    case 15:
                        Append(ref builderRef, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg15, width, leftJustify);
                        break;
                }
            }
        } 

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 1)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 2)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 3)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 4)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 5)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 6)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 7)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 8)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 9)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 10)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 11)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 12)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 13)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 14)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 15)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg14, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this NativeStringBuilder<byte> builder, IFormatProvider? provider,
#if NET7_0_OR_GREATER
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
#endif
            string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ThrowHelpers.ThrowIfNull(format, nameof(format));
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan, out var index))
                    return;

                if ((uint)index >= 16)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg14, width, leftJustify);
                        break;
                    case 15:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan), itemFormatSpan.Length), arg15, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Parse format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ParseFormat(ref NativeStringBuilder<char> builderRef, string format, ref int position, out int width, out bool leftJustify, out ReadOnlySpan<char> itemFormatSpan, out int index)
        {
            index = default;
            leftJustify = default;
            itemFormatSpan = default;
            width = default;
            char ch;
            while (true)
            {
                if ((uint)position >= (uint)format.Length)
                    return false;
                var remainder = format.AsSpan(position);
                var countUntilNextBrace = remainder.IndexOfAny('{', '}');
                if (countUntilNextBrace < 0)
                {
                    builderRef.Append(remainder);
                    return false;
                }

                builderRef.Append(remainder.Slice(0, countUntilNextBrace));
                position += countUntilNextBrace;
                var brace = format[position];
                ch = MoveNext(format, ref position);
                if (brace == ch)
                {
                    builderRef.Append(ch);
                    position++;
                    continue;
                }

                if (brace != '{')
                    ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.UnexpectedClosingBrace);
                break;
            }

            width = 0;
            leftJustify = false;
            itemFormatSpan = default;
            index = ch - '0';
            if ((uint)index >= 10)
                ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.ExpectedAsciiDigit);
            ch = MoveNext(format, ref position);
            if (ch != '}')
            {
                while (CharHelpers.IsAsciiDigit(ch) && index < 1_000_000)
                {
                    index = index * 10 + ch - '0';
                    ch = MoveNext(format, ref position);
                }

                while (ch == ' ')
                    ch = MoveNext(format, ref position);
                if (ch == ',')
                {
                    do
                    {
                        ch = MoveNext(format, ref position);
                    } while (ch == ' ');

                    if (ch == '-')
                    {
                        leftJustify = true;
                        ch = MoveNext(format, ref position);
                    }

                    width = ch - '0';
                    if ((uint)width >= 10)
                        ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.ExpectedAsciiDigit);
                    ch = MoveNext(format, ref position);
                    while (CharHelpers.IsAsciiDigit(ch) && width < 1_000_000)
                    {
                        width = width * 10 + ch - '0';
                        ch = MoveNext(format, ref position);
                    }

                    while (ch == ' ')
                        ch = MoveNext(format, ref position);
                }

                if (ch != '}')
                {
                    if (ch != ':')
                        ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.UnclosedFormatItem);
                    var startingPosition = position;
                    while (true)
                    {
                        ch = MoveNext(format, ref position);
                        if (ch == '}')
                            break;
                        if (ch == '{')
                            ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.UnclosedFormatItem);
                    }

                    startingPosition++;
                    itemFormatSpan = format.AsSpan(startingPosition, position - startingPosition);
                }
            }

            position++;
            return true;
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append<T>(ref NativeStringBuilder<char> builderRef, ReadOnlySpan<char> itemFormatSpan, T? arg, int width, bool leftJustify)
        {
            int charsWritten;
            if (leftJustify)
            {
                while (!FormatHelpers.TryFormat(arg, builderRef.Space, out charsWritten, itemFormatSpan))
                    builderRef.EnsureCapacity(builderRef.Capacity + 1);
                builderRef.Advance(charsWritten);
                var padding = width - charsWritten;
                if (width > 0 && padding > 0)
                    builderRef.Append(' ', padding);
            }
            else
            {
                if (arg != null)
                {
                    ReadOnlySpan<char> obj;
                    if (typeof(T) == typeof(string))
                        obj = Unsafe.As<T?, string>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ArraySegment<char>))
                        obj = Unsafe.As<T, ArraySegment<char>>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ReadOnlyMemory<char>))
                        obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref arg).Span;
                    else if (typeof(T) == typeof(Memory<char>))
                        obj = Unsafe.As<T, Memory<char>>(ref arg).Span;
                    else
                        goto label;
                    charsWritten = obj.Length;
                    var padding = width - charsWritten;
                    if (padding > 0)
                        builderRef.Append(' ', padding);
                    builderRef.Append(obj);
                    return;
                }

                label:
                using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
                while (!FormatHelpers.TryFormat(arg, temp.Space, out charsWritten, itemFormatSpan))
                    temp.EnsureCapacity(temp.Capacity + 1);
                temp.Advance(charsWritten);
                var padding2 = width - charsWritten;
                if (padding2 > 0)
                    builderRef.Append(' ', padding2);
                var buffer = temp.AsReadOnlySpan();
                builderRef.Append(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer), buffer.Length));
            }
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append<T>(ref NativeStringBuilder<char> builderRef, IFormatProvider? provider, ICustomFormatter? customFormatter, ref string? itemFormat, ReadOnlySpan<char> itemFormatSpan, T? arg, int width, bool leftJustify)
        {
            if (customFormatter != null)
            {
                if (itemFormat == null || !itemFormat.AsSpan().SequenceEqual(itemFormatSpan))
                    itemFormat = new string(itemFormatSpan);
                var result = customFormatter.Format(itemFormat, arg, provider);
                var obj = (result != null ? result : "").AsSpan();
                if (width <= obj.Length)
                {
                    builderRef.Append(obj);
                }
                else if (leftJustify)
                {
                    builderRef.Append(obj);
                    builderRef.Append(' ', width - obj.Length);
                }
                else
                {
                    builderRef.Append(' ', width - obj.Length);
                    builderRef.Append(obj);
                }

                return;
            }

            int charsWritten;
            if (leftJustify)
            {
                while (!FormatHelpers.TryFormat(arg, builderRef.Space, out charsWritten, itemFormatSpan, provider))
                    builderRef.EnsureCapacity(builderRef.Capacity + 1);
                builderRef.Advance(charsWritten);
                var padding = width - charsWritten;
                if (width > 0 && padding > 0)
                    builderRef.Append(' ', padding);
            }
            else
            {
                if (arg != null)
                {
                    ReadOnlySpan<char> obj;
                    if (typeof(T) == typeof(string))
                        obj = Unsafe.As<T?, string>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ArraySegment<char>))
                        obj = Unsafe.As<T, ArraySegment<char>>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ReadOnlyMemory<char>))
                        obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref arg).Span;
                    else if (typeof(T) == typeof(Memory<char>))
                        obj = Unsafe.As<T, Memory<char>>(ref arg).Span;
                    else
                        goto label;
                    charsWritten = obj.Length;
                    var padding = width - charsWritten;
                    if (padding > 0)
                        builderRef.Append(' ', padding);
                    builderRef.Append(obj);
                    return;
                }

                label:
                using var temp = new NativeStringBuilder<char>(stackalloc char[512], 0);
                while (!FormatHelpers.TryFormat(arg, temp.Space, out charsWritten, itemFormatSpan, provider))
                    temp.EnsureCapacity(temp.Capacity + 1);
                temp.Advance(charsWritten);
                var padding2 = width - charsWritten;
                if (padding2 > 0)
                    builderRef.Append(' ', padding2);
                var buffer = temp.AsReadOnlySpan();
                builderRef.Append(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer), buffer.Length));
            }
        }

        /// <summary>
        ///     Parse format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ParseFormat(ref NativeStringBuilder<byte> builderRef, string format, ref int position, out int width, out bool leftJustify, out ReadOnlySpan<char> itemFormatSpan, out int index)
        {
            index = default;
            leftJustify = default;
            itemFormatSpan = default;
            width = default;
            char ch;
            while (true)
            {
                if ((uint)position >= (uint)format.Length)
                    return false;
                var remainder = format.AsSpan(position);
                var countUntilNextBrace = remainder.IndexOfAny('{', '}');
                if (countUntilNextBrace < 0)
                {
                    builderRef.Append(remainder);
                    return false;
                }

                builderRef.Append(remainder.Slice(0, countUntilNextBrace));
                position += countUntilNextBrace;
                var brace = format[position];
                ch = MoveNext(format, ref position);
                if (brace == ch)
                {
                    builderRef.Append(ch);
                    position++;
                    continue;
                }

                if (brace != '{')
                    ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.UnexpectedClosingBrace);
                break;
            }

            width = 0;
            leftJustify = false;
            itemFormatSpan = default;
            index = ch - '0';
            if ((uint)index >= 10)
                ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.ExpectedAsciiDigit);
            ch = MoveNext(format, ref position);
            if (ch != '}')
            {
                while (CharHelpers.IsAsciiDigit(ch) && index < 1_000_000)
                {
                    index = index * 10 + ch - '0';
                    ch = MoveNext(format, ref position);
                }

                while (ch == ' ')
                    ch = MoveNext(format, ref position);
                if (ch == ',')
                {
                    do
                    {
                        ch = MoveNext(format, ref position);
                    } while (ch == ' ');

                    if (ch == '-')
                    {
                        leftJustify = true;
                        ch = MoveNext(format, ref position);
                    }

                    width = ch - '0';
                    if ((uint)width >= 10)
                        ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.ExpectedAsciiDigit);
                    ch = MoveNext(format, ref position);
                    while (CharHelpers.IsAsciiDigit(ch) && width < 1_000_000)
                    {
                        width = width * 10 + ch - '0';
                        ch = MoveNext(format, ref position);
                    }

                    while (ch == ' ')
                        ch = MoveNext(format, ref position);
                }

                if (ch != '}')
                {
                    if (ch != ':')
                        ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.UnclosedFormatItem);
                    var startingPosition = position;
                    while (true)
                    {
                        ch = MoveNext(format, ref position);
                        if (ch == '}')
                            break;
                        if (ch == '{')
                            ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.UnclosedFormatItem);
                    }

                    startingPosition++;
                    itemFormatSpan = format.AsSpan(startingPosition, position - startingPosition);
                }
            }

            position++;
            return true;
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append<T>(ref NativeStringBuilder<byte> builderRef, ReadOnlySpan<char> itemFormatSpan, T? arg, int width, bool leftJustify)
        {
            int bytesWritten;
            int charsWritten;
            if (leftJustify)
            {
                while (!Utf8FormatHelpers.TryFormat(arg, builderRef.Space, out bytesWritten, itemFormatSpan))
                    builderRef.EnsureCapacity(builderRef.Capacity + 1);
                charsWritten = Encoding.UTF8.GetCharCount(builderRef.GetSpan(bytesWritten));
                builderRef.Advance(bytesWritten);
                var padding1 = width - charsWritten;
                if (width > 0 && padding1 > 0)
                    builderRef.Append(' ', padding1);
            }
            else
            {
                if (arg != null)
                {
                    ReadOnlySpan<char> obj;
                    if (typeof(T) == typeof(string))
                        obj = Unsafe.As<T?, string>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ArraySegment<char>))
                        obj = Unsafe.As<T, ArraySegment<char>>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ReadOnlyMemory<char>))
                        obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref arg).Span;
                    else if (typeof(T) == typeof(Memory<char>))
                        obj = Unsafe.As<T, Memory<char>>(ref arg).Span;
                    else
                        goto label;
                    charsWritten = obj.Length;
                    var padding = width - charsWritten;
                    if (padding > 0)
                        builderRef.Append(' ', padding);
                    builderRef.Append(obj);
                    return;
                }

                label:
                using var temp = new NativeStringBuilder<byte>(stackalloc byte[1024], 0);
                while (!Utf8FormatHelpers.TryFormat(arg, temp.Space, out bytesWritten, itemFormatSpan))
                    temp.EnsureCapacity(temp.Capacity + 1);
                charsWritten = Encoding.UTF8.GetCharCount(temp.GetSpan(bytesWritten));
                temp.Advance(bytesWritten);
                var padding2 = width - charsWritten;
                if (padding2 > 0)
                    builderRef.Append(' ', padding2);
                var buffer = temp.AsReadOnlySpan();
                builderRef.Append(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer), buffer.Length));
            }
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append<T>(ref NativeStringBuilder<byte> builderRef, IFormatProvider? provider, ICustomFormatter? customFormatter, ref string? itemFormat, ReadOnlySpan<char> itemFormatSpan, T? arg, int width, bool leftJustify)
        {
            if (customFormatter != null)
            {
                if (itemFormat == null || !itemFormat.AsSpan().SequenceEqual(itemFormatSpan))
                    itemFormat = new string(itemFormatSpan);
                var result = customFormatter.Format(itemFormat, arg, provider);
                var obj = (result != null ? result : "").AsSpan();
                if (width <= obj.Length)
                {
                    builderRef.Append(obj);
                }
                else if (leftJustify)
                {
                    builderRef.Append(obj);
                    builderRef.Append(' ', width - obj.Length);
                }
                else
                {
                    builderRef.Append(' ', width - obj.Length);
                    builderRef.Append(obj);
                }

                return;
            }

            int bytesWritten;
            int charsWritten;
            if (leftJustify)
            {
                while (!Utf8FormatHelpers.TryFormat(arg, builderRef.Space, out bytesWritten, itemFormatSpan, provider))
                    builderRef.EnsureCapacity(builderRef.Capacity + 1);
                charsWritten = Encoding.UTF8.GetCharCount(builderRef.GetSpan(bytesWritten));
                builderRef.Advance(bytesWritten);
                var padding1 = width - charsWritten;
                if (width > 0 && padding1 > 0)
                    builderRef.Append(' ', padding1);
            }
            else
            {
                if (arg != null)
                {
                    ReadOnlySpan<char> obj;
                    if (typeof(T) == typeof(string))
                        obj = Unsafe.As<T?, string>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ArraySegment<char>))
                        obj = Unsafe.As<T, ArraySegment<char>>(ref arg).AsSpan();
                    else if (typeof(T) == typeof(ReadOnlyMemory<char>))
                        obj = Unsafe.As<T, ReadOnlyMemory<char>>(ref arg).Span;
                    else if (typeof(T) == typeof(Memory<char>))
                        obj = Unsafe.As<T, Memory<char>>(ref arg).Span;
                    else
                        goto label;
                    charsWritten = obj.Length;
                    var padding = width - charsWritten;
                    if (padding > 0)
                        builderRef.Append(' ', padding);
                    builderRef.Append(obj);
                    return;
                }

                label:
                using var temp = new NativeStringBuilder<byte>(stackalloc byte[1024], 0);
                while (!Utf8FormatHelpers.TryFormat(arg, temp.Space, out bytesWritten, itemFormatSpan, provider))
                    temp.EnsureCapacity(temp.Capacity + 1);
                charsWritten = Encoding.UTF8.GetCharCount(temp.GetSpan(bytesWritten));
                temp.Advance(bytesWritten);
                var padding2 = width - charsWritten;
                if (padding2 > 0)
                    builderRef.Append(' ', padding2);
                var buffer = temp.AsReadOnlySpan();
                builderRef.Append(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer), buffer.Length));
            }
        }

        /// <summary>
        ///     Move next
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char MoveNext(string format, ref int position)
        {
            position++;
            if ((uint)position >= (uint)format.Length)
                ThrowHelpers.ThrowFormatInvalidString(position, InvalidFormatReason.UnclosedFormatItem);
            return format[position];
        }
    }
}