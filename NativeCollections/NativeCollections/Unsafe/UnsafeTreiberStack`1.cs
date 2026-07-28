using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using crossbeam;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe concurrentStack
    ///     (Slower than ConcurrentStack, disable Enumerator, try peek, push/pop range either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community)]
    public unsafe struct UnsafeTreiberStack<T> : IIsCreated, IDisposable, IEquatable<UnsafeTreiberStack<T>> where T : unmanaged
    {
        /// <summary>
        ///     Padding to avoid false sharing with adjacent data.
        /// </summary>
        private readonly Padding _padding;

        /// <summary>
        ///     The stack is a singly linked list, and only remembers the head.
        /// </summary>
        private CachePaddedAtomicPtr<Node> _head;

        /// <summary>
        ///     Epoch collector
        /// </summary>
        private UnsafeEpochCollector _ebr;

        /// <summary>
        ///     Arbitrary number to cap backoff
        /// </summary>
        private const int BACKOFF_MAX_YIELDS = 8;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => true;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>true if this is empty; otherwise, false.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of this property is recommended rather than
        ///     retrieving the number of items from the <see cref="Count" /> property and comparing it to 0.
        ///     However, as this collection is intended to be accessed concurrently, it may be the case that another thread will
        ///     modify the collection after <see cref="IsEmpty" /> returns, thus invalidating the result.
        /// </remarks>
        public bool IsEmpty => _head.load(Ordering.Relaxed) == null;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>The number of elements contained in this.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of the <see cref="IsEmpty" />
        ///     property is recommended rather than retrieving the number of items from the <see cref="Count" />
        ///     property and comparing it to 0.
        /// </remarks>
        public int Count
        {
            get
            {
                var count = 0;
                using (_ebr.Scope())
                {
                    for (var node = _head.load(Ordering.Relaxed); node != null; node = node->Next)
                        count++;
                }

                return count;
            }
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeTreiberStack<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeTreiberStack<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeTreiberStack<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeTreiberStack<T> left, UnsafeTreiberStack<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeTreiberStack<T> left, UnsafeTreiberStack<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var node = _head.get_mut();
            while (node != null)
            {
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _ebr.Dispose();
        }

        /// <summary>
        ///     Removes all objects from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var node = _head.swap(null);
            using (var scope = _ebr.Scope())
            {
                while (node != null)
                {
                    var temp = node;
                    node = node->Next;
                    scope.Retire(temp);
                }
            }
        }

        /// <summary>
        ///     Inserts an object at the top of this.
        /// </summary>
        /// <param name="item">
        ///     The object to push onto this.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            var newNode = NativeMemoryAllocator.AlignedAlloc<Node>(1);
            newNode->Value = item;

            var head = _head.load(Ordering.Relaxed);
            newNode->Next = head;
            if (_head.compare_exchange(head, newNode) == head)
                return;

            PushSlow(newNode);
        }

        /// <summary>
        ///     Inserts an object at the top of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushSlow(Node* newNode)
        {
            var spinWait = new UnsafeSpinWait();
            Node* head;
            do
            {
                spinWait.SpinOnce(-1);
                head = _head.load(Ordering.Acquire);
                newNode->Next = head;
            } while (_head.compare_exchange(head, newNode) != head);
        }

        /// <summary>
        ///     Attempts to pop and return the object at the top of this.
        /// </summary>
        /// <param name="result">
        ///     When this method returns, if the operation was successful, <paramref name="result" /> contains the object removed.
        ///     If no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        ///     true if an element was removed and returned from the top of this successfully;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            using (var scope = _ebr.Scope())
            {
                var head = _head.load(Ordering.Acquire);
                if (head == null)
                {
                    result = default;
                    return false;
                }

                var next = head->Next;
                if (_head.compare_exchange(head, next) == head)
                {
                    result = head->Value;
                    scope.Retire(head);
                    return true;
                }
            }

            return TryPopSlow(out result);
        }

        /// <summary>
        ///     Attempts to pop and return the object at the top of this.
        /// </summary>
        /// <param name="result">
        ///     When this method returns, if the operation was successful, <paramref name="result" /> contains the object removed.
        ///     If no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        ///     true if an element was removed and returned from the top of this successfully;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryPopSlow(out T result)
        {
            var random = UnsafeXorshift32.Create();
            var spinWait = new UnsafeSpinWait();
            var backoff = 1;
            while (true)
            {
                using (var scope = _ebr.Scope())
                {
                    var head = _head.load(Ordering.Acquire);
                    if (head == null)
                    {
                        result = default;
                        return false;
                    }

                    var next = head->Next;
                    if (_head.compare_exchange(head, next) == head)
                    {
                        result = head->Value;
                        scope.Retire(head);
                        return true;
                    }
                }

                for (var i = 0; i < backoff; ++i)
                    spinWait.SpinOnce(-1);

                backoff = spinWait.NextSpinWillYield ? random.NextInt32(1, BACKOFF_MAX_YIELDS) : backoff * 2;
            }
        }

        /// <summary>
        ///     A simple (internal) node type used to store elements of concurrent stacks and queues.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Node
        {
            /// <summary>
            ///     Value of the node.
            /// </summary>
            public T Value;

            /// <summary>
            ///     Next pointer.
            /// </summary>
            public Node* Next;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeTreiberStack<T> Empty => default;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeTreiberStack<T> Create() => new();
    }
}