using System;
using System.Runtime.CompilerServices;
using NativeCollections;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

namespace Examples
{
    public unsafe struct RwLock<T>
    {
        private T _value;
        private UnsafeReaderWriterLock _state;

        public RwLock(T value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _value = value;
            _state = new UnsafeReaderWriterLock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RwLockMutRefScope<T> MutRefScope()
        {
            _state.EnterWrite();
            return new RwLockMutRefScope<T>(NativeRef<T>.Create(ref _value), NativeRef<UnsafeReaderWriterLock>.Create(ref _state));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RwLockReadOnlyRefScope<T> ReadOnlyRefScope()
        {
            _state.EnterRead();
            return new RwLockReadOnlyRefScope<T>(NativeRef<T>.Create(ref _value), NativeRef<UnsafeReaderWriterLock>.Create(ref _state));
        }
    }

    [IsAssignableTo(typeof(IDisposable))]
    public readonly ref struct RwLockMutRefScope<T> : IDisposable
    {
        private readonly NativeRef<T> _ptr;
        private readonly NativeRef<UnsafeReaderWriterLock> _state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockMutRefScope(NativeRef<T> ptr, NativeRef<UnsafeReaderWriterLock> state)
        {
            _ptr = ptr;
            _state = state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AsRef() => ref _ptr.AsRef();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _state.AsRef().ExitWrite();
    }

    [IsAssignableTo(typeof(IDisposable))]
    public readonly ref struct RwLockReadOnlyRefScope<T> : IDisposable
    {
        private readonly NativeRef<T> _ptr;
        private readonly NativeRef<UnsafeReaderWriterLock> _state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockReadOnlyRefScope(NativeRef<T> ptr, NativeRef<UnsafeReaderWriterLock> state)
        {
            _ptr = ptr;
            _state = state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T AsRef() => ref _ptr.AsRef();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _state.AsRef().ExitRead();
    }
}