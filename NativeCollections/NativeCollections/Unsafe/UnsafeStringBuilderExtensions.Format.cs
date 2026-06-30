using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe string builder extensions
    /// </summary>
    public static partial class UnsafeStringBuilderExtensions
    {
        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 1)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 2)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 3)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 4)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 5)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 6)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 7)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 8)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 9)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 10)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 11)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 12)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 13)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 14)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 15)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, itemFormatSpan2, arg14, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<char> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 16)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, itemFormatSpan2, arg14, width, leftJustify);
                        break;
                    case 15:
                        Append(ref builderRef, itemFormatSpan2, arg15, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 1)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 2)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 3)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 4)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 5)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 6)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 7)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 8)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 9)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 10)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 11)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 12)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 13)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 14)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 15)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg14, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<char> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 16)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg14, width, leftJustify);
                        break;
                    case 15:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg15, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 1)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 2)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 3)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 4)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 5)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 6)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 7)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 8)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 9)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 10)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 11)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 12)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 13)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 14)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 15)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, itemFormatSpan2, arg14, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<byte> builder, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 16)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, itemFormatSpan2, arg14, width, leftJustify);
                        break;
                    case 15:
                        Append(ref builderRef, itemFormatSpan2, arg15, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 1)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 2)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 3)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 4)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 5)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 6)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 7)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 8)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 9)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 10)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 11)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 12)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 13)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 14)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 15)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg14, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Append format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(in this UnsafeStringBuilder<byte> builder, IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ref var builderRef = ref builder.AsRef();
            var customFormatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            string? itemFormat = null;
            var position = 0;
            while (true)
            {
                if (!ParseFormat(ref builderRef, format, ref position, out var width, out var leftJustify, out var itemFormatSpan1, out var index))
                    return;

                if ((uint)index >= 16)
                    ThrowHelpers.ThrowFormatIndexOutOfRange();

                var itemFormatSpan2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(itemFormatSpan1), itemFormatSpan1.Length);
                switch (index)
                {
                    case 0:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg0, width, leftJustify);
                        break;
                    case 1:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg1, width, leftJustify);
                        break;
                    case 2:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg2, width, leftJustify);
                        break;
                    case 3:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg3, width, leftJustify);
                        break;
                    case 4:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg4, width, leftJustify);
                        break;
                    case 5:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg5, width, leftJustify);
                        break;
                    case 6:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg6, width, leftJustify);
                        break;
                    case 7:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg7, width, leftJustify);
                        break;
                    case 8:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg8, width, leftJustify);
                        break;
                    case 9:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg9, width, leftJustify);
                        break;
                    case 10:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg10, width, leftJustify);
                        break;
                    case 11:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg11, width, leftJustify);
                        break;
                    case 12:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg12, width, leftJustify);
                        break;
                    case 13:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg13, width, leftJustify);
                        break;
                    case 14:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg14, width, leftJustify);
                        break;
                    case 15:
                        Append(ref builderRef, provider, customFormatter, ref itemFormat, itemFormatSpan2, arg15, width, leftJustify);
                        break;
                }
            }
        }

        /// <summary>
        ///     Parse format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ParseFormat(ref UnsafeStringBuilder<char> builderRef, ReadOnlySpan<char> format, ref int position, out int width, out bool leftJustify, out ReadOnlySpan<char> itemFormatSpan1, out int index)
        {
            index = default;
            leftJustify = default;
            itemFormatSpan1 = default;
            width = default;
            char ch;
            while (true)
            {
                if ((uint)position >= (uint)format.Length)
                    return false;
                var remainder = format.Slice(position);
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
                    ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnexpectedClosingBrace);
                break;
            }

            width = 0;
            leftJustify = false;
            itemFormatSpan1 = default;
            index = ch - '0';
            if ((uint)index >= 10)
                ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_ExpectedAsciiDigit);
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
                        ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_ExpectedAsciiDigit);
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
                        ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);
                    var startingPosition = position;
                    while (true)
                    {
                        ch = MoveNext(format, ref position);
                        if (ch == '}')
                            break;
                        if (ch == '{')
                            ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);
                    }

                    startingPosition++;
                    itemFormatSpan1 = format.Slice(startingPosition, position - startingPosition);
                }
            }

            position++;
            return true;
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append<T>(ref UnsafeStringBuilder<char> builderRef, ReadOnlySpan<char> itemFormatSpan1, T? arg, int width, bool leftJustify)
        {
            int charsWritten;
            if (leftJustify)
            {
                while (!FormatHelpers.TryFormat(arg, builderRef.Space, out charsWritten, itemFormatSpan1))
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
                using var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0);
                while (!FormatHelpers.TryFormat(arg, temp.Space, out charsWritten, itemFormatSpan1))
                    temp.EnsureCapacity(temp.Capacity + 1);
                temp.Advance(charsWritten);
                var padding2 = width - charsWritten;
                if (padding2 > 0)
                    builderRef.Append(' ', padding2);
                var buffer1 = temp.AsReadOnlySpan();
                var buffer2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer1), buffer1.Length);
                builderRef.Append(buffer2);
            }
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append<T>(ref UnsafeStringBuilder<char> builderRef, IFormatProvider? provider, ICustomFormatter? customFormatter, ref string? itemFormat, ReadOnlySpan<char> itemFormatSpan1, T? arg, int width, bool leftJustify)
        {
            if (customFormatter != null)
            {
                if (itemFormat == null || !itemFormat.AsSpan().SequenceEqual(itemFormatSpan1))
                    itemFormat = new string(itemFormatSpan1);
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
                while (!FormatHelpers.TryFormat(arg, builderRef.Space, out charsWritten, itemFormatSpan1, provider))
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
                using var temp = new UnsafeStringBuilder<char>(stackalloc char[512], 0);
                while (!FormatHelpers.TryFormat(arg, temp.Space, out charsWritten, itemFormatSpan1, provider))
                    temp.EnsureCapacity(temp.Capacity + 1);
                temp.Advance(charsWritten);
                var padding2 = width - charsWritten;
                if (padding2 > 0)
                    builderRef.Append(' ', padding2);
                var buffer1 = temp.AsReadOnlySpan();
                var buffer2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer1), buffer1.Length);
                builderRef.Append(buffer2);
            }
        }

        /// <summary>
        ///     Parse format
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ParseFormat(ref UnsafeStringBuilder<byte> builderRef, ReadOnlySpan<char> format, ref int position, out int width, out bool leftJustify, out ReadOnlySpan<char> itemFormatSpan1, out int index)
        {
            index = default;
            leftJustify = default;
            itemFormatSpan1 = default;
            width = default;
            char ch;
            while (true)
            {
                if ((uint)position >= (uint)format.Length)
                    return false;
                var remainder = format.Slice(position);
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
                    ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnexpectedClosingBrace);
                break;
            }

            width = 0;
            leftJustify = false;
            itemFormatSpan1 = default;
            index = ch - '0';
            if ((uint)index >= 10)
                ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_ExpectedAsciiDigit);
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
                        ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_ExpectedAsciiDigit);
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
                        ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);
                    var startingPosition = position;
                    while (true)
                    {
                        ch = MoveNext(format, ref position);
                        if (ch == '}')
                            break;
                        if (ch == '{')
                            ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);
                    }

                    startingPosition++;
                    itemFormatSpan1 = format.Slice(startingPosition, position - startingPosition);
                }
            }

            position++;
            return true;
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append<T>(ref UnsafeStringBuilder<byte> builderRef, ReadOnlySpan<char> itemFormatSpan1, T? arg, int width, bool leftJustify)
        {
            int bytesWritten;
            int charsWritten;
            if (leftJustify)
            {
                while (!Utf8FormatHelpers.TryFormat(arg, builderRef.Space, out bytesWritten, itemFormatSpan1))
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
                using var temp = new UnsafeStringBuilder<byte>(stackalloc byte[1024], 0);
                while (!Utf8FormatHelpers.TryFormat(arg, temp.Space, out bytesWritten, itemFormatSpan1))
                    temp.EnsureCapacity(temp.Capacity + 1);
                charsWritten = Encoding.UTF8.GetCharCount(temp.GetSpan(bytesWritten));
                temp.Advance(bytesWritten);
                var padding2 = width - charsWritten;
                if (padding2 > 0)
                    builderRef.Append(' ', padding2);
                var buffer1 = temp.AsReadOnlySpan();
                var buffer2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer1), buffer1.Length);
                builderRef.Append(buffer2);
            }
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append<T>(ref UnsafeStringBuilder<byte> builderRef, IFormatProvider? provider, ICustomFormatter? customFormatter, ref string? itemFormat, ReadOnlySpan<char> itemFormatSpan1, T? arg, int width, bool leftJustify)
        {
            if (customFormatter != null)
            {
                if (itemFormat == null || !itemFormat.AsSpan().SequenceEqual(itemFormatSpan1))
                    itemFormat = new string(itemFormatSpan1);
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
                while (!Utf8FormatHelpers.TryFormat(arg, builderRef.Space, out bytesWritten, itemFormatSpan1, provider))
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
                using var temp = new UnsafeStringBuilder<byte>(stackalloc byte[1024], 0);
                while (!Utf8FormatHelpers.TryFormat(arg, temp.Space, out bytesWritten, itemFormatSpan1, provider))
                    temp.EnsureCapacity(temp.Capacity + 1);
                charsWritten = Encoding.UTF8.GetCharCount(temp.GetSpan(bytesWritten));
                temp.Advance(bytesWritten);
                var padding2 = width - charsWritten;
                if (padding2 > 0)
                    builderRef.Append(' ', padding2);
                var buffer1 = temp.AsReadOnlySpan();
                var buffer2 = MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(buffer1), buffer1.Length);
                builderRef.Append(buffer2);
            }
        }

        /// <summary>
        ///     Move next
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char MoveNext(ReadOnlySpan<char> format, ref int position)
        {
            position++;
            if ((uint)position >= (uint)format.Length)
                ThrowHelpers.ThrowFormatInvalidString(position, ExceptionResource.Format_UnclosedFormatItem);
            return format[position];
        }
    }
}