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
        public RwLockWriteRefScope<T> EnterWriteRefScope() => new(NativeRef<T>.Create(ref _value), _state.EnterWriteRefScope());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RwLockReadRefScope<T> EnterReadRefScope() => new(NativeRef<T>.Create(ref _value), _state.EnterReadRefScope());
    }

    [IsAssignableTo(typeof(IIsCreated), typeof(IDisposable))]
    public readonly ref struct RwLockWriteRefScope<T> : IIsCreated, IDisposable
    {
        private readonly NativeRef<T> _ptr;
        private readonly NativeReaderWriterLockRefScope _scope;

        public bool IsCreated => _ptr.IsCreated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockWriteRefScope(NativeRef<T> ptr, NativeReaderWriterLockRefScope scope)
        {
            _ptr = ptr;
            _scope = scope;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AsRef() => ref _ptr.AsRef();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _scope.Dispose();
    }

    [IsAssignableTo(typeof(IIsCreated), typeof(IDisposable))]
    public readonly ref struct RwLockReadRefScope<T> : IIsCreated, IDisposable
    {
        private readonly NativeRef<T> _ptr;
        private readonly NativeReaderWriterLockRefScope _scope;

        public bool IsCreated => _ptr.IsCreated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal RwLockReadRefScope(NativeRef<T> ptr, NativeReaderWriterLockRefScope scope)
        {
            _ptr = ptr;
            _scope = scope;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T AsRef() => ref _ptr.AsRef();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _scope.Dispose();
    }
}