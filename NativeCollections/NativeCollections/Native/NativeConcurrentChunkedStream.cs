using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Creates a stream whose backing store is memory.
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
            _handle = Box.New(ref value);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _handle->IsEmpty;

        /// <summary>
        ///     Gets the total number of chunks currently allocated in the stack.
        /// </summary>
        public int Chunks => _handle->Chunks;

        /// <summary>
        ///     Gets the number of chunks that are currently free and available for reuse.
        /// </summary>
        public int FreeChunks => _handle->FreeChunks;

        /// <summary>
        ///     Gets the maximum number of free chunks that can be retained before excess chunks are freed.
        /// </summary>
        public int MaxFreeChunks => _handle->MaxFreeChunks;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        public int Size => _handle->Size;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeConcurrentChunkedStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeConcurrentChunkedStream other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeConcurrentChunkedStream";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeConcurrentChunkedStream left, NativeConcurrentChunkedStream right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeConcurrentChunkedStream left, NativeConcurrentChunkedStream right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length) => _handle->Read(buffer, length);

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream,
        ///     and advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int length) => _handle->Write(buffer, length);

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer) => _handle->Read(buffer);

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream,
        ///     and advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer) => _handle->Write(buffer);

        /// <summary>
        ///     Advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(int length) => _handle->Read(length);

        /// <summary>
        ///     Advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int length) => _handle->Write(length);

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentChunkedStream Empty => default;
    }
}