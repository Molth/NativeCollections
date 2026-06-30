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
    ///     Unsafe sortedSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeSortedSet<T> : IIsCreated, IDisposable, IEquatable<UnsafeSortedSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IComparable<T>
    {
        /// <summary>
        ///     Root
        /// </summary>
        private Node<T>* _root;

        /// <summary>
        ///     Count
        /// </summary>
        private int _count;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Node pool
        /// </summary>
        private UnsafeMemoryPool<Node<T>> _nodePool;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _nodePool.IsCreated;

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Min
        /// </summary>
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
        ///     Max
        /// </summary>
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
        ///     Structure
        /// </summary>
        /// <param name="size">MemoryPool size</param>
        /// <param name="maxFreeSlabs">MemoryPool maxFreeSlabs</param>
        public UnsafeSortedSet(int size, int maxFreeSlabs)
        {
            var nodePool = new UnsafeMemoryPool<Node<T>>(size, maxFreeSlabs);
            _root = null;
            _count = 0;
            _version = 0;
            _nodePool = nodePool;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeSortedSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeSortedSet<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => SR.Format("UnsafeSortedSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeSortedSet<T> left, UnsafeSortedSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeSortedSet<T> left, UnsafeSortedSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _nodePool.Dispose();

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _nodePool.Clear();
            _root = null;
            _count = 0;
            ++_version;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(in T item)
        {
            if (UnsafeHelpers.IsNull(_root))
            {
                _root = _nodePool.Rent();
                _root->Item = item;
                _root->Left = null;
                _root->Right = null;
                _root->Color = NodeColor.Black;
                _count = 1;
                _version++;
                return true;
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
                    return false;
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

            var node = _nodePool.Rent();
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
            return true;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T equalValue, in T actualValue)
        {
            var node = FindNode(equalValue);
            if (UnsafeHelpers.IsNull(node))
            {
                Add(actualValue);
            }
            else
            {
                node->Item = actualValue;
                _version++;
            }
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Removed</returns>
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
        ///     Remove
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Removed</returns>
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
        ///     Contains
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in T item) => !UnsafeHelpers.IsNull(FindNode(item));

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
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
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(in T equalValue, out NativeReference<T> actualValue)
        {
            var node = FindNode(equalValue);
            if (!UnsafeHelpers.IsNull(node))
            {
                actualValue = new NativeReference<T>(UnsafeHelpers.AsPointer(ref node->Item));
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Insertion balance
        /// </summary>
        /// <param name="current">Current</param>
        /// <param name="parent">Parent</param>
        /// <param name="grandParent">Grand parent</param>
        /// <param name="greatGrandParent">GreatGrand parent</param>
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
        ///     Replace child or root
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="child">Child</param>
        /// <param name="newChild">New child</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReplaceChildOrRoot(Node<T>* parent, Node<T>* child, Node<T>* newChild)
        {
            if (!UnsafeHelpers.IsNull(parent))
                parent->ReplaceChild(child, newChild);
            else
                _root = newChild;
        }

        /// <summary>
        ///     Replace node
        /// </summary>
        /// <param name="match">Match</param>
        /// <param name="parentOfMatch">Parent of match</param>
        /// <param name="successor">Successor</param>
        /// <param name="parentOfSuccessor">Parent of successor</param>
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
        ///     Find node
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Node</returns>
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
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (UnsafeHelpers.IsNull(_root))
                return 0;
            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var index = 0;
            using (var nodeStack = new UnsafeStack<NativeReference<Node<T>>>(2 * BitOperationsHelpers.Log2((uint)(_count + 1))))
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
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (UnsafeHelpers.IsNull(_root))
                return;
            var index = 0;
            using (var nodeStack = new UnsafeStack<NativeReference<Node<T>>>(2 * BitOperationsHelpers.Log2((uint)(_count + 1))))
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
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeSortedSet<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<T>, IDisposable
        {
            /// <summary>
            ///     NativeHashSet
            /// </summary>
            private readonly UnsafeSortedSet<T>* _handle;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Node stack
            /// </summary>
            private readonly NativeStack<NativeReference<Node<T>>> _nodeStack;

            /// <summary>
            ///     Current
            /// </summary>
            private Node<T>* _currentNode;

            /// <summary>
            ///     Current
            /// </summary>
            private T _current;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeSortedSet<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _nodeStack = new NativeStack<NativeReference<Node<T>>>(2 * BitOperationsHelpers.Log2((uint)(handle->_count + 1)));
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
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
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
            ///     Reset
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
            ///     Current
            /// </summary>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Dispose() => _nodeStack.Dispose();
        }
    }
}