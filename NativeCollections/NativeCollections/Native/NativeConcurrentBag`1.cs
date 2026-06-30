using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrentBag
    ///     (Slower than ConcurrentBag, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard | FromType.NotImplemented)]
    [BindingType(typeof(UnsafeConcurrentBag<>))]
    public readonly unsafe struct NativeConcurrentBag<T> : IIsCreated, IDisposable, IEquatable<NativeConcurrentBag<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeConcurrentBag<T>* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeConcurrentBag(UnsafeConcurrentBag<T>* handle) => _handle = handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->IsEmpty;
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>The number of elements contained in this.</value>
        /// <remarks>
        ///     The count returned represents a moment-in-time snapshot of the contents of the bag.
        ///     It does not reflect any updates to the collection after
        ///     <see cref="System.Collections.Concurrent.ConcurrentBag{T}.GetEnumerator" /> was called.
        /// </remarks>
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
        public bool Equals(NativeConcurrentBag<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentBag<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("NativeConcurrentBag<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentBag<T> left, NativeConcurrentBag<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentBag<T> left, NativeConcurrentBag<T> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (UnsafeHelpers.IsNull(handle))
                return;
            handle->Dispose();
            NativeMemoryAllocator.AlignedFree(handle);
        }

        /// <summary>
        ///     Removes all values from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Adds an object to this.
        /// </summary>
        /// <param name="item">
        ///     The object to be added to this.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item) => _handle->Add(item);

        /// <summary>
        ///     Attempts to remove and return an object from this.
        /// </summary>
        /// <param name="result">
        ///     When this method returns, <paramref name="result" /> contains the object
        ///     removed from this or the default value
        ///     of <typeparamref name="T" /> if the operation failed.
        /// </param>
        /// <returns>
        ///     true if an object was removed successfully;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryTake(out T result) => _handle->TryTake(out result);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentBag<T> Empty => new();

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentBag<T> Create()
        {
            var value = UnsafeConcurrentBag<T>.Create();
            var handle = NativeMemoryAllocator.AlignedAlloc<UnsafeConcurrentBag<T>>(1);
            Unsafe.AsRef<UnsafeConcurrentBag<T>>(handle) = value;
            return new NativeConcurrentBag<T>(handle);
        }
    }
}