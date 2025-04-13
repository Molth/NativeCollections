using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native bit buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    [BindingType(typeof(UnsafeBitArray))]
    public readonly unsafe struct NativeBitArray : IDisposable, IEquatable<NativeBitArray>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeBitArray* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(int length)
        {
            var value = new UnsafeBitArray(length);
            var handle = (UnsafeBitArray*)NativeMemoryAllocator.Alloc((uint)sizeof(UnsafeBitArray));
            *handle = value;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(int length, bool defaultValue)
        {
            var value = new UnsafeBitArray(length, defaultValue);
            var handle = (UnsafeBitArray*)NativeMemoryAllocator.Alloc((uint)sizeof(UnsafeBitArray));
            *handle = value;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(int* buffer, int length)
        {
            var value = new UnsafeBitArray(buffer, length);
            var handle = (UnsafeBitArray*)NativeMemoryAllocator.Alloc((uint)sizeof(UnsafeBitArray));
            *handle = value;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(int* buffer, int length, bool defaultValue)
        {
            var value = new UnsafeBitArray(buffer, length, defaultValue);
            var handle = (UnsafeBitArray*)NativeMemoryAllocator.Alloc((uint)sizeof(UnsafeBitArray));
            *handle = value;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(NativeArray<int> buffer, int length)
        {
            var value = new UnsafeBitArray(buffer, length);
            var handle = (UnsafeBitArray*)NativeMemoryAllocator.Alloc((uint)sizeof(UnsafeBitArray));
            *handle = value;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray(NativeArray<int> buffer, int length, bool defaultValue)
        {
            var value = new UnsafeBitArray(buffer, length, defaultValue);
            var handle = (UnsafeBitArray*)NativeMemoryAllocator.Alloc((uint)sizeof(UnsafeBitArray));
            *handle = value;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Buffer
        /// </summary>
        public NativeArray<int> Buffer => _handle->Buffer;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->Length = value;
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.AsRef<UnsafeBitArray>(_handle)[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.AsRef<UnsafeBitArray>(_handle)[index] = value;
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public bool this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.AsRef<UnsafeBitArray>(_handle)[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.AsRef<UnsafeBitArray>(_handle)[index] = value;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeBitArray other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeBitArray nativeBitArray && nativeBitArray == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeBitArray";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeBitArray left, NativeBitArray right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeBitArray left, NativeBitArray right) => left._handle != right._handle;

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
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Set length
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length) => _handle->SetLength(length);

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index) => _handle->Get(index);

        /// <summary>
        ///     Set
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value) => _handle->Set(index, value);

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(uint index) => _handle->Get(index);

        /// <summary>
        ///     Set
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, bool value) => _handle->Set(index, value);

        /// <summary>
        ///     Set all
        /// </summary>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(bool value) => _handle->SetAll(value);

        /// <summary>
        ///     And
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray And(NativeBitArray value)
        {
            if (!value.IsCreated)
                throw new ArgumentNullException(nameof(value));
            _handle->And(value._handle);
            return this;
        }

        /// <summary>
        ///     Or
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Or(NativeBitArray value)
        {
            if (!value.IsCreated)
                throw new ArgumentNullException(nameof(value));
            _handle->Or(value._handle);
            return this;
        }

        /// <summary>
        ///     Xor
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Xor(NativeBitArray value)
        {
            if (!value.IsCreated)
                throw new ArgumentNullException(nameof(value));
            _handle->Xor(value._handle);
            return this;
        }

        /// <summary>
        ///     Not
        /// </summary>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Not()
        {
            _handle->Not();
            return this;
        }

        /// <summary>
        ///     Right shift
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray RightShift(int count)
        {
            _handle->RightShift(count);
            return this;
        }

        /// <summary>
        ///     Left shift
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>NativeBitArray</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray LeftShift(int count)
        {
            _handle->LeftShift(count);
            return this;
        }

        /// <summary>
        ///     Has all set
        /// </summary>
        /// <returns>All set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllSet() => _handle->HasAllSet();

        /// <summary>
        ///     Has any set
        /// </summary>
        /// <returns>Any set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnySet() => _handle->HasAnySet();

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        public NativeBitArraySlot GetSlot(int index) => _handle->GetSlot(index);

        /// <summary>
        ///     Try get
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="slot">Slot</param>
        /// <returns>Got</returns>
        public bool TryGetSlot(int index, out NativeBitArraySlot slot) => _handle->TryGetSlot(index, out slot);

        /// <summary>
        ///     Get
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        public NativeBitArraySlot GetSlot(uint index) => _handle->GetSlot(index);

        /// <summary>
        ///     Try get
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="slot">Slot</param>
        /// <returns>Got</returns>
        public bool TryGetSlot(uint index, out NativeBitArraySlot slot) => _handle->TryGetSlot(index, out slot);

        /// <summary>
        ///     Get int32 buffer length from bit length
        /// </summary>
        /// <param name="n">Bit length</param>
        /// <returns>Int32 buffer length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInt32ArrayLengthFromBitLength(int n) => UnsafeBitArray.GetInt32ArrayLengthFromBitLength(n);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeBitArray Empty => new();
    }
}