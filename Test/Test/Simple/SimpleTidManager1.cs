using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Examples
{
    public readonly struct SimpleTidGuard1 : IDisposable
    {
        public readonly int ThreadId;

        internal SimpleTidGuard1(int threadId) => ThreadId = threadId;

        public void Dispose() => SimpleTidManager1.Return(ThreadId);

        public static implicit operator int(SimpleTidGuard1 value) => value.ThreadId;
    }

    public static class SimpleTidManager1
    {
        [ThreadStatic] private static int _tid;

        private static readonly ConcurrentQueue<int> FreeList = new();

        private static int _global;

        public static SimpleTidGuard1 Guard() => new(Rent());

        public static int Rent()
        {
            var tid = _tid;
            if (tid != 0)
                return tid;

            if (FreeList.TryDequeue(out tid))
            {
                _tid = tid;
                return tid;
            }

            _tid = Interlocked.Increment(ref _global) - 1;
            return _tid;
        }

        public static void Return(int tid)
        {
            if (_tid == tid)
                _tid = 0;

            FreeList.Enqueue(tid);
        }
    }
}