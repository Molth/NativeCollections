using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Examples
{
    public unsafe struct RwLock<T> where T : unmanaged
    {
        private T _value;
        private int _state;

        private RwLock(T value, int state)
        {
            _value = value;
            _state = state;
        }

        public static RwLock<T> Create(in T value) => new(value, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RwLockRef<T> BorrowMutable()
        {
            if (Interlocked.CompareExchange(ref _state, -1, 0) != 0)
                throw new InvalidOperationException();
            return new RwLockRef<T>(Unsafe.AsPointer(ref _value), Unsafe.AsPointer(ref _state));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RwLockReadOnlyRef<T> Borrow()
        {
            while (true)
            {
                var snapshot = Volatile.Read(ref _state);
                if (snapshot < 0)
                    throw new InvalidOperationException();
                if (Interlocked.CompareExchange(ref _state, snapshot + 1, snapshot) == snapshot)
                    break;
            }

            return new RwLockReadOnlyRef<T>(Unsafe.AsPointer(ref _value), Unsafe.AsPointer(ref _state));
        }
    }

    public unsafe struct RwLockRef<T> : IDisposable where T : unmanaged
    {
        private nint _ptr;
        private readonly int* _state;
        private int _disposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockRef(void* ptr, void* state)
        {
            _ptr = (nint)ptr;
            _state = (int*)state;
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
            Volatile.Write(ref Unsafe.AsRef<int>(_state), 0);
        }
    }

    public unsafe struct RwLockReadOnlyRef<T> : IDisposable where T : unmanaged
    {
        private nint _ptr;
        private readonly int* _state;
        private int _disposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockReadOnlyRef(void* ptr, void* state)
        {
            _ptr = (nint)ptr;
            _state = (int*)state;
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
            Interlocked.Decrement(ref Unsafe.AsRef<int>(_state));
        }
    }
}