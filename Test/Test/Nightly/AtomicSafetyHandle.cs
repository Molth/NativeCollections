using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace NativeCollections
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct AtomicSafetyHandle : IDisposable
    {
        private readonly ulong* _ptr;
        private readonly ulong _value;

        public AtomicSafetyHandle(ulong* ptr)
        {
            _ptr = ptr;
            _value = Interlocked.Read(ref Unsafe.AsRef<ulong>(ptr));
        }

        public AtomicSafetyHandle(ulong* ptr, ulong value)
        {
            _ptr = ptr;
            _value = value;
        }

        public void Dispose() => NativeMemoryAllocator.AlignedFree(_ptr);

        public AtomicSafetyHandle Clone() => new(_ptr);

        public void Bump() => Interlocked.Increment(ref Unsafe.AsRef<ulong>(_ptr));

        public bool TryValidate() => _value == Interlocked.Read(ref Unsafe.AsRef<ulong>(_ptr));

        public static AtomicSafetyHandle Create()
        {
            var handle = NativeMemoryAllocator.AlignedAlloc<ulong>(1);
            var definite = Unsafe.AsRef<ulong>(handle);
            Unsafe.AsRef<ulong>(handle) = definite + uint.MaxValue;
            return new AtomicSafetyHandle(handle, Unsafe.AsRef<ulong>(handle));
        }
    }
}