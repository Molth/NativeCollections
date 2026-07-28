using System;
using System.Runtime.CompilerServices;

namespace Examples
{
    public unsafe struct RwLock<T> where T : unmanaged
    {
        private T _value;
        private RwLock _state;

        private RwLock(T value)
        {
            _value = value;
            _state = new RwLock();
        }

        public static RwLock<T> Create(in T value) => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RwLockRef<T> BorrowMutable()
        {
            _state.Write();
            return new RwLockRef<T>((T*)Unsafe.AsPointer(ref _value), (RwLock*)Unsafe.AsPointer(ref _state));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RwLockReadOnlyRef<T> Borrow()
        {
            _state.Read();
            return new RwLockReadOnlyRef<T>((T*)Unsafe.AsPointer(ref _value), (RwLock*)Unsafe.AsPointer(ref _state));
        }
    }

    public readonly unsafe struct RwLockRef<T> : IDisposable where T : unmanaged
    {
        private readonly T* _ptr;
        private readonly RwLock* _state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockRef(T* ptr, RwLock* state)
        {
            _ptr = ptr;
            _state = state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Unwrap() => ref Unsafe.AsRef<T>(_ptr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _state->ExitWrite();
    }

    public readonly unsafe struct RwLockReadOnlyRef<T> : IDisposable where T : unmanaged
    {
        private readonly T* _ptr;
        private readonly RwLock* _state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockReadOnlyRef(T* ptr, RwLock* state)
        {
            _ptr = ptr;
            _state = state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T Unwrap() => ref Unsafe.AsRef<T>(_ptr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _state->ExitRead();
    }
}