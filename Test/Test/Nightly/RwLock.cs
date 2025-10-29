using System;
using System.Runtime.CompilerServices;
using System.Threading;
using NativeCollections;

namespace Examples
{
    public unsafe struct RwLock<T> where T : unmanaged
    {
        private T _value;
        private UnsafeConcurrentReaderWriterLock _rwLock;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RwLockRef<T> BorrowMutable()
        {
            _rwLock.Write();
            return new RwLockRef<T>(Unsafe.AsPointer(ref _value), Unsafe.AsPointer(ref _rwLock));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RwLockReadOnlyRef<T> Borrow()
        {
            _rwLock.Read();
            return new RwLockReadOnlyRef<T>(Unsafe.AsPointer(ref _value), Unsafe.AsPointer(ref _rwLock));
        }
    }

    public unsafe struct RwLockRef<T> : IDisposable where T : unmanaged
    {
        private nint _ptr;
        private readonly UnsafeConcurrentReaderWriterLock* _state;
        private int _disposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockRef(void* ptr, void* state)
        {
            _ptr = (nint)ptr;
            _state = (UnsafeConcurrentReaderWriterLock*)state;
            _disposed = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Unwrap()
        {
            var handle = Interlocked.Exchange(ref _ptr, 0);
            if (handle == 0)
                throw new InvalidOperationException();
            return ref Unsafe.AsRef<T>((void*)handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            Interlocked.Exchange(ref _ptr, 0);
            Unsafe.AsRef<UnsafeConcurrentReaderWriterLock>(_state).Exit();
        }
    }

    public unsafe struct RwLockReadOnlyRef<T> : IDisposable where T : unmanaged
    {
        private nint _ptr;
        private readonly UnsafeConcurrentReaderWriterLock* _state;
        private int _disposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockReadOnlyRef(void* ptr, void* state)
        {
            _ptr = (nint)ptr;
            _state = (UnsafeConcurrentReaderWriterLock*)state;
            _disposed = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Unwrap()
        {
            var handle = Interlocked.Exchange(ref _ptr, 0);
            if (handle == 0)
                throw new InvalidOperationException();
            return ref Unsafe.AsRef<T>((void*)handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            Interlocked.Exchange(ref _ptr, 0);
            Unsafe.AsRef<UnsafeConcurrentReaderWriterLock>(_state).Exit();
        }
    }
}