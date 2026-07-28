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
    ///     Stackalloc sorted dictionary
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocSortedDictionary<TKey, TValue> : IIsCreated, IEquatable<StackallocSortedDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Root
        /// </summary>
        private Node<TKey, TValue>* _root;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Node pool
        /// </summary>
        private StackallocFixedSizeStackMemoryPool<Node<TKey, TValue>> _nodePool;

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public KeyCollection Keys => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public ValueCollection Values => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _nodePool.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the minimum value in this.
        /// </summary>
        /// <returns>The minimum value in the set.</returns>
        public readonly KeyValuePair<TKey, TValue>? Min
        {
            get
            {
                if (UnsafeHelpers.IsNull(_root))
                    return default;
                var current = _root;
                while (!UnsafeHelpers.IsNull(current->Left))
                    current = current->Left;
                return new KeyValuePair<TKey, TValue>(current->Key, current->Value);
            }
        }

        /// <summary>
        ///     Gets the maximum value in this.
        /// </summary>
        /// <returns>The maximum value in the set.</returns>
        public readonly KeyValuePair<TKey, TValue>? Max
        {
            get
            {
                if (UnsafeHelpers.IsNull(_root))
                    return default;
                var current = _root;
                while (!UnsafeHelpers.IsNull(current->Right))
                    current = current->Right;
                return new KeyValuePair<TKey, TValue>(current->Key, current->Value);
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
        public static int GetByteCount(int capacity) => StackallocFixedSizeStackMemoryPool<Node<TKey, TValue>>.GetByteCount(capacity);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocSortedDictionary([MustBePinned] Span<byte> buffer, int capacity)
        {
            var nodePool = new StackallocFixedSizeStackMemoryPool<Node<TKey, TValue>>(buffer, capacity);
            _root = null;
            _count = 0;
            _version = 0;
            _nodePool = nodePool;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(StackallocSortedDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is StackallocSortedDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("StackallocSortedDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(StackallocSortedDictionary<TKey, TValue> left, StackallocSortedDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(StackallocSortedDictionary<TKey, TValue> left, StackallocSortedDictionary<TKey, TValue> right) => !left.Equals(right);

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
        ///     Adds the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(in TKey key, in TValue value)
        {
            if (UnsafeHelpers.IsNull(_root))
            {
                if (!_nodePool.TryRent(out _root))
                    return InsertResult.InsufficientCapacity;
                _root->Key = key;
                _root->Value = value;
                _root->Left = null;
                _root->Right = null;
                _root->Color = NodeColor.Black;
                _count = 1;
                _version++;
                return InsertResult.Success;
            }

            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* greatGrandParent = null;
            _version++;
            var order = 0;
            while (!UnsafeHelpers.IsNull(current))
            {
                order = key.CompareTo(current->Key);
                if (order == 0)
                {
                    _root->ColorBlack();
                    return InsertResult.AlreadyExists;
                }

                if (current->Is4Node)
                {
                    current->Split4Node();
                    if (Node<TKey, TValue>.IsNonNullRed(parent))
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
            return InsertResult.Success;
        }

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already
        ///     exist, or updates a key/value pair in this if the key
        ///     already exists.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAddOrUpdate(in TKey key, in TValue value)
        {
            var node = FindNode(key);
            if (UnsafeHelpers.IsNull(node))
                return TryAdd(key, value);
            node->Value = value;
            _version++;
            return InsertResult.Overwritten;
        }

        /// <summary>
        ///     Removes the value with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and removed;
        ///     otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="key" /> is
        ///     not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key)
        {
            if (UnsafeHelpers.IsNull(_root))
                return false;
            _version++;
            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* match = null;
            Node<TKey, TValue>* parentOfMatch = null;
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
        ///     Removes the value with the specified key from this,
        ///     and copies the element to the <paramref name="value" /> parameter.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">The removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and removed;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key, out TValue value)
        {
            if (UnsafeHelpers.IsNull(_root))
            {
                value = default;
                return false;
            }

            _version++;
            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* match = null;
            Node<TKey, TValue>* parentOfMatch = null;
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

            if (!UnsafeHelpers.IsNull(match))
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

            if (!UnsafeHelpers.IsNull(_root))
                _root->ColorBlack();
            return foundMatch;
        }

        /// <summary>
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsKey(in TKey key) => !UnsafeHelpers.IsNull(FindNode(key));

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(in TKey key, out TValue value)
        {
            var node = FindNode(key);
            if (!UnsafeHelpers.IsNull(node))
            {
                value = node->Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(in TKey key, out NativePtr<TValue> value)
        {
            var node = FindNode(key);
            if (!UnsafeHelpers.IsNull(node))
            {
                value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref node->Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" /> in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" /> is in use.
        ///     The ref null can be detected using System.Runtime.CompilerServices.Unsafe.IsNullRef
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueRefOrNullRef(in TKey key)
        {
            var node = FindNode(key);
            return ref !UnsafeHelpers.IsNull(node) ? ref node->Value : ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" /> in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="exists">Whether or not a new entry for the given key was added to this.</param>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" /> is in use.
        ///     The ref null can be detected using System.Runtime.CompilerServices.Unsafe.IsNullRef
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueRefOrNullRef(in TKey key, out bool exists)
        {
            var node = FindNode(key);
            if (!UnsafeHelpers.IsNull(node))
            {
                exists = true;
                return ref node->Value;
            }

            exists = false;
            return ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" /> in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" /> is in use.
        ///     The ref null can be detected using System.Runtime.CompilerServices.Unsafe.IsNullRef
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueRefOrAddDefault(in TKey key, out NativePtr<TValue> value)
        {
            if (UnsafeHelpers.IsNull(_root))
            {
                if (!_nodePool.TryRent(out _root))
                {
                    value = default;
                    return false;
                }

                _root->Key = key;
                _root->Value = default;
                _root->Left = null;
                _root->Right = null;
                _root->Color = NodeColor.Black;
                _count = 1;
                _version++;
                value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref _root->Value));
                return true;
            }

            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* greatGrandParent = null;
            _version++;
            var order = 0;
            while (!UnsafeHelpers.IsNull(current))
            {
                order = key.CompareTo(current->Key);
                if (order == 0)
                {
                    _root->ColorBlack();
                    value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref current->Value));
                    return true;
                }

                if (current->Is4Node)
                {
                    current->Split4Node();
                    if (Node<TKey, TValue>.IsNonNullRed(parent))
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
                value = default;
                return false;
            }

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
            value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref node->Value));
            return true;
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" /> in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <param name="exists">Whether or not a new entry for the given key was added to this.</param>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" /> is in use.
        ///     The ref null can be detected using System.Runtime.CompilerServices.Unsafe.IsNullRef
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueRefOrAddDefault(in TKey key, out NativePtr<TValue> value, out bool exists)
        {
            if (UnsafeHelpers.IsNull(_root))
            {
                if (!_nodePool.TryRent(out _root))
                {
                    value = default;
                    exists = false;
                    return false;
                }

                _root->Key = key;
                _root->Value = default;
                _root->Left = null;
                _root->Right = null;
                _root->Color = NodeColor.Black;
                _count = 1;
                _version++;
                value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref _root->Value));
                exists = false;
                return true;
            }

            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* greatGrandParent = null;
            _version++;
            var order = 0;
            while (!UnsafeHelpers.IsNull(current))
            {
                order = key.CompareTo(current->Key);
                if (order == 0)
                {
                    _root->ColorBlack();
                    value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref current->Value));
                    exists = true;
                    return true;
                }

                if (current->Is4Node)
                {
                    current->Split4Node();
                    if (Node<TKey, TValue>.IsNonNullRed(parent))
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
                value = default;
                exists = false;
                return false;
            }

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
            value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref node->Value));
            exists = false;
            return true;
        }

        /// <summary>
        ///     Insertion balance
        /// </summary>
        /// <param name="current">Current</param>
        /// <param name="parent">Parent</param>
        /// <param name="grandParent">Grand parent</param>
        /// <param name="greatGrandParent">GreatGrand parent</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InsertionBalance(Node<TKey, TValue>* current, Node<TKey, TValue>* parent, Node<TKey, TValue>* grandParent, Node<TKey, TValue>* greatGrandParent)
        {
            var parentIsOnRight = grandParent->Right == parent;
            var currentIsOnRight = parent->Right == current;
            Node<TKey, TValue>* newChildOfGreatGrandParent;
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
        private void ReplaceChildOrRoot(Node<TKey, TValue>* parent, Node<TKey, TValue>* child, Node<TKey, TValue>* newChild)
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
        private void ReplaceNode(Node<TKey, TValue>* match, Node<TKey, TValue>* parentOfMatch, Node<TKey, TValue>* successor, Node<TKey, TValue>* parentOfSuccessor)
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
        /// <param name="key">Key</param>
        /// <returns>Node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Node<TKey, TValue>* FindNode(in TKey key)
        {
            var current = _root;
            while (!UnsafeHelpers.IsNull(current))
            {
                var order = key.CompareTo(current->Key);
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
        public readonly int CopyTo(Span<KeyValuePair<TKey, TValue>> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (UnsafeHelpers.IsNull(_root))
                return 0;
            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var index = 0;
            using (var nodeStack = new UnsafeStack<NativePtr<Node<TKey, TValue>>>(2 * BitOperationsHelpers.Log2((uint)(_count + 1))))
            {
                for (var node = _root; !UnsafeHelpers.IsNull(node); node = node->Left)
                    nodeStack.Push(node);
                while (nodeStack.Count != 0)
                {
                    if (index >= count)
                        break;
                    var node1 = (Node<TKey, TValue>*)nodeStack.Pop();
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index++), new KeyValuePair<TKey, TValue>(node1->Key, node1->Value));
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
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<KeyValuePair<TKey, TValue>> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (UnsafeHelpers.IsNull(_root))
                return;
            var index = 0;
            using (var nodeStack = new UnsafeStack<NativePtr<Node<TKey, TValue>>>(2 * BitOperationsHelpers.Log2((uint)(_count + 1))))
            {
                for (var node = _root; !UnsafeHelpers.IsNull(node); node = node->Left)
                    nodeStack.Push(node);
                while (nodeStack.Count != 0)
                {
                    var node1 = (Node<TKey, TValue>*)nodeStack.Pop();
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index++), new KeyValuePair<TKey, TValue>(node1->Key, node1->Value));
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
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer));

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocSortedDictionary<TKey, TValue> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
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
        public struct Enumerator : IIterator<KeyValuePair<TKey, TValue>>, IDisposable
        {
            /// <summary>
            ///     NativeHashSet
            /// </summary>
            private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Node stack
            /// </summary>
            private readonly NativeStack<NativePtr<Node<TKey, TValue>>> _nodeStack;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private Node<TKey, TValue>* _currentNode;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private KeyValuePair<TKey, TValue> _current;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(StackallocSortedDictionary<TKey, TValue>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _nodeStack = new NativeStack<NativePtr<Node<TKey, TValue>>>(2 * BitOperationsHelpers.Log2((uint)(handle->_count + 1)));
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
                _current = new KeyValuePair<TKey, TValue>(_currentNode->Key, _currentNode->Value);
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
            public readonly KeyValuePair<TKey, TValue> Current
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

        /// <summary>
        ///     Represents the collection of keys.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection : IIsCreated, IReadOnlyCollection<TKey>
        {
            /// <summary>
            ///     NativeSortedDictionary
            /// </summary>
            private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(StackallocSortedDictionary<TKey, TValue>* handle) => _handle = handle;

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
            public int CopyTo(Span<TKey> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (UnsafeHelpers.IsNull(_handle->_root))
                    return 0;
                count = Math.Min(buffer.Length, Math.Min(count, _handle->_count));
                var index = 0;
                using (var nodeStack = new UnsafeStack<NativePtr<Node<TKey, TValue>>>(2 * BitOperationsHelpers.Log2((uint)(_handle->_count + 1))))
                {
                    for (var node = _handle->_root; !UnsafeHelpers.IsNull(node); node = node->Left)
                        nodeStack.Push(node);
                    while (nodeStack.Count != 0)
                    {
                        if (index >= count)
                            break;
                        var node1 = (Node<TKey, TValue>*)nodeStack.Pop();
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index++), node1->Key);
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
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span.
            ///     The span must have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<TKey> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (UnsafeHelpers.IsNull(_handle->_root))
                    return;
                var index = 0;
                using (var nodeStack = new UnsafeStack<NativePtr<Node<TKey, TValue>>>(2 * BitOperationsHelpers.Log2((uint)(_handle->_count + 1))))
                {
                    for (var node = _handle->_root; !UnsafeHelpers.IsNull(node); node = node->Left)
                        nodeStack.Push(node);
                    while (nodeStack.Count != 0)
                    {
                        var node1 = (Node<TKey, TValue>*)nodeStack.Pop();
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index++), node1->Key);
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
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer));

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            public Enumerator GetEnumerator() => new(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TKey>, IDisposable
            {
                /// <summary>
                ///     NativeHashSet
                /// </summary>
                private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

                /// <summary>
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Node stack
                /// </summary>
                private readonly NativeStack<NativePtr<Node<TKey, TValue>>> _nodeStack;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                private Node<TKey, TValue>* _currentNode;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                private TKey _current;

                /// <summary>
                ///     Structure
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(StackallocSortedDictionary<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _nodeStack = new NativeStack<NativePtr<Node<TKey, TValue>>>(2 * BitOperationsHelpers.Log2((uint)(handle->_count + 1)));
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
                    _current = _currentNode->Key;
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
                public readonly TKey Current
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

        /// <summary>
        ///     Represents the collection of values.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ValueCollection : IIsCreated, IReadOnlyCollection<TValue>
        {
            /// <summary>
            ///     NativeSortedDictionary
            /// </summary>
            private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(StackallocSortedDictionary<TKey, TValue>* handle) => _handle = handle;

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
            public int CopyTo(Span<TValue> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (UnsafeHelpers.IsNull(_handle->_root))
                    return 0;
                count = Math.Min(buffer.Length, Math.Min(count, _handle->_count));
                var index = 0;
                using (var nodeStack = new UnsafeStack<NativePtr<Node<TKey, TValue>>>(2 * BitOperationsHelpers.Log2((uint)(_handle->_count + 1))))
                {
                    for (var node = _handle->_root; !UnsafeHelpers.IsNull(node); node = node->Left)
                        nodeStack.Push(node);
                    while (nodeStack.Count != 0)
                    {
                        if (index >= count)
                            break;
                        var node1 = (Node<TKey, TValue>*)nodeStack.Pop();
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index++), node1->Value);
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
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span.
            ///     The span must have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<TValue> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (UnsafeHelpers.IsNull(_handle->_root))
                    return;
                var index = 0;
                using (var nodeStack = new UnsafeStack<NativePtr<Node<TKey, TValue>>>(2 * BitOperationsHelpers.Log2((uint)(_handle->_count + 1))))
                {
                    for (var node = _handle->_root; !UnsafeHelpers.IsNull(node); node = node->Left)
                        nodeStack.Push(node);
                    while (nodeStack.Count != 0)
                    {
                        var node1 = (Node<TKey, TValue>*)nodeStack.Pop();
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index++), node1->Value);
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
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer));

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            public Enumerator GetEnumerator() => new(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TValue>, IDisposable
            {
                /// <summary>
                ///     NativeHashSet
                /// </summary>
                private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

                /// <summary>
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Node stack
                /// </summary>
                private readonly NativeStack<NativePtr<Node<TKey, TValue>>> _nodeStack;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                private Node<TKey, TValue>* _currentNode;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                private TValue _current;

                /// <summary>
                ///     Structure
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(StackallocSortedDictionary<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _nodeStack = new NativeStack<NativePtr<Node<TKey, TValue>>>(2 * BitOperationsHelpers.Log2((uint)(handle->_count + 1)));
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
                    _current = _currentNode->Value;
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
                public readonly TValue Current
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
}