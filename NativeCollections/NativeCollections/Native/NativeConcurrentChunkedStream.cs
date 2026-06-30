using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrent chunked stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeConcurrentChunkedStream))]
    public readonly unsafe struct NativeConcurrentChunkedStream : IIsCreated, IDisposable, IEquatable<NativeConcurrentChunkedStream>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeConcurrentChunkedStream* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentChunkedStream(int size, int maxFreeChunks)
        {
            var value = new UnsafeConcurrentChunkedStream(size, maxFreeChunks);
            var handle = NativeMemoryAllocator.AlignedAlloc<UnsafeConcurrentChunkedStream>(1);
            Unsafe.AsRef<UnsafeConcurrentChunkedStream>(handle) = value;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->IsEmpty;

        /// <summary>
        ///     Chunks
        /// </summary>
        public int Chunks => _handle->Chunks;

        /// <summary>
        ///     Free chunks
        /// </summary>
        public int FreeChunks => _handle->FreeChunks;

        /// <summary>
        ///     Max free chunks
        /// </summary>
        public int MaxFreeChunks => _handle->MaxFreeChunks;

        /// <summary>
        ///     Size
        /// </summary>
        public int Size => _handle->Size;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentChunkedStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentChunkedStream other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeConcurrentChunkedStream";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentChunkedStream left, NativeConcurrentChunkedStream right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentChunkedStream left, NativeConcurrentChunkedStream right) => !left.Equals(right);

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
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length) => _handle->Read(buffer, length);

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int length) => _handle->Write(buffer, length);

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer) => _handle->Read(buffer);

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer) => _handle->Write(buffer);

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(int length) => _handle->Read(length);

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int length) => _handle->Write(length);

        /// <summary>
        ///     Get first read buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetBuffer() => _handle->GetBuffer();

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentChunkedStream Empty => new();
    }
}