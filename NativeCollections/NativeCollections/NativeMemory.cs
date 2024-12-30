﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8603

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly struct NativeMemory<T> : IDisposable, IEquatable<NativeMemory<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle.IsAllocated;

        /// <summary>
        ///     Manager
        /// </summary>
        public NativeMemoryManager<T> Manager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (NativeMemoryManager<T>)_handle.Target;
        }

        /// <summary>
        ///     Memory
        /// </summary>
        public Memory<T> Memory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Manager.Memory;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="memoryManager">Native memory manager</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemory(NativeMemoryManager<T> memoryManager) => _handle = GCHandle.Alloc(memoryManager);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeMemory<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeMemory<T> nativeMemory && nativeMemory == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => _handle.GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeMemory<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeMemory<T> left, NativeMemory<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeMemory<T> left, NativeMemory<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;
            handle.Free();
        }
    }
}