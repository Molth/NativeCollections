using System;
using NativeCollections;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

namespace Examples
{
    public readonly struct SimpleTidManager2 : IDisposable
    {
        private readonly NativeConcurrentFixedSizeBucket _freeList;

        public SimpleTidManager2(int maxThreads)
        {
            _freeList = new NativeConcurrentFixedSizeBucket(maxThreads);
        }

        public int Rent()
        {
            if (_freeList.TryRent(out var tid))
            {
                return tid;
            }

            throw new Exception("FULL");
        }

        public void Return(int tid)
        {
            _freeList.Return(tid);
        }

        public void Dispose()
        {
            _freeList.Dispose();
        }
    }
}