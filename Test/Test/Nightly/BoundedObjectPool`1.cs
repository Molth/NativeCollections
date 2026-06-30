using System;
using System.Threading;

namespace Examples
{
    public sealed class BoundedObjectPool<T> where T : class
    {
        private object? _fastItem;
        private readonly RingBufferNotPow2<T>? _buffer;
        private readonly int _capacity;

        public BoundedObjectPool(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
            if (capacity != 1)
                _buffer = capacity == 2 ? new RingBufferNotPow2<T>(capacity) : new RingBufferNotPow2<T>(capacity - 1);
            _capacity = capacity;
        }

        public int Capacity => _capacity;

        public bool TryDequeue(out T? item)
        {
            if (_capacity != 1)
            {
                if (_capacity != 2)
                {
                    var obj1 = Interlocked.Exchange(ref _fastItem, null);
                    if (obj1 != null)
                    {
                        item = (T)obj1;
                        return true;
                    }
                }

                return _buffer!.TryDequeue(out item);
            }

            var obj2 = Interlocked.Exchange(ref _fastItem, null);
            if (obj2 != null)
            {
                item = (T)obj2;
                return true;
            }

            item = null;
            return false;
        }

        public bool TryEnqueue(T? item)
        {
            if (_capacity != 1)
                return (_capacity != 2 && Interlocked.CompareExchange(ref _fastItem, item, null) == null) || _buffer!.TryEnqueue(item);
            return Interlocked.CompareExchange(ref _fastItem, item, null) == null;
        }
    }
}