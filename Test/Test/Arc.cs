using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Examples
{
    public unsafe struct Arc<T> where T : unmanaged
    {
        private T _value;
        private int _state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArcRef<T> BorrowMutable()
        {
            if (Interlocked.CompareExchange(ref _state, -1, 0) != 0)
                throw new InvalidOperationException();
            return new ArcRef<T>(Unsafe.AsPointer(ref _value), Unsafe.AsPointer(ref _state));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArcReadOnlyRef<T> Borrow()
        {
            while (true)
            {
                var snapshot = Volatile.Read(ref _state);
                if (snapshot < 0)
                    throw new InvalidOperationException();
                if (Interlocked.CompareExchange(ref _state, snapshot + 1, snapshot) == snapshot)
                    break;
            }

            return new ArcReadOnlyRef<T>(Unsafe.AsPointer(ref _value), Unsafe.AsPointer(ref _state));
        }
    }

    public unsafe struct ArcRef<T> : IDisposable where T : unmanaged
    {
        private nint _ptr;
        private readonly int* _state;
        private int _disposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ArcRef(void* ptr, void* state)
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

    public unsafe struct ArcReadOnlyRef<T> : IDisposable where T : unmanaged
    {
        private nint _ptr;
        private readonly int* _state;
        private int _disposed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ArcReadOnlyRef(void* ptr, void* state)
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