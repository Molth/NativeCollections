using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8602
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    [BindingType(typeof(UnsafeConcurrentQueue<>))]
    public readonly unsafe struct NativeConcurrentQueue<T> : IDisposable, IEquatable<NativeConcurrentQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeConcurrentQueue<T>* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentQueue(int size, int maxFreeSlabs)
        {
            var value = new UnsafeConcurrentQueue<T>(size, maxFreeSlabs);
            var handle = NativeMemoryAllocator.AlignedAlloc<UnsafeConcurrentQueue<T>>(1);
            Unsafe.AsRef<UnsafeConcurrentQueue<T>>(handle) = value;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->IsEmpty;
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Count;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentQueue<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentQueue<T> nativeConcurrentQueue && nativeConcurrentQueue == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeConcurrentQueue<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentQueue<T> left, NativeConcurrentQueue<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentQueue<T> left, NativeConcurrentQueue<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            handle->Dispose();
            NativeMemoryAllocator.AlignedFree(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in T item) => _handle->Enqueue(item);

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result) => _handle->TryDequeue(out result);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentQueue<T> Empty => new();
    }
}