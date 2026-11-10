using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a thread-safe last-in, first-out collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    /// <remarks>
    ///     This type does not provide enumeration, peek, push-range, or pop-range operations.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [InternalCollection(FromType.Standard | FromType.Community)]
    internal unsafe struct RawTreiberStack<T> : IIsCreated, IDisposable, IEquatable<RawTreiberStack<T>> where T : unmanaged
    {
        /// <summary>
        ///     Epoch-based reclamation (EBR) collector for safe memory reclamation of retired nodes.
        /// </summary>
        private RawEpochCollector _ebr;

        /// <summary>
        ///     The stack is a singly linked list, and only remembers the head.
        /// </summary>
        private CachePaddedAtomicPtr<Node> _head;

        /// <summary>
        ///     Arbitrary number to cap backoff.
        /// </summary>
        private const int BACKOFF_MAX_YIELDS = 8;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Always <see langword="true" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The default value of this type is a valid empty stack: the head pointer starts
        ///                 <see langword="null" /> and the first node is allocated by the first push,
        ///                 so there is no separate initialization step.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public readonly bool IsCreated => true;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value><see langword="true" /> if this is empty; otherwise, <see langword="false" />.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of this property is recommended rather
        ///     than retrieving the number of items from the <see cref="Count" /> property and comparing it to 0.
        ///     However, as this collection is intended to be accessed concurrently, it may be the case that another
        ///     thread will modify the collection after <see cref="IsEmpty" /> returns, thus invalidating the result.
        /// </remarks>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _head.Load(Ordering.Relaxed) == null;
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>The number of elements contained in this.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of the
        ///     <see cref="IsEmpty" /> property is recommended rather than retrieving the
        ///     number of items from the <see cref="Count" /> property and comparing it to 0.
        /// </remarks>
        public int Count
        {
            get
            {
                var count = 0;
                using (_ebr.EnterRefScope())
                {
                    for (var node = _head.Load(Ordering.Relaxed); node != null; node = node->Next)
                        count = count.WrappingAdd(1);
                }

                return count;
            }
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(RawTreiberStack<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is RawTreiberStack<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("RawTreiberStack<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(RawTreiberStack<T> left, RawTreiberStack<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(RawTreiberStack<T> left, RawTreiberStack<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var node = _head.AsRef();
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
        public void Clear()
        {
            var node = _head.Exchange(null);
            using (var scope = _ebr.EnterRefScope())
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
            var newNode = NativeMemoryAllocator.AlignedAlloc<Node>(Usize.FromI32Unchecked(1));
            newNode->Value = item;

            var head = _head.Load(Ordering.Relaxed);
            newNode->Next = head;
            if (_head.CompareExchange(newNode, head) == head)
                return;

            PushSlow(newNode);
        }

        /// <summary>
        ///     Inserts an object at the top of this.
        /// </summary>
        /// <param name="newNode">A pointer to the newly allocated node to insert at the top of this.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void PushSlow(Node* newNode)
        {
            UnsafeSpinWait spinWait = default;
            while (true)
            {
                spinWait.SpinOnce(-1);

                var head = _head.Load(Ordering.Acquire);
                newNode->Next = head;

                if (_head.CompareExchange(newNode, head) == head)
                    break;
            }
        }

        /// <summary>
        ///     Attempts to pop and return the object at the top of this.
        /// </summary>
        /// <param name="result">
        ///     When this method returns, if the operation was successful, <paramref name="result" /> contains
        ///     the object removed. If no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if an element was successfully removed and
        ///     returned from the top of this; otherwise, <see langword="false" />.
        /// </returns>
        public bool TryPop(out T result)
        {
            using (var scope = _ebr.EnterRefScope())
            {
                var head = _head.Load(Ordering.Acquire);
                if (head == null)
                {
                    result = default;
                    return false;
                }

                var next = head->Next;
                if (_head.CompareExchange(next, head) == head)
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
        ///     When this method returns, if the operation was successful, <paramref name="result" /> contains
        ///     the object removed. If no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if an element was successfully removed and
        ///     returned from the top of this; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryPopSlow(out T result)
        {
            var random = UnsafeSmallRandom.Shared;
            UnsafeSpinWait spinWait = default;
            var backoff = 1;
            while (true)
            {
                using (var scope = _ebr.EnterRefScope())
                {
                    var head = _head.Load(Ordering.Acquire);
                    if (head == null)
                    {
                        result = default;
                        return false;
                    }

                    var next = head->Next;
                    if (_head.CompareExchange(next, head) == head)
                    {
                        result = head->Value;
                        scope.Retire(head);
                        return true;
                    }
                }

                for (var i = 0; i < backoff; i = i.WrappingAdd(1))
                    spinWait.SpinOnce(-1);

                backoff = spinWait.NextSpinWillYield ? random.NextI32(1, BACKOFF_MAX_YIELDS) : backoff.WrappingMul(2);
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
    }
}