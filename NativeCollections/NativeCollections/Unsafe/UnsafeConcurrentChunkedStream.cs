using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe concurrent chunked stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    [BindingType(typeof(UnsafeChunkedStream))]
    public unsafe struct UnsafeConcurrentChunkedStream : IIsCreated, IDisposable, IEquatable<UnsafeConcurrentChunkedStream>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private UnsafeChunkedStream _handle;

        /// <summary>
        ///     Spin lock
        /// </summary>
        private UnsafeConcurrentSpinLock _spinLock;

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _handle.IsEmpty;

        /// <summary>
        ///     Chunks
        /// </summary>
        public readonly int Chunks => _handle.Chunks;

        /// <summary>
        ///     Free chunks
        /// </summary>
        public readonly int FreeChunks => _handle.FreeChunks;

        /// <summary>
        ///     Max free chunks
        /// </summary>
        public readonly int MaxFreeChunks => _handle.MaxFreeChunks;

        /// <summary>
        ///     Size
        /// </summary>
        public readonly int Size => _handle.Size;

        /// <summary>
        ///     Length
        /// </summary>
        public readonly int Length => _handle.Length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeConcurrentChunkedStream(int size, int maxFreeChunks)
        {
            _handle = new UnsafeChunkedStream(size, maxFreeChunks);
            _spinLock = new UnsafeConcurrentSpinLock();
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeConcurrentChunkedStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentChunkedStream other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "UnsafeConcurrentChunkedStream";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeConcurrentChunkedStream left, UnsafeConcurrentChunkedStream right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeConcurrentChunkedStream left, UnsafeConcurrentChunkedStream right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length)
        {
            _spinLock.Enter();
            try
            {
                return _handle.Read(buffer, length);
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int length)
        {
            _spinLock.Enter();
            try
            {
                _handle.Write(buffer, length);
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer)
        {
            _spinLock.Enter();
            try
            {
                return _handle.Read(buffer);
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer)
        {
            _spinLock.Enter();
            try
            {
                _handle.Write(buffer);
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(int length)
        {
            _spinLock.Enter();
            try
            {
                return _handle.Read(length);
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int length)
        {
            _spinLock.Enter();
            try
            {
                _handle.Write(length);
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Get first read buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetBuffer()
        {
            _spinLock.Enter();
            try
            {
                return _handle.GetBuffer();
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            _spinLock.Enter();
            try
            {
                return _handle.EnsureCapacity(capacity);
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            _spinLock.Enter();
            try
            {
                _handle.TrimExcess();
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            _spinLock.Enter();
            try
            {
                return _handle.TrimExcess(capacity);
            }
            finally
            {
                _spinLock.Exit();
            }
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentChunkedStream Empty => new();
    }
}