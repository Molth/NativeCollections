using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.NativeSortedSet;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a collection of objects that is maintained in sorted order.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocSortedSet<T> : IIsCreated, IEquatable<StackallocSortedSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IComparable<T>
    {
        /// <summary>
        ///     The root node.
        /// </summary>
        private Node<T>* _root;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Represents a memory pool that provides reusable fixed-size memory blocks.
        /// </summary>
        private StackallocFixedSizeStackMemoryPool<Node<T>> _nodePool;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _nodePool.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the minimum value in this.
        /// </summary>
        /// <returns>The minimum value in the set.</returns>
        public readonly T? Min
        {
            get
            {
                if (UnsafeHelpers.IsNull(_root))
                    return default;
                var current = _root;
                while (!UnsafeHelpers.IsNull(current->Left))
                    current = current->Left;
                return current->Item;
            }
        }

        /// <summary>
        ///     Gets the maximum value in this.
        /// </summary>
        /// <returns>The maximum value in the set.</returns>
        public readonly T? Max
        {
            get
            {
                if (UnsafeHelpers.IsNull(_root))
                    return default;
                var current = _root;
                while (!UnsafeHelpers.IsNull(current->Right))
                    current = current->Right;
                return current->Item;
            }
        }

        /// <summary>
        ///     Calculates the minimum number of bytes required to store a specified number of elements,
        ///     taking into account alignment requirements for the underlying buffer.
        /// </summary>
        /// <param name="capacity">The number of elements to store. Must be non-negative.</param>
        /// <returns>
        ///     The minimum byte count needed to allocate a buffer capable of
        ///     holding <paramref name="capacity" /> elements with proper alignment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity) => StackallocFixedSizeStackMemoryPool<Node<T>>.GetByteCount(capacity);

        /// <summary>
        ///     Initializes a new instance of this class
        ///     that uses a caller-provided byte buffer as storage.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage.
        ///     It must be large enough to store the specified number of elements with proper alignment.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the stack can hold.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="capacity" /> is negative, or if <paramref name="buffer" /> is too small
        ///     to hold the required number of elements (including alignment padding).
        /// </exception>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocSortedSet([MustBePinned] Span<byte> buffer, int capacity)
        {
            var nodePool = new StackallocFixedSizeStackMemoryPool<Node<T>>(buffer, capacity);
            _root = null;
            _count = 0;
            _version = 0;
            _nodePool = nodePool;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(StackallocSortedSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is StackallocSortedSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("StackallocSortedSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(StackallocSortedSet<T> left, StackallocSortedSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(StackallocSortedSet<T> left, StackallocSortedSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _nodePool.Reset();
            _root = null;
            _count = 0;
            ++_version;
        }

        /// <summary>
        ///     Adds the specified element to this.
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(in T item)
        {
            if (UnsafeHelpers.IsNull(_root))
            {
                if (!_nodePool.TryRent(out _root))
                    return InsertResult.InsufficientCapacity;
                _root->Item = item;
                _root->Left = null;
                _root->Right = null;
                _root->Color = NodeColor.Black;
                _count = 1;
                _version++;
                return InsertResult.Success;
            }

            var current = _root;
            Node<T>* parent = null;
            Node<T>* grandParent = null;
            Node<T>* greatGrandParent = null;
            _version++;
            var order = 0;
            while (!UnsafeHelpers.IsNull(current))
            {
                order = item.CompareTo(current->Item);
                if (order == 0)
                {
                    _root->ColorBlack();
                    return InsertResult.AlreadyExists;
                }

                if (current->Is4Node)
                {
                    current->Split4Node();
                    if (Node<T>.IsNonNullRed(parent))
                        InsertionBalance(current, parent, grandParent, greatGrandParent);
                }

                greatGrandParent = grandParent;
                grandParent = parent;
                parent = current;
                current = order < 0 ? current->Left : current->Right;
            }

            if (!_nodePool.TryRent(out var node))
            {
                _root->ColorBlack();
                return InsertResult.InsufficientCapacity;
            }

            node->Item = item;
            node->Left = null;
            node->Right = null;
            node->Color = NodeColor.Red;
            if (order > 0)
                parent->Right = node;
            else
                parent->Left = node;
            if (parent->IsRed)
                InsertionBalance(node, parent, grandParent, greatGrandParent);
            _root->ColorBlack();
            ++_count;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Adds a new element to the set, or updates an existing element
        ///     that compares equal to the specified <paramref name="equalValue" />.
        /// </summary>
        /// <param name="equalValue">The value used for equality comparison to locate an existing element.</param>
        /// <param name="actualValue">The value to add, or to replace the existing element with if found.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAddOrUpdate(in T equalValue, in T actualValue)
        {
            var node = FindNode(equalValue);
            if (UnsafeHelpers.IsNull(node))
                return TryAdd(actualValue);
            node->Item = actualValue;
            _version++;
            return InsertResult.Overwritten;
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from this.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully removed;
        ///     otherwise, <see langword="false" />.
        ///     This method also returns <see langword="false" /> if <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T item)
        {
            if (UnsafeHelpers.IsNull(_root))
                return false;
            _version++;
            var current = _root;
            Node<T>* parent = null;
            Node<T>* grandParent = null;
            Node<T>* match = null;
            Node<T>* parentOfMatch = null;
            var foundMatch = false;
            while (!UnsafeHelpers.IsNull(current))
            {
                if (current->Is2Node)
                {
                    if (UnsafeHelpers.IsNull(parent))
                    {
                        current->ColorRed();
                    }
                    else
                    {
                        var sibling = parent->GetSibling(current);
                        if (sibling->IsRed)
                        {
                            if (parent->Right == sibling)
                                parent->RotateLeft();
                            else
                                parent->RotateRight();
                            parent->ColorRed();
                            sibling->ColorBlack();
                            ReplaceChildOrRoot(grandParent, parent, sibling);
                            grandParent = sibling;
                            if (parent == match)
                                parentOfMatch = sibling;
                            sibling = parent->GetSibling(current);
                        }

                        if (sibling->Is2Node)
                        {
                            parent->Merge2Nodes();
                        }
                        else
                        {
                            var newGrandParent = parent->Rotate(parent->GetRotation(current, sibling));
                            newGrandParent->Color = parent->Color;
                            parent->ColorBlack();
                            current->ColorRed();
                            ReplaceChildOrRoot(grandParent, parent, newGrandParent);
                            if (parent == match)
                                parentOfMatch = newGrandParent;
                        }
                    }
                }

                var order = foundMatch ? -1 : item.CompareTo(current->Item);
                if (order == 0)
                {
                    foundMatch = true;
                    match = current;
                    parentOfMatch = parent;
                }

                grandParent = parent;
                parent = current;
                current = order < 0 ? current->Left : current->Right;
            }

            if (!UnsafeHelpers.IsNull(match))
            {
                ReplaceNode(match, parentOfMatch, parent, grandParent);
                --_count;
                _nodePool.Return(match);
            }

            if (!UnsafeHelpers.IsNull(_root))
                _root->ColorBlack();
            return foundMatch;
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object and returns the equal value from this.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value of
        ///     <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="equalValue" /> is successfully removed;
        ///     otherwise, <see langword="false" />.
        ///     This method also returns <see langword="false" /> if <paramref name="equalValue" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T equalValue, out T actualValue)
        {
            if (UnsafeHelpers.IsNull(_root))
            {
                actualValue = default;
                return false;
            }

            _version++;
            var current = _root;
            Node<T>* parent = null;
            Node<T>* grandParent = null;
            Node<T>* match = null;
            Node<T>* parentOfMatch = null;
            var foundMatch = false;
            while (!UnsafeHelpers.IsNull(current))
            {
                if (current->Is2Node)
                {
                    if (UnsafeHelpers.IsNull(parent))
                    {
                        current->ColorRed();
                    }
                    else
                    {
                        var sibling = parent->GetSibling(current);
                        if (sibling->IsRed)
                        {
                            if (parent->Right == sibling)
                                parent->RotateLeft();
                            else
                                parent->RotateRight();
                            parent->ColorRed();
                            sibling->ColorBlack();
                            ReplaceChildOrRoot(grandParent, parent, sibling);
                            grandParent = sibling;
                            if (parent == match)
                                parentOfMatch = sibling;
                            sibling = parent->GetSibling(current);
                        }

                        if (sibling->Is2Node)
                        {
                            parent->Merge2Nodes();
                        }
                        else
                        {
                            var newGrandParent = parent->Rotate(parent->GetRotation(current, sibling));
                            newGrandParent->Color = parent->Color;
                            parent->ColorBlack();
                            current->ColorRed();
                            ReplaceChildOrRoot(grandParent, parent, newGrandParent);
                            if (parent == match)
                                parentOfMatch = newGrandParent;
                        }
                    }
                }

                var order = foundMatch ? -1 : equalValue.CompareTo(current->Item);
                if (order == 0)
                {
                    foundMatch = true;
                    match = current;
                    parentOfMatch = parent;
                }

                grandParent = parent;
                parent = current;
                current = order < 0 ? current->Left : current->Right;
            }

            if (!UnsafeHelpers.IsNull(match))
            {
                actualValue = match->Item;
                ReplaceNode(match, parentOfMatch, parent, grandParent);
                --_count;
                _nodePool.Return(match);
            }
            else
            {
                actualValue = default;
            }

            if (!UnsafeHelpers.IsNull(_root))
                _root->ColorBlack();
            return foundMatch;
        }

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>true if this contains the specified element; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in T item) => !UnsafeHelpers.IsNull(FindNode(item));

        /// <summary>
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value of
        ///     <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of
        ///     a newly constructed one (so that more sharing of references can occur) or to look up
        ///     a value that has more complete data than the value you currently have, although their
        ///     comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(in T equalValue, out T actualValue)
        {
            var node = FindNode(equalValue);
            if (!UnsafeHelpers.IsNull(node))
            {
                actualValue = node->Item;
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value of
        ///     <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of
        ///     a newly constructed one (so that more sharing of references can occur) or to look up
        ///     a value that has more complete data than the value you currently have, although their
        ///     comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(in T equalValue, out NativePtr<T> actualValue)
        {
            var node = FindNode(equalValue);
            if (!UnsafeHelpers.IsNull(node))
            {
                actualValue = new NativePtr<T>(UnsafeHelpers.AsPointer(ref node->Item));
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Restores red‑black tree balance after insertion by performing rotations and recoloring.
        /// </summary>
        /// <param name="current">The newly inserted red node.</param>
        /// <param name="parent">The parent of <paramref name="current" />.</param>
        /// <param name="grandParent">The grandparent of <paramref name="current" />.</param>
        /// <param name="greatGrandParent">
        ///     The great‑grandparent of <paramref name="current" />,
        ///     used to update the tree root.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InsertionBalance(Node<T>* current, Node<T>* parent, Node<T>* grandParent, Node<T>* greatGrandParent)
        {
            var parentIsOnRight = grandParent->Right == parent;
            var currentIsOnRight = parent->Right == current;
            Node<T>* newChildOfGreatGrandParent;
            if (parentIsOnRight == currentIsOnRight)
                newChildOfGreatGrandParent = currentIsOnRight ? grandParent->RotateLeft() : grandParent->RotateRight();
            else
                newChildOfGreatGrandParent = currentIsOnRight ? grandParent->RotateLeftRight() : grandParent->RotateRightLeft();
            grandParent->ColorRed();
            newChildOfGreatGrandParent->ColorBlack();
            ReplaceChildOrRoot(greatGrandParent, grandParent, newChildOfGreatGrandParent);
        }

        /// <summary>
        ///     Replaces a child node of a given parent with a new child,
        ///     or updates the root if the parent is <see langword="null" />.
        /// </summary>
        /// <param name="parent">
        ///     The parent node whose child is being replaced.
        ///     May be <see langword="null" />.
        /// </param>
        /// <param name="child">The child node to replace.</param>
        /// <param name="newChild">The new child node.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReplaceChildOrRoot(Node<T>* parent, Node<T>* child, Node<T>* newChild)
        {
            if (!UnsafeHelpers.IsNull(parent))
                parent->ReplaceChild(child, newChild);
            else
                _root = newChild;
        }

        /// <summary>
        ///     Replaces a matched node with its successor during deletion,
        ///     adjusting the tree structure accordingly.
        /// </summary>
        /// <param name="match">The node to be removed.</param>
        /// <param name="parentOfMatch">The parent of <paramref name="match" />.</param>
        /// <param name="successor">The successor node that will replace <paramref name="match" />.</param>
        /// <param name="parentOfSuccessor">The parent of <paramref name="successor" />.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReplaceNode(Node<T>* match, Node<T>* parentOfMatch, Node<T>* successor, Node<T>* parentOfSuccessor)
        {
            if (successor == match)
            {
                successor = match->Left;
            }
            else
            {
                if (!UnsafeHelpers.IsNull(successor->Right))
                    successor->Right->ColorBlack();
                if (parentOfSuccessor != match)
                {
                    parentOfSuccessor->Left = successor->Right;
                    successor->Right = match->Right;
                }

                successor->Left = match->Left;
            }

            if (!UnsafeHelpers.IsNull(successor))
                successor->Color = match->Color;
            ReplaceChildOrRoot(parentOfMatch, match, successor);
        }

        /// <summary>
        ///     Searches for a node with the specified key.
        /// </summary>
        /// <param name="item">The key to locate.</param>
        /// <returns>
        ///     A node if found;
        ///     otherwise, <see langword="null" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Node<T>* FindNode(in T item)
        {
            var current = _root;
            while (!UnsafeHelpers.IsNull(current))
            {
                var order = item.CompareTo(current->Item);
                if (order == 0)
                    return current;
                current = order < 0 ? current->Left : current->Right;
            }

            return null;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this.
        ///     The actual number of copied elements is limited by the span's length, the specified count,
        ///     and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from the this.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (UnsafeHelpers.IsNull(_root))
                return 0;
            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var index = 0;
            using (var nodeStack = new UnsafeStack<NativePtr<Node<T>>>(2 * BitOperationsHelpers.Log2((uint)(_count + 1))))
            {
                for (var node = _root; !UnsafeHelpers.IsNull(node); node = node->Left)
                    nodeStack.Push(node);
                while (nodeStack.Count != 0)
                {
                    if (index >= count)
                        break;
                    var node1 = (Node<T>*)nodeStack.Pop();
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index++), node1->Item);
                    for (var node2 = node1->Right; !UnsafeHelpers.IsNull(node2); node2 = node2->Left)
                        nodeStack.Push(node2);
                }
            }

            return count;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this.
        ///     The actual number of copied elements is limited by the span's length, the specified count,
        ///     and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from the this.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (UnsafeHelpers.IsNull(_root))
                return;
            var index = 0;
            using (var nodeStack = new UnsafeStack<NativePtr<Node<T>>>(2 * BitOperationsHelpers.Log2((uint)(_count + 1))))
            {
                for (var node = _root; !UnsafeHelpers.IsNull(node); node = node->Left)
                    nodeStack.Push(node);
                while (nodeStack.Count != 0)
                {
                    var node1 = (Node<T>*)nodeStack.Pop();
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index++), node1->Item);
                    for (var node2 = node1->Right; !UnsafeHelpers.IsNull(node2); node2 = node2->Left)
                        nodeStack.Push(node2);
                }
            }
        }

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static StackallocSortedSet<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Supports a simple iteration over a generic collection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<T>, IDisposable
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly StackallocSortedSet<T>* _handle;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Stack used for in-order traversal of the tree during enumeration.
            /// </summary>
            private readonly NativeStack<NativePtr<Node<T>>> _nodeStack;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private Node<T>* _currentNode;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private T _current;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(StackallocSortedSet<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _nodeStack = new NativeStack<NativePtr<Node<T>>>(2 * BitOperationsHelpers.Log2((uint)(handle->_count + 1)));
                _currentNode = null;
                _current = default;
                var node = handle->_root;
                while (!UnsafeHelpers.IsNull(node))
                {
                    var next = node->Left;
                    _nodeStack.Push(node);
                    node = next;
                }
            }

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, _handle->_version);
                if (!_nodeStack.TryPop(out var result))
                {
                    _currentNode = null;
                    _current = default;
                    return false;
                }

                _currentNode = result;
                _current = _currentNode->Item;
                var node = _currentNode->Right;
                while (!UnsafeHelpers.IsNull(node))
                {
                    var next = node->Left;
                    _nodeStack.Push(node);
                    node = next;
                }

                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _nodeStack.Clear();
                _currentNode = null;
                _current = default;
                var node = _handle->_root;
                while (!UnsafeHelpers.IsNull(node))
                {
                    var next = node->Left;
                    _nodeStack.Push(node);
                    node = next;
                }
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Dispose() => _nodeStack.Dispose();
        }
    }
}