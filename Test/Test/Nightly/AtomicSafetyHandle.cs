using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace NativeCollections
{
    internal static unsafe class AtomicSafetyHandleManager
    {
        private const int PER_CHUNK_SIZE = 128;

        private static UnsafeSegQueue<nint> _freeList;
        private static UnsafeSegQueue<NativeMemoryArray<long>> _allocatedList;
        private static SpinLock _syncRoot;

        public static void Clear()
        {
            while (_freeList.TryDequeue(out _))
                ;

            while (_allocatedList.TryDequeue(out var handles))
                handles.Dispose();
        }

        public static long* Rent()
        {
            var spinWait = new SpinWait();
            while (true)
            {
                if (_freeList.TryDequeue(out var handle))
                {
                    var ptr = (long*)handle;
                    Interlocked.Increment(ref Unsafe.AsRef<long>(ptr));
                    return ptr;
                }

                var lockTaken = false;
                _syncRoot.TryEnter(ref lockTaken);
                if (lockTaken)
                {
                    var newHandles = new NativeMemoryArray<long>(PER_CHUNK_SIZE);
                    _allocatedList.Enqueue(newHandles);
                    for (var i = 0; i < PER_CHUNK_SIZE; ++i)
                        _freeList.Enqueue((nint)newHandles[i]);

                    _syncRoot.Exit();
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

            _freeList.Enqueue((nint)ptr);
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