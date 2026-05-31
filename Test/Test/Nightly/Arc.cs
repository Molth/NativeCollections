using System;
using System.Runtime.CompilerServices;
using System.Threading;
using NativeCollections;

namespace Examples
{
    public readonly unsafe struct Arc<T> : IDisposable where T : unmanaged, IDisposable
    {
        private readonly int* _state;
        private readonly T* _value;

        private Arc(int* state, T* value)
        {
            _state = state;
            _value = value;
        }

        public void Dispose()
        {
            if (Interlocked.Decrement(ref Unsafe.AsRef<int>(_state)) > 0)
                return;
            _value->Dispose();
            NativeMemoryAllocator.AlignedFree(_state);
        }

        public Arc<T> Clone()
        {
            Interlocked.Increment(ref Unsafe.AsRef<int>(_state));
            return this;
        }

        public static Arc<T> Create(in T value)
        {
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<T>());
            var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<int>(), alignment);
            var buckets = (int*)NativeMemoryAllocator.AlignedAllocZeroed((uint)(bucketsByteCount + Unsafe.SizeOf<T>()), alignment);
            var entries = (T*)((nint)buckets + (nint)bucketsByteCount);
            Unsafe.AsRef<int>(buckets) = 1;
            Unsafe.AsRef<T>(entries) = value;
            return new Arc<T>(buckets, entries);
        }
    }
}