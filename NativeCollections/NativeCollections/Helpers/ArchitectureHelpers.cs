﻿using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Architecture helpers
    /// </summary>
    internal static class ArchitectureHelpers
    {
        /// <summary>
        ///     Catch line size
        /// </summary>
        public const nuint CACHE_LINE_SIZE_NOT_ARM64 = 64;

        /// <summary>
        ///     Catch line size
        /// </summary>
        public const nuint CACHE_LINE_SIZE_ARM64 = 128;

        /// <summary>
        ///     Not Arm64
        /// </summary>
#if NET7_0_OR_GREATER
        public static bool NotArm64 => RuntimeInformation.ProcessArchitecture != Architecture.Arm64;
#else
        public static readonly bool NotArm64 = RuntimeInformation.ProcessArchitecture != Architecture.Arm64;
#endif
    }
}