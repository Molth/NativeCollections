using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native IntPtr
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeIntPtr : IDisposable, IEquatable<NativeIntPtr>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly void* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="handle">Handle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeIntPtr(void* handle) => _handle = handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="handle">Handle</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeIntPtr(nint handle) => _handle = (void*)handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Handle
        /// </summary>
        public void* Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle;
        }

        /// <summary>
        ///     Value
        /// </summary>
        public nint Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (nint)_handle;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeIntPtr other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeIntPtr nativeIntPtr && nativeIntPtr == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeIntPtr";

        /// <summary>
        ///     As reference
        /// </summary>
        /// <returns>NativeIntPtr</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeIntPtr(void* handle) => new(handle);

        /// <summary>
        ///     As handle
        /// </summary>
        /// <returns>Handle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator void*(NativeIntPtr nativeIntPtr) => nativeIntPtr._handle;

        /// <summary>
        ///     As reference
        /// </summary>
        /// <returns>NativeIntPtr</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeIntPtr(nint handle) => new(handle);

        /// <summary>
        ///     As handle
        /// </summary>
        /// <returns>Handle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator nint(NativeIntPtr nativeIntPtr) => (nint)nativeIntPtr._handle;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeIntPtr left, NativeIntPtr right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeIntPtr left, NativeIntPtr right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="reference">Reference</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>NativeIntPtr</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeIntPtr Create<T>(ref T reference) where T : unmanaged => new(Unsafe.AsPointer(ref reference));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeIntPtr Empty => new();
    }
}