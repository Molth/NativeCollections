using System;
using System.Runtime.CompilerServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Type helpers
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    internal static unsafe class TypeHelpers<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Is write atomic
        /// </summary>
        public static readonly bool IsWriteAtomic = IsWriteAtomicPrivate();

        /// <summary>
        ///     Is write atomic
        /// </summary>
        /// <returns>Is write atomic</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsWriteAtomicPrivate()
        {
            if (typeof(T) == typeof(nint) || typeof(T) == typeof(nuint))
                return true;
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    return true;
                case TypeCode.Double:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return Environment.Is64BitProcess;
                default:
                    return false;
            }
        }
    }
}