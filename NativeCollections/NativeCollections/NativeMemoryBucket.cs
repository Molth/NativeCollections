using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Native.Collections
{
    /// <summary>
    ///     Native memory bucket
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct NativeMemoryBucket : IDisposable, IEquatable<NativeMemoryBucket>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMemoryBucketHandle
        {
            /// <summary>
            ///     Size
            /// </summary>
            public int Size;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Array
            /// </summary>
            public byte** Array;

            /// <summary>
            ///     Index
            /// </summary>
            public int Index;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeMemoryBucketHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryBucket(int size, int length)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "MustBePositive");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            _handle = (NativeMemoryBucketHandle*)NativeMemoryAllocator.Alloc(sizeof(NativeMemoryBucketHandle));
            _handle->Size = size;
            _handle->Length = length;
            _handle->Array = (byte**)NativeMemoryAllocator.AllocZeroed(size);
            _handle->Index = 0;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->Index == 0;

        /// <summary>
        ///     Is full
        /// </summary>
        public bool IsFull => _handle->Index == _handle->Size;

        /// <summary>
        ///     Size
        /// </summary>
        public int Size => _handle->Size;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->Index;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeMemoryBucket other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeMemoryBucket nativeMemoryBucket && nativeMemoryBucket == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_handle;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeMemoryBucket";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeMemoryBucket left, NativeMemoryBucket right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeMemoryBucket left, NativeMemoryBucket right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_handle == null)
                return;
            for (var i = 0; i < _handle->Size; ++i)
            {
                var ptr = _handle->Array[i];
                if (ptr != null)
                    NativeMemoryAllocator.Free(ptr);
            }

            NativeMemoryAllocator.Free(_handle->Array);
            NativeMemoryAllocator.Free(_handle);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* Rent()
        {
            byte* buffer = null;
            if (_handle->Index < _handle->Size)
            {
                buffer = _handle->Array[_handle->Index];
                _handle->Array[_handle->Index++] = null;
            }

            if (buffer == null)
                buffer = (byte*)NativeMemoryAllocator.Alloc(_handle->Length);
            return buffer;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(byte* ptr)
        {
            if (_handle->Index != 0)
                _handle->Array[--_handle->Index] = ptr;
            else
                NativeMemoryAllocator.Free(ptr);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMemoryBucket Empty => new();
    }
}