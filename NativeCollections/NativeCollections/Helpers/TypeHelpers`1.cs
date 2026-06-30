using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Type helpers
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    internal static class TypeHelpers<T> where T : unmanaged
    {
        /// <summary>
        ///     Is supported for target-64
        /// </summary>
        public static readonly bool IsSupported64 = IsSupported64Private();

        /// <summary>
        ///     Is supported for target-32
        /// </summary>
        public static readonly bool IsSupported32 = IsSupported32Private();

        /// <summary>
        ///     Is supported for target-64
        /// </summary>
        private static bool IsSupported64Private()
        {
            if (typeof(T) == typeof(IntPtr) || typeof(T) == typeof(UIntPtr))
                return true;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.Single:
                case TypeCode.UInt32:
                case TypeCode.Double:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     Is supported for target-32
        /// </summary>
        private static bool IsSupported32Private()
        {
            if (typeof(T) == typeof(IntPtr) || typeof(T) == typeof(UIntPtr))
                return !Environment.Is64BitProcess;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.Single:
                case TypeCode.UInt32:
                    return true;
                default:
                    return false;
            }
        }
    }
}