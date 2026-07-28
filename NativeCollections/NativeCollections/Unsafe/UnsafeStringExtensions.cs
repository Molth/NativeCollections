#if NET6_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe string extensions
    /// </summary>
    public static class UnsafeStringExtensions
    {
        /// <summary>
        ///     Appends the string representation of a specified read-only character span to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AppendInterpolated(in this UnsafeString builder, [InterpolatedStringHandlerArgument("builder")] ref UnsafeStringInterpolatedStringHandler handler) => handler.TryCopyTo(ref builder.AsRef());

        /// <summary>
        ///     Appends the string representation of a specified read-only character span to this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AppendInterpolated(in this UnsafeString builder, IFormatProvider? provider, [InterpolatedStringHandlerArgument("builder", "provider")] ref UnsafeStringInterpolatedStringHandler handler) => handler.TryCopyTo(ref builder.AsRef());
    }
}
#endif