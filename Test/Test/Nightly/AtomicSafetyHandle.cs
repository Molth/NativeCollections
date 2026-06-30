using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace NativeCollections
{
    internal static unsafe class AtomicSafetyHandleManager
    {
        private const int PER_CHUNK_SIZE = 4096;

        private static readonly ConcurrentQueue<nint> FreeList = new();
        private static readonly ConcurrentQueue<NativeMemoryArray<long>> AllocatedList = new();
        private static readonly Lock SyncRoot = new();

        public static void Clear()
        {
            while (FreeList.TryDequeue(out _))
                ;

            while (AllocatedList.TryDequeue(out var handles))
                handles.Dispose();
        }

        public static long* Rent()
        {
            var spinWait = new SpinWait();
            while (true)
            {
                if (FreeList.TryDequeue(out var handle))
                {
                    var ptr = (long*)handle;
                    return ptr;
                }

                if (SyncRoot.TryEnter())
                {
                    var newHandles = new NativeMemoryArray<long>(PER_CHUNK_SIZE);
                    AllocatedList.Enqueue(newHandles);
                    for (var i = 0; i < PER_CHUNK_SIZE; ++i)
                        FreeList.Enqueue((nint)newHandles[i]);
                }

                spinWait.SpinOnce();
            }
        }

        public static void Return(long* ptr)
        {
            var spinWait = new SpinWait();
            ref var location = ref Unsafe.AsRef<long>(ptr);
            while (true)
            {
                var value = Volatile.Read(ref location);
                var valueAsInt2 = Unsafe.As<long, (int, int)>(ref value);
                valueAsInt2.Item1 += 1;
                if (Interlocked.CompareExchange(ref location, Unsafe.As<(int, int), long>(ref valueAsInt2), value) == value)
                    break;

                spinWait.SpinOnce();
            }

            FreeList.Enqueue((nint)ptr);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct AtomicSafetyHandle : IDisposable
    {
        private readonly long* _ptr;
        private readonly long _value;

        public bool IsCreated => _ptr != null;

        private AtomicSafetyHandle(long* ptr)
        {
            _ptr = ptr;
            _value = Volatile.Read(ref Unsafe.AsRef<long>(ptr));
        }

        public void Dispose()
        {
            ref var location = ref Unsafe.AsRef<long>(_ptr);
            var value = Volatile.Read(ref location);
            if (Unsafe.As<long, (int, int)>(ref Unsafe.AsRef(in _value)).Item1 != Unsafe.As<long, (int, int)>(ref value).Item1)
                throw new ArgumentException();

            AtomicSafetyHandleManager.Return(_ptr);
        }

        public AtomicSafetyHandle Clone() => new(_ptr);

        public void Bump()
        {
            var spinWait = new SpinWait();
            ref var location = ref Unsafe.AsRef<long>(_ptr);
            while (true)
            {
                var value = Volatile.Read(ref location);
                var valueAsInt2 = Unsafe.As<long, (int, int)>(ref value);
                valueAsInt2.Item2 += 1;
                if (Interlocked.CompareExchange(ref location, Unsafe.As<(int, int), long>(ref valueAsInt2), value) == value)
                    break;

                spinWait.SpinOnce();
            }
        }

        public bool TryValidate() => _value == Volatile.Read(ref Unsafe.AsRef<long>(_ptr));

        public static AtomicSafetyHandle Create()
        {
            var handle = AtomicSafetyHandleManager.Rent();
            return new AtomicSafetyHandle(handle);
        }
    }
}