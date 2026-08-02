using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Examples
{
    public sealed class BoundedObjectPool<T> where T : class?
    {
        private object? _fastItem;
        private readonly RingBuffer<object>? _buffer;
        private readonly int _capacity;

        public BoundedObjectPool(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

            if (capacity != 1)
                _buffer = capacity == 2 ? new RingBuffer<object>(2) : new RingBuffer<object>(capacity - 1);

            _capacity = capacity;
        }

        public int Capacity => _capacity;

        public bool TryDequeue([NotNullWhen(true)] out T? item)
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

                var result = _buffer!.TryDequeue(out var obj2);
                item = (T?)obj2;
                return result;
            }

            var obj3 = Interlocked.Exchange(ref _fastItem, null);
            if (obj3 != null)
            {
                item = (T)obj3;
                return true;
            }

            item = null;
            return false;
        }

        public bool TryEnqueue(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (_capacity != 1)
                return (_capacity != 2 && Interlocked.CompareExchange(ref _fastItem, item, null) == null) || _buffer!.TryEnqueue(item);

            return Interlocked.CompareExchange(ref _fastItem, item, null) == null;
        }
    }
}