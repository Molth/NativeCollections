using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Examples
{
    public static class SimpleTidManager
    {
        [ThreadStatic] private static int _tid;

        private static readonly ConcurrentQueue<int> FreeList = new();

        private static int _global;

        public static SimpleTidGuard Guard() => new(Rent());

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

            _tid = Interlocked.Increment(ref _global);
            return _tid;
        }

        public static void Return(int tid)
        {
            if (tid == 0 || _tid != tid)
            {
                Console.WriteLine("ERROR");
                return;
            }

            _tid = 0;
            FreeList.Enqueue(tid);
        }
    }
}