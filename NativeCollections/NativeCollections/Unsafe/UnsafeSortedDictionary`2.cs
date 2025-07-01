using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe sorted dictionary
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeSortedDictionary<TKey, TValue> : IDisposable, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Root
        /// </summary>
        private Node* _root;

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
        private UnsafeMemoryPool _nodePool;

        /// <summary>
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _count;

        /// <summary>
        ///     Min
        /// </summary>
        public KeyValuePair<TKey, TValue>? Min
        {
            get
            {
                if (_root == null)
                    return default;
                var current = _root;
                while (current->Left != null)
                    current = current->Left;
                return new KeyValuePair<TKey, TValue>(current->Key, current->Value);
            }
        }

        /// <summary>
        ///     Max
        /// </summary>
        public KeyValuePair<TKey, TValue>? Max
        {
            get
            {
                if (_root == null)
                    return default;
                var current = _root;
                while (current->Right != null)
                    current = current->Right;
                return new KeyValuePair<TKey, TValue>(current->Key, current->Value);
            }
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="key">Key</param>
        public TValue this[in TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!TryGetValue(key, out var value))
                    ThrowHelpers.ThrowKeyNotFoundException(key);
                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var node = FindNode(key);
                if (node == null)
                {
                    Add(key, value);
                }
                else
                {
                    node->Value = value;
                    _version++;
                }
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">MemoryPool size</param>
        /// <param name="maxFreeSlabs">MemoryPool maxFreeSlabs</param>
        public UnsafeSortedDictionary(int size, int maxFreeSlabs)
        {
            var nodePool = new UnsafeMemoryPool(size, sizeof(Node), maxFreeSlabs, (int)NativeMemoryAllocator.AlignOf<Node>());
            _root = null;
            _count = 0;
            _version = 0;
            _nodePool = nodePool;
        }

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
            if (_root != null)
            {
                using (var nodeStack = new UnsafeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(_count + 1))))
                {
                    nodeStack.Push((nint)_root);
                    while (nodeStack.TryPop(out var node))
                    {
                        var currentNode = (Node*)node;
                        if (currentNode->Left != null)
                            nodeStack.Push((nint)currentNode->Left);
                        if (currentNode->Right != null)
                            nodeStack.Push((nint)currentNode->Right);
                        _nodePool.Return(currentNode);
                    }
                }
            }

            _root = null;
            _count = 0;
            ++_version;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(in TKey key, in TValue value)
        {
            if (_root == null)
            {
                _root = (Node*)_nodePool.Rent();
                _root->Key = key;
                _root->Value = value;
                _root->Left = null;
                _root->Right = null;
                _root->Color = NodeColor.Black;
                _count = 1;
                _version++;
                return true;
            }

            var current = _root;
            Node* parent = null;
            Node* grandParent = null;
            Node* greatGrandParent = null;
            _version++;
            var order = 0;
            while (current != null)
            {
                order = key.CompareTo(current->Key);
                if (order == 0)
                {
                    _root->ColorBlack();
                    return false;
                }

                if (current->Is4Node)
                {
                    current->Split4Node();
                    if (Node.IsNonNullRed(parent))
                        InsertionBalance(current, parent, grandParent, greatGrandParent);
                }

                greatGrandParent = grandParent;
                grandParent = parent;
                parent = current;
                current = order < 0 ? current->Left : current->Right;
            }

            var node = (Node*)_nodePool.Rent();
            node->Key = key;
            node->Value = value;
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
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key)
        {
            if (_root == null)
                return false;
            _version++;
            var current = _root;
            Node* parent = null;
            Node* grandParent = null;
            Node* match = null;
            Node* parentOfMatch = null;
            var foundMatch = false;
            while (current != null)
            {
                if (current->Is2Node)
                {
                    if (parent == null)
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

                var order = foundMatch ? -1 : key.CompareTo(current->Key);
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

            if (match != null)
            {
                ReplaceNode(match, parentOfMatch, parent, grandParent);
                --_count;
                _nodePool.Return(match);
            }

            if (_root != null)
                _root->ColorBlack();
            return foundMatch;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key, out TValue value)
        {
            if (_root == null)
            {
                value = default;
                return false;
            }

            _version++;
            var current = _root;
            Node* parent = null;
            Node* grandParent = null;
            Node* match = null;
            Node* parentOfMatch = null;
            var foundMatch = false;
            while (current != null)
            {
                if (current->Is2Node)
                {
                    if (parent == null)
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

                var order = foundMatch ? -1 : key.CompareTo(current->Key);
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

            if (match != null)
            {
                value = match->Value;
                ReplaceNode(match, parentOfMatch, parent, grandParent);
                --_count;
                _nodePool.Return(match);
            }
            else
            {
                value = default;
            }

            if (_root != null)
                _root->ColorBlack();
            return foundMatch;
        }

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => FindNode(key) != null;

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value)
        {
            var node = FindNode(key);
            if (node != null)
            {
                value = node->Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(in TKey key, out NativeReference<TValue> value)
        {
            var node = FindNode(key);
            if (node != null)
            {
                value = new NativeReference<TValue>(Unsafe.AsPointer(ref node->Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Get value ref
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrNullRef(in TKey key)
        {
            var node = FindNode(key);
            return ref node != null ? ref node->Value : ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Get value ref
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="exists">Exists</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrNullRef(in TKey key, out bool exists)
        {
            var node = FindNode(key);
            if (node != null)
            {
                exists = true;
                return ref node->Value;
            }

            exists = false;
            return ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Get value ref or add default
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrAddDefault(in TKey key)
        {
            if (_root == null)
            {
                _root = (Node*)_nodePool.Rent();
                _root->Key = key;
                _root->Value = default;
                _root->Left = null;
                _root->Right = null;
                _root->Color = NodeColor.Black;
                _count = 1;
                _version++;
                return ref _root->Value;
            }

            var current = _root;
            Node* parent = null;
            Node* grandParent = null;
            Node* greatGrandParent = null;
            _version++;
            var order = 0;
            while (current != null)
            {
                order = key.CompareTo(current->Key);
                if (order == 0)
                {
                    _root->ColorBlack();
                    return ref current->Value;
                }

                if (current->Is4Node)
                {
                    current->Split4Node();
                    if (Node.IsNonNullRed(parent))
                        InsertionBalance(current, parent, grandParent, greatGrandParent);
                }

                greatGrandParent = grandParent;
                grandParent = parent;
                parent = current;
                current = order < 0 ? current->Left : current->Right;
            }

            var node = (Node*)_nodePool.Rent();
            node->Key = key;
            node->Value = default;
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
            return ref node->Value;
        }

        /// <summary>
        ///     Get value ref or add default
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="exists">Exists</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrAddDefault(in TKey key, out bool exists)
        {
            if (_root == null)
            {
                _root = (Node*)_nodePool.Rent();
                _root->Key = key;
                _root->Value = default;
                _root->Left = null;
                _root->Right = null;
                _root->Color = NodeColor.Black;
                _count = 1;
                _version++;
                exists = false;
                return ref _root->Value;
            }

            var current = _root;
            Node* parent = null;
            Node* grandParent = null;
            Node* greatGrandParent = null;
            _version++;
            var order = 0;
            while (current != null)
            {
                order = key.CompareTo(current->Key);
                if (order == 0)
                {
                    _root->ColorBlack();
                    exists = true;
                    return ref current->Value;
                }

                if (current->Is4Node)
                {
                    current->Split4Node();
                    if (Node.IsNonNullRed(parent))
                        InsertionBalance(current, parent, grandParent, greatGrandParent);
                }

                greatGrandParent = grandParent;
                grandParent = parent;
                parent = current;
                current = order < 0 ? current->Left : current->Right;
            }

            var node = (Node*)_nodePool.Rent();
            node->Key = key;
            node->Value = default;
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
            exists = false;
            return ref node->Value;
        }

        /// <summary>
        ///     Insertion balance
        /// </summary>
        /// <param name="current">Current</param>
        /// <param name="parent">Parent</param>
        /// <param name="grandParent">Grand parent</param>
        /// <param name="greatGrandParent">GreatGrand parent</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InsertionBalance(Node* current, Node* parent, Node* grandParent, Node* greatGrandParent)
        {
            var parentIsOnRight = grandParent->Right == parent;
            var currentIsOnRight = parent->Right == current;
            Node* newChildOfGreatGrandParent;
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
        private void ReplaceChildOrRoot(Node* parent, Node* child, Node* newChild)
        {
            if (parent != null)
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
        private void ReplaceNode(Node* match, Node* parentOfMatch, Node* successor, Node* parentOfSuccessor)
        {
            if (successor == match)
            {
                successor = match->Left;
            }
            else
            {
                if (successor->Right != null)
                    successor->Right->ColorBlack();
                if (parentOfSuccessor != match)
                {
                    parentOfSuccessor->Left = successor->Right;
                    successor->Right = match->Right;
                }

                successor->Left = match->Left;
            }

            if (successor != null)
                successor->Color = match->Color;
            ReplaceChildOrRoot(parentOfMatch, match, successor);
        }

        /// <summary>
        ///     Find node
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Node* FindNode(in TKey key)
        {
            var current = _root;
            while (current != null)
            {
                var order = key.CompareTo(current->Key);
                if (order == 0)
                    return current;
                current = order < 0 ? current->Left : current->Right;
            }

            return null;
        }

        /// <summary>
        ///     Node
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Node
        {
            /// <summary>
            ///     Is non null red
            /// </summary>
            /// <param name="node">Node</param>
            /// <returns>Is non null red</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsNonNullRed(Node* node) => node != null && node->IsRed;

            /// <summary>
            ///     Is null or black
            /// </summary>
            /// <param name="node">Node</param>
            /// <returns>Is null or black</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsNullOrBlack(Node* node) => node == null || node->IsBlack;

            /// <summary>
            ///     Key
            /// </summary>
            public TKey Key;

            /// <summary>
            ///     Value
            /// </summary>
            public TValue Value;

            /// <summary>
            ///     Left
            /// </summary>
            public Node* Left;

            /// <summary>
            ///     Right
            /// </summary>
            public Node* Right;

            /// <summary>
            ///     Color
            /// </summary>
            public NodeColor Color;

            /// <summary>
            ///     Is black
            /// </summary>
            private bool IsBlack
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Color == NodeColor.Black;
            }

            /// <summary>
            ///     Is red
            /// </summary>
            public bool IsRed
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Color == NodeColor.Red;
            }

            /// <summary>
            ///     Is 2 node
            /// </summary>
            public bool Is2Node
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => IsBlack && IsNullOrBlack(Left) && IsNullOrBlack(Right);
            }

            /// <summary>
            ///     Is 4 node
            /// </summary>
            public bool Is4Node
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => IsNonNullRed(Left) && IsNonNullRed(Right);
            }

            /// <summary>
            ///     Set color to black
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ColorBlack() => Color = NodeColor.Black;

            /// <summary>
            ///     Set color to red
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ColorRed() => Color = NodeColor.Red;

            /// <summary>
            ///     Get rotation
            /// </summary>
            /// <param name="current">Current</param>
            /// <param name="sibling">Sibling</param>
            /// <returns>Rotation</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TreeRotation GetRotation(Node* current, Node* sibling)
            {
                var currentIsLeftChild = Left == current;
                return IsNonNullRed(sibling->Left) ? currentIsLeftChild ? TreeRotation.RightLeft : TreeRotation.Right : currentIsLeftChild ? TreeRotation.Left : TreeRotation.LeftRight;
            }

            /// <summary>
            ///     Get sibling
            /// </summary>
            /// <param name="node">Node</param>
            /// <returns>Sibling</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node* GetSibling(Node* node) => node == Left ? Right : Left;

            /// <summary>
            ///     Split 4 node
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Split4Node()
            {
                ColorRed();
                Left->ColorBlack();
                Right->ColorBlack();
            }

            /// <summary>
            ///     Rotate
            /// </summary>
            /// <param name="rotation">Rotation</param>
            /// <returns>Node</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node* Rotate(TreeRotation rotation)
            {
                Node* removeRed;
                switch (rotation)
                {
                    case TreeRotation.Right:
                        removeRed = Left->Left;
                        removeRed->ColorBlack();
                        return RotateRight();
                    case TreeRotation.Left:
                        removeRed = Right->Right;
                        removeRed->ColorBlack();
                        return RotateLeft();
                    case TreeRotation.RightLeft:
                        return RotateRightLeft();
                    case TreeRotation.LeftRight:
                        return RotateLeftRight();
                    default:
                        return null;
                }
            }

            /// <summary>
            ///     Rotate left
            /// </summary>
            /// <returns>Node</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node* RotateLeft()
            {
                var child = Right;
                Right = child->Left;
                child->Left = (Node*)Unsafe.AsPointer(ref this);
                return child;
            }

            /// <summary>
            ///     Rotate left right
            /// </summary>
            /// <returns>Node</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node* RotateLeftRight()
            {
                var child = Left;
                var grandChild = child->Right;
                Left = grandChild->Right;
                grandChild->Right = (Node*)Unsafe.AsPointer(ref this);
                child->Right = grandChild->Left;
                grandChild->Left = child;
                return grandChild;
            }

            /// <summary>
            ///     Rotate right
            /// </summary>
            /// <returns>Node</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node* RotateRight()
            {
                var child = Left;
                Left = child->Right;
                child->Right = (Node*)Unsafe.AsPointer(ref this);
                return child;
            }

            /// <summary>
            ///     Rotate right left
            /// </summary>
            /// <returns>Node</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node* RotateRightLeft()
            {
                var child = Right;
                var grandChild = child->Left;
                Right = grandChild->Left;
                grandChild->Left = (Node*)Unsafe.AsPointer(ref this);
                child->Left = grandChild->Right;
                grandChild->Right = child;
                return grandChild;
            }

            /// <summary>
            ///     Merge 2 nodes
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Merge2Nodes()
            {
                ColorBlack();
                Left->ColorRed();
                Right->ColorRed();
            }

            /// <summary>
            ///     Replace child
            /// </summary>
            /// <param name="child">Child</param>
            /// <param name="newChild">New child</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ReplaceChild(Node* child, Node* newChild)
            {
                if (Left == child)
                    Left = newChild;
                else
                    Right = newChild;
            }
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CopyTo(Span<KeyValuePair<TKey, TValue>> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, nameof(count));
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (_root == null)
                return 0;
            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var index = 0;
            using (var nodeStack = new UnsafeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(_count + 1))))
            {
                for (var node = _root; node != null; node = node->Left)
                    nodeStack.Push((nint)node);
                while (nodeStack.Count != 0)
                {
                    if (index >= count)
                        break;
                    var node1 = (Node*)nodeStack.Pop();
                    Unsafe.WriteUnaligned(ref Unsafe.As<KeyValuePair<TKey, TValue>, byte>(ref Unsafe.Add(ref reference, index++)), new KeyValuePair<TKey, TValue>(node1->Key, node1->Value));
                    for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                        nodeStack.Push((nint)node2);
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
        public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer), count);

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<KeyValuePair<TKey, TValue>> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, nameof(buffer));
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (_root == null)
                return;
            var index = 0;
            using (var nodeStack = new UnsafeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(_count + 1))))
            {
                for (var node = _root; node != null; node = node->Left)
                    nodeStack.Push((nint)node);
                while (nodeStack.Count != 0)
                {
                    var node1 = (Node*)nodeStack.Pop();
                    Unsafe.WriteUnaligned(ref Unsafe.As<KeyValuePair<TKey, TValue>, byte>(ref Unsafe.Add(ref reference, (nint)index++)), new KeyValuePair<TKey, TValue>(node1->Key, node1->Value));
                    for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                        nodeStack.Push((nint)node2);
                }
            }
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer));

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeSortedDictionary<TKey, TValue> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator : IDisposable
        {
            /// <summary>
            ///     NativeHashSet
            /// </summary>
            private readonly UnsafeSortedDictionary<TKey, TValue>* _nativeSortedDictionary;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Node stack
            /// </summary>
            private readonly NativeStack<nint> _nodeStack;

            /// <summary>
            ///     Current
            /// </summary>
            private Node* _currentNode;

            /// <summary>
            ///     Current
            /// </summary>
            private KeyValuePair<TKey, TValue> _current;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeSortedDictionary)
            {
                var handle = (UnsafeSortedDictionary<TKey, TValue>*)nativeSortedDictionary;
                _nativeSortedDictionary = handle;
                _version = handle->_version;
                _nodeStack = new NativeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(handle->_count + 1)));
                _currentNode = null;
                _current = default;
                var node = handle->_root;
                while (node != null)
                {
                    var next = node->Left;
                    _nodeStack.Push((nint)node);
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
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, _nativeSortedDictionary->_version);
                if (!_nodeStack.TryPop(out var result))
                {
                    _currentNode = null;
                    _current = default;
                    return false;
                }

                _currentNode = (Node*)result;
                _current = new KeyValuePair<TKey, TValue>(_currentNode->Key, _currentNode->Value);
                var node = _currentNode->Right;
                while (node != null)
                {
                    var next = node->Left;
                    _nodeStack.Push((nint)node);
                    node = next;
                }

                return true;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _nodeStack.Dispose();
        }

        /// <summary>
        ///     Key collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection : IReadOnlyCollection<TKey>
        {
            /// <summary>
            ///     NativeSortedDictionary
            /// </summary>
            private readonly UnsafeSortedDictionary<TKey, TValue>* _nativeSortedDictionary;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSortedDictionary->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeSortedDictionary) => _nativeSortedDictionary = (UnsafeSortedDictionary<TKey, TValue>*)nativeSortedDictionary;

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<TKey> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, nameof(count));
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (_nativeSortedDictionary->_root == null)
                    return 0;
                count = Math.Min(buffer.Length, Math.Min(count, _nativeSortedDictionary->_count));
                var index = 0;
                using (var nodeStack = new UnsafeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(_nativeSortedDictionary->_count + 1))))
                {
                    for (var node = _nativeSortedDictionary->_root; node != null; node = node->Left)
                        nodeStack.Push((nint)node);
                    while (nodeStack.Count != 0)
                    {
                        if (index >= count)
                            break;
                        var node1 = (Node*)nodeStack.Pop();
                        Unsafe.WriteUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref reference, index++)), node1->Key);
                        for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                            nodeStack.Push((nint)node2);
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
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer), count);

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<TKey> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, nameof(buffer));
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (_nativeSortedDictionary->_root == null)
                    return;
                var index = 0;
                using (var nodeStack = new UnsafeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(_nativeSortedDictionary->_count + 1))))
                {
                    for (var node = _nativeSortedDictionary->_root; node != null; node = node->Left)
                        nodeStack.Push((nint)node);
                    while (nodeStack.Count != 0)
                    {
                        var node1 = (Node*)nodeStack.Pop();
                        Unsafe.WriteUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref reference, (nint)index++)), node1->Key);
                        for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                            nodeStack.Push((nint)node2);
                    }
                }
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer));

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSortedDictionary);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator : IDisposable
            {
                /// <summary>
                ///     NativeHashSet
                /// </summary>
                private readonly UnsafeSortedDictionary<TKey, TValue>* _nativeSortedDictionary;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Node stack
                /// </summary>
                private readonly NativeStack<nint> _nodeStack;

                /// <summary>
                ///     Current
                /// </summary>
                private Node* _currentNode;

                /// <summary>
                ///     Current
                /// </summary>
                private TKey _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSortedDictionary)
                {
                    var handle = (UnsafeSortedDictionary<TKey, TValue>*)nativeSortedDictionary;
                    _nativeSortedDictionary = handle;
                    _version = handle->_version;
                    _nodeStack = new NativeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(handle->_count + 1)));
                    _currentNode = null;
                    _current = default;
                    var node = handle->_root;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push((nint)node);
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
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, _nativeSortedDictionary->_version);
                    if (!_nodeStack.TryPop(out var result))
                    {
                        _currentNode = null;
                        _current = default;
                        return false;
                    }

                    _currentNode = (Node*)result;
                    _current = _currentNode->Key;
                    var node = _currentNode->Right;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push((nint)node);
                        node = next;
                    }

                    return true;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public TKey Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }

                /// <summary>
                ///     Dispose
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() => _nodeStack.Dispose();
            }
        }

        /// <summary>
        ///     Value collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ValueCollection : IReadOnlyCollection<TValue>
        {
            /// <summary>
            ///     NativeSortedDictionary
            /// </summary>
            private readonly UnsafeSortedDictionary<TKey, TValue>* _nativeSortedDictionary;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeSortedDictionary->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeSortedDictionary) => _nativeSortedDictionary = (UnsafeSortedDictionary<TKey, TValue>*)nativeSortedDictionary;

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<TValue> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, nameof(count));
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (_nativeSortedDictionary->_root == null)
                    return 0;
                count = Math.Min(buffer.Length, Math.Min(count, _nativeSortedDictionary->_count));
                var index = 0;
                using (var nodeStack = new UnsafeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(_nativeSortedDictionary->_count + 1))))
                {
                    for (var node = _nativeSortedDictionary->_root; node != null; node = node->Left)
                        nodeStack.Push((nint)node);
                    while (nodeStack.Count != 0)
                    {
                        if (index >= count)
                            break;
                        var node1 = (Node*)nodeStack.Pop();
                        Unsafe.WriteUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref reference, index++)), node1->Value);
                        for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                            nodeStack.Push((nint)node2);
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
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer), count);

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<TValue> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, nameof(buffer));
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (_nativeSortedDictionary->_root == null)
                    return;
                var index = 0;
                using (var nodeStack = new UnsafeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(_nativeSortedDictionary->_count + 1))))
                {
                    for (var node = _nativeSortedDictionary->_root; node != null; node = node->Left)
                        nodeStack.Push((nint)node);
                    while (nodeStack.Count != 0)
                    {
                        var node1 = (Node*)nodeStack.Pop();
                        Unsafe.WriteUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref reference, (nint)index++)), node1->Value);
                        for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                            nodeStack.Push((nint)node2);
                    }
                }
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer));

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeSortedDictionary);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator : IDisposable
            {
                /// <summary>
                ///     NativeHashSet
                /// </summary>
                private readonly UnsafeSortedDictionary<TKey, TValue>* _nativeSortedDictionary;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Node stack
                /// </summary>
                private readonly NativeStack<nint> _nodeStack;

                /// <summary>
                ///     Current
                /// </summary>
                private Node* _currentNode;

                /// <summary>
                ///     Current
                /// </summary>
                private TValue _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeSortedDictionary">NativeSortedDictionary</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeSortedDictionary)
                {
                    var handle = (UnsafeSortedDictionary<TKey, TValue>*)nativeSortedDictionary;
                    _nativeSortedDictionary = handle;
                    _version = handle->_version;
                    _nodeStack = new NativeStack<nint>(2 * BitOperationsHelpers.Log2((uint)(handle->_count + 1)));
                    _currentNode = null;
                    _current = default;
                    var node = handle->_root;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push((nint)node);
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
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, _nativeSortedDictionary->_version);
                    if (!_nodeStack.TryPop(out var result))
                    {
                        _currentNode = null;
                        _current = default;
                        return false;
                    }

                    _currentNode = (Node*)result;
                    _current = _currentNode->Value;
                    var node = _currentNode->Right;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push((nint)node);
                        node = next;
                    }

                    return true;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }

                /// <summary>
                ///     Dispose
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose() => _nodeStack.Dispose();
            }
        }

        /// <summary>
        ///     Node color
        /// </summary>
        private enum NodeColor : byte
        {
            Black,
            Red
        }

        /// <summary>
        ///     Tree rotation
        /// </summary>
        private enum TreeRotation : byte
        {
            Left,
            LeftRight,
            Right,
            RightLeft
        }
    }
}