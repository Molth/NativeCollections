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
    ///     Represents a collection of key/value pairs that are sorted on the key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocSortedDictionary<TKey, TValue> : IIsCreated, IEquatable<StackallocSortedDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     The root node.
        /// </summary>
        private Node<TKey, TValue>* _root;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Represents a memory pool that provides reusable fixed-size memory blocks.
        /// </summary>
        private StackallocFixedSizeStackMemoryPool<Node<TKey, TValue>> _nodePool;

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public KeyCollection Keys => KeyCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public ValueCollection Values => ValueCollection.From(UnsafeHelpers.AsPointer(ref this));

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
        ///     Gets the entry with the smallest key in this.
        /// </summary>
        /// <value>The entry with the smallest key in this, or <see langword="null" /> if this is empty.</value>
        public readonly KeyValuePair<TKey, TValue>? Min
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
        ///     Gets the entry with the largest key in this.
        /// </summary>
        /// <value>The entry with the largest key in this, or <see langword="null" /> if this is empty.</value>
        public readonly KeyValuePair<TKey, TValue>? Max
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
        ///     Calculates the minimum number of bytes required to store a specified number of
        ///     elements, taking into account alignment requirements for the underlying buffer.
        /// </summary>
        /// <param name="capacity">The number of elements to store. Must be non-negative.</param>
        /// <returns>
        ///     The minimum byte count needed to allocate a buffer capable of
        ///     holding <paramref name="capacity" /> elements with proper alignment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity) => StackallocFixedSizeStackMemoryPool<Node<TKey, TValue>>.GetByteCount(capacity);

        /// <summary>
        ///     Initializes a new instance of this class that uses a caller-provided byte buffer as storage.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage. It must be large enough
        ///     to store the specified number of elements with proper alignment.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the dictionary can hold. Must be non-negative.
        /// </param>
        /// <returns>
        ///     A sorted dictionary that uses <paramref name="buffer" /> as its
        ///     storage and holds up to <paramref name="capacity" /> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative or <paramref name="buffer" />
        ///     is too small to hold the required number of elements (including alignment padding).
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StackallocSortedDictionary<TKey, TValue> FromBytes([MustBePinned] Span<byte> buffer, int capacity) => new(buffer, capacity);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the dictionary can hold.
        /// </param>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        private StackallocSortedDictionary([MustBePinned] Span<byte> buffer, int capacity)
        {
            var nodePool = StackallocFixedSizeStackMemoryPool<Node<TKey, TValue>>.FromBytes(buffer, capacity);
            _root = null;
            _count = 0;
            _version = 0;
            _nodePool = nodePool;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(StackallocSortedDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is StackallocSortedDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("StackallocSortedDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(StackallocSortedDictionary<TKey, TValue> left, StackallocSortedDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
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
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Adds the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the key/value pair was added to this;
        ///     <see cref="InsertResult.AlreadyExists" /> if the key is already present;
        ///     <see cref="InsertResult.InsufficientCapacity" /> if this has no available capacity.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(TKey key, TValue value)
        {
            if (_root == null)
            {
                if (!_nodePool.TryRent(out _root))
                    return InsertResult.InsufficientCapacity;

                _root->Key = key;
                _root->Value = value;
                _root->Left = null;
                _root->Right = null;
                _root->Color = NodeColor.Black;
                _count = 1;
                _version = _version.WrappingAdd(1);
                return InsertResult.Success;
            }

            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* greatGrandParent = null;
            _version = _version.WrappingAdd(1);
            var order = 0;
            while (current != null)
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
            _count = _count.WrappingAdd(1);
            return InsertResult.Success;
        }

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist,
        ///     or updates a key/value pair in this if the key already exists.
        /// </summary>
        /// <param name="key">The key of the element to add or update.</param>
        /// <param name="value">The value of the element to add or update.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the key/value pair was added to this;
        ///     <see cref="InsertResult.Overwritten" /> if an existing value was replaced;
        ///     <see cref="InsertResult.InsufficientCapacity" /> if this has no available capacity for a new key.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAddOrUpdate(TKey key, TValue value)
        {
            var node = FindNode(key);
            if (node == null)
                return TryAdd(key, value);

            node->Value = value;
            _version = _version.WrappingAdd(1);
            return InsertResult.Overwritten;
        }

        /// <summary>
        ///     Removes the value with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and
        ///     removed; otherwise, <see langword="false" />. This method returns
        ///     <see langword="false" /> if <paramref name="key" /> is not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            if (_root == null)
                return false;

            _version = _version.WrappingAdd(1);
            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* match = null;
            Node<TKey, TValue>* parentOfMatch = null;
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
                _count = _count.WrappingSub(1);
                _nodePool.Return(match);
            }

            if (_root != null)
                _root->ColorBlack();

            return foundMatch;
        }

        /// <summary>
        ///     Removes the value with the specified key from this, and
        ///     copies the element to the <paramref name="value" /> parameter.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">The removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully
        ///     found and removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key, out TValue value)
        {
            if (_root == null)
            {
                value = default;
                return false;
            }

            _version = _version.WrappingAdd(1);
            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* match = null;
            Node<TKey, TValue>* parentOfMatch = null;
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
                _count = _count.WrappingSub(1);
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
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsKey(TKey key) => FindNode(key) != null;

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified
        ///     key, if the key is found; otherwise, the default value for the type of the
        ///     <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(TKey key, out TValue value)
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
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified
        ///     key, if the key is found; otherwise, the default value for the type of the
        ///     <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(TKey key, out NativePtr<TValue> value)
        {
            var node = FindNode(key);
            if (node != null)
            {
                value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref node->Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" />
        ///     in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
        ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueRefOrNullRef(TKey key)
        {
            var node = FindNode(key);
            return ref node != null ? ref node->Value : ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" />
        ///     in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="exists">Whether or not a new entry for the given key was added to this.</param>
        /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
        ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueRefOrNullRef(TKey key, out bool exists)
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
        ///     Gets a pointer to the value associated with the specified key, adding
        ///     a new entry with a default value if the key does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="value">
        ///     When this method returns, contains a pointer to the value associated with the
        ///     specified key if the operation succeeds; otherwise, the default value for the type
        ///     of the <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the value was found or a new
        ///     entry was added; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Items should not be added to or removed from this while the returned pointer is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueRefOrAddDefault(TKey key, out NativePtr<TValue> value)
        {
            if (_root == null)
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
                _version = _version.WrappingAdd(1);
                value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref _root->Value));
                return true;
            }

            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* greatGrandParent = null;
            _version = _version.WrappingAdd(1);
            var order = 0;
            while (current != null)
            {
                order = key.CompareTo(current->Key);
                if (order == 0)
                {
                    _root->ColorBlack();
                    value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref current->Value));
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
            _count = _count.WrappingAdd(1);
            value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref node->Value));
            return true;
        }

        /// <summary>
        ///     Gets a pointer to the value associated with the specified key, adding
        ///     a new entry with a default value if the key does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="value">
        ///     When this method returns, contains a pointer to the value associated with the
        ///     specified key if the operation succeeds; otherwise, the default value for the type
        ///     of the <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <param name="exists">
        ///     When this method returns, indicates whether the specified key was
        ///     already present in this. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the value was found or a new
        ///     entry was added; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Items should not be added to or removed from this while the returned pointer is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueRefOrAddDefault(TKey key, out NativePtr<TValue> value, out bool exists)
        {
            if (_root == null)
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
                _version = _version.WrappingAdd(1);
                value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref _root->Value));
                exists = false;
                return true;
            }

            var current = _root;
            Node<TKey, TValue>* parent = null;
            Node<TKey, TValue>* grandParent = null;
            Node<TKey, TValue>* greatGrandParent = null;
            _version = _version.WrappingAdd(1);
            var order = 0;
            while (current != null)
            {
                order = key.CompareTo(current->Key);
                if (order == 0)
                {
                    _root->ColorBlack();
                    value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref current->Value));
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
            _count = _count.WrappingAdd(1);
            value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref node->Value));
            exists = false;
            return true;
        }

        /// <summary>
        ///     Restores red‑black tree balance after insertion by performing rotations and recoloring.
        /// </summary>
        /// <param name="current">The newly inserted red node.</param>
        /// <param name="parent">The parent of <paramref name="current" />.</param>
        /// <param name="grandParent">The grandparent of <paramref name="current" />.</param>
        /// <param name="greatGrandParent">
        ///     The great‑grandparent of <paramref name="current" />, used to update the tree root.
        /// </param>
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
        ///     Replaces a child node of a given parent with a new child, or
        ///     updates the root if the parent is <see langword="null" />.
        /// </summary>
        /// <param name="parent">
        ///     The parent node whose child is being replaced. May be <see langword="null" />.
        /// </param>
        /// <param name="child">The child node to replace.</param>
        /// <param name="newChild">The new child node.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReplaceChildOrRoot(Node<TKey, TValue>* parent, Node<TKey, TValue>* child, Node<TKey, TValue>* newChild)
        {
            if (parent != null)
                parent->ReplaceChild(child, newChild);
            else
                _root = newChild;
        }

        /// <summary>
        ///     Replaces a matched node with its successor during deletion, adjusting the tree structure accordingly.
        /// </summary>
        /// <param name="match">The node to be removed.</param>
        /// <param name="parentOfMatch">The parent of <paramref name="match" />.</param>
        /// <param name="successor">The successor node that will replace <paramref name="match" />.</param>
        /// <param name="parentOfSuccessor">The parent of <paramref name="successor" />.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReplaceNode(Node<TKey, TValue>* match, Node<TKey, TValue>* parentOfMatch, Node<TKey, TValue>* successor, Node<TKey, TValue>* parentOfSuccessor)
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
        ///     Searches for a node with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        ///     A node if found; otherwise, <see langword="null" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Node<TKey, TValue>* FindNode(TKey key)
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
        ///     Copies up to the specified number of elements from this. The actual number of copied elements is
        ///     limited by the span's length, the specified count, and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        /// <remarks>
        ///     This method temporarily allocates native memory to hold the traversal stack and frees it
        ///     before returning. The allocation is observable for the duration of the call, which is a
        ///     deviation from the no-allocation contract of the Stackalloc flavor.
        /// </remarks>
        public readonly int CopyTo(Span<KeyValuePair<TKey, TValue>> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (_root == null)
                return 0;

            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var index = 0;
            using (var nodeStack = UnsafeStack<NativePtr<Node<TKey, TValue>>>.Create(2.WrappingMul(BitOperationsHelpers.Log2(U32.FromI32Unchecked(_count.WrappingAdd(1))))))
            {
                for (var node = _root; node != null; node = node->Left)
                    nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));

                while (nodeStack.Count != 0)
                {
                    if (index >= count)
                        break;

                    var node1 = nodeStack.Pop().Handle;
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((index = index.WrappingAdd(1)).WrappingSub(1))), new KeyValuePair<TKey, TValue>(node1->Key, node1->Value));
                    for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                        nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node2));
                }
            }

            return count;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this. The actual number of copied elements is
        ///     limited by the span's length, the specified count, and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span. The span must
        ///     have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
        /// </exception>
        /// <remarks>
        ///     This method temporarily allocates native memory to hold the traversal stack and frees it
        ///     before returning. The allocation is observable for the duration of the call, which is a
        ///     deviation from the no-allocation contract of the Stackalloc flavor.
        /// </remarks>
        public readonly void CopyTo(Span<KeyValuePair<TKey, TValue>> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (_root == null)
                return;

            var index = 0;
            using (var nodeStack = UnsafeStack<NativePtr<Node<TKey, TValue>>>.Create(2.WrappingMul(BitOperationsHelpers.Log2(U32.FromI32Unchecked(_count.WrappingAdd(1))))))
            {
                for (var node = _root; node != null; node = node->Left)
                    nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));

                while (nodeStack.Count != 0)
                {
                    var node1 = nodeStack.Pop().Handle;
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((index = index.WrappingAdd(1)).WrappingSub(1))), new KeyValuePair<TKey, TValue>(node1->Key, node1->Value));
                    for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                        nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node2));
                }
            }
        }

        /// <summary>
        ///     Copies all elements from this into a destination span. The span must
        ///     have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => Enumerator.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
        public struct Enumerator : IIterator<KeyValuePair<TKey, TValue>>, IDisposable
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

            /// <summary>
            ///     Used to keep the enumerator synchronized with the collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Stack used for in-order traversal of the tree during enumeration.
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
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            private Enumerator(StackallocSortedDictionary<TKey, TValue>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _nodeStack = NativeStack<NativePtr<Node<TKey, TValue>>>.Create(2.WrappingMul(BitOperationsHelpers.Log2(U32.FromI32Unchecked(handle->_count.WrappingAdd(1)))));
                _currentNode = null;
                _current = default;
                var node = handle->_root;
                while (node != null)
                {
                    var next = node->Left;
                    _nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));
                    node = next;
                }
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(StackallocSortedDictionary<TKey, TValue>* handle) => new(handle);

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            ///     Thrown when the collection was modified after the enumerator was created.
            /// </exception>
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

                _currentNode = result.Handle;
                _current = new KeyValuePair<TKey, TValue>(_currentNode->Key, _currentNode->Value);
                var node = _currentNode->Right;
                while (node != null)
                {
                    var next = node->Left;
                    _nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));
                    node = next;
                }

                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _nodeStack.Clear();
                _currentNode = null;
                _current = default;
                var node = _handle->_root;
                while (node != null)
                {
                    var next = node->Left;
                    _nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));
                    node = next;
                }
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public readonly void Dispose() => _nodeStack.Dispose();
        }

        /// <summary>
        ///     Represents the collection of keys.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection : IIsCreated, IReadOnlyCollection<TKey>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private KeyCollection(StackallocSortedDictionary<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static KeyCollection From(StackallocSortedDictionary<TKey, TValue>* handle) => new(handle);

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            /// <remarks>
            ///     This method temporarily allocates native memory to hold the traversal stack and frees it
            ///     before returning. The allocation is observable for the duration of the call, which is a
            ///     deviation from the no-allocation contract of the Stackalloc flavor.
            /// </remarks>
            public int CopyTo(Span<TKey> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (_handle->_root == null)
                    return 0;

                count = Math.Min(buffer.Length, Math.Min(count, _handle->_count));
                var index = 0;
                using (var nodeStack = UnsafeStack<NativePtr<Node<TKey, TValue>>>.Create(2.WrappingMul(BitOperationsHelpers.Log2(U32.FromI32Unchecked(_handle->_count.WrappingAdd(1))))))
                {
                    for (var node = _handle->_root; node != null; node = node->Left)
                        nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));

                    while (nodeStack.Count != 0)
                    {
                        if (index >= count)
                            break;

                        var node1 = nodeStack.Pop().Handle;
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((index = index.WrappingAdd(1)).WrappingSub(1))), node1->Key);
                        for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                            nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node2));
                    }
                }

                return count;
            }

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            /// <remarks>
            ///     This method temporarily allocates native memory to hold the traversal stack and frees it
            ///     before returning. The allocation is observable for the duration of the call, which is a
            ///     deviation from the no-allocation contract of the Stackalloc flavor.
            /// </remarks>
            public void CopyTo(Span<TKey> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (_handle->_root == null)
                    return;

                var index = 0;
                using (var nodeStack = UnsafeStack<NativePtr<Node<TKey, TValue>>>.Create(2.WrappingMul(BitOperationsHelpers.Log2(U32.FromI32Unchecked(_handle->_count.WrappingAdd(1))))))
                {
                    for (var node = _handle->_root; node != null; node = node->Left)
                        nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));

                    while (nodeStack.Count != 0)
                    {
                        var node1 = nodeStack.Pop().Handle;
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((index = index.WrappingAdd(1)).WrappingSub(1))), node1->Key);
                        for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                            nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node2));
                    }
                }
            }

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer));

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            public Enumerator GetEnumerator() => Enumerator.From(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Supports a simple iteration over a generic collection.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TKey>, IDisposable
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Stack used for in-order traversal of the tree during enumeration.
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
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                private Enumerator(StackallocSortedDictionary<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _nodeStack = NativeStack<NativePtr<Node<TKey, TValue>>>.Create(2.WrappingMul(BitOperationsHelpers.Log2(U32.FromI32Unchecked(handle->_count.WrappingAdd(1)))));
                    _currentNode = null;
                    _current = default;
                    var node = handle->_root;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));
                        node = next;
                    }
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(StackallocSortedDictionary<TKey, TValue>* handle) => new(handle);

                /// <summary>
                ///     Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns>
                ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
                ///     <see langword="false" /> if the enumerator has passed the end of the collection.
                /// </returns>
                /// <exception cref="InvalidOperationException">
                ///     Thrown when the collection was modified after the enumerator was created.
                /// </exception>
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

                    _currentNode = result.Handle;
                    _current = _currentNode->Key;
                    var node = _currentNode->Right;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));
                        node = next;
                    }

                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    _nodeStack.Clear();
                    _currentNode = null;
                    _current = default;
                    var node = _handle->_root;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));
                        node = next;
                    }
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly TKey Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }

                /// <summary>
                ///     Performs application-defined tasks associated with
                ///     freeing, releasing, or resetting unmanaged resources.
                /// </summary>
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
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ValueCollection(StackallocSortedDictionary<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ValueCollection From(StackallocSortedDictionary<TKey, TValue>* handle) => new(handle);

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            /// <remarks>
            ///     This method temporarily allocates native memory to hold the traversal stack and frees it
            ///     before returning. The allocation is observable for the duration of the call, which is a
            ///     deviation from the no-allocation contract of the Stackalloc flavor.
            /// </remarks>
            public int CopyTo(Span<TValue> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (_handle->_root == null)
                    return 0;

                count = Math.Min(buffer.Length, Math.Min(count, _handle->_count));
                var index = 0;
                using (var nodeStack = UnsafeStack<NativePtr<Node<TKey, TValue>>>.Create(2.WrappingMul(BitOperationsHelpers.Log2(U32.FromI32Unchecked(_handle->_count.WrappingAdd(1))))))
                {
                    for (var node = _handle->_root; node != null; node = node->Left)
                        nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));

                    while (nodeStack.Count != 0)
                    {
                        if (index >= count)
                            break;

                        var node1 = nodeStack.Pop().Handle;
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((index = index.WrappingAdd(1)).WrappingSub(1))), node1->Value);
                        for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                            nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node2));
                    }
                }

                return count;
            }

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            /// <remarks>
            ///     This method temporarily allocates native memory to hold the traversal stack and frees it
            ///     before returning. The allocation is observable for the duration of the call, which is a
            ///     deviation from the no-allocation contract of the Stackalloc flavor.
            /// </remarks>
            public void CopyTo(Span<TValue> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (_handle->_root == null)
                    return;

                var index = 0;
                using (var nodeStack = UnsafeStack<NativePtr<Node<TKey, TValue>>>.Create(2.WrappingMul(BitOperationsHelpers.Log2(U32.FromI32Unchecked(_handle->_count.WrappingAdd(1))))))
                {
                    for (var node = _handle->_root; node != null; node = node->Left)
                        nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));

                    while (nodeStack.Count != 0)
                    {
                        var node1 = nodeStack.Pop().Handle;
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((index = index.WrappingAdd(1)).WrappingSub(1))), node1->Value);
                        for (var node2 = node1->Right; node2 != null; node2 = node2->Left)
                            nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node2));
                    }
                }
            }

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer));

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            public Enumerator GetEnumerator() => Enumerator.From(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Supports a simple iteration over a generic collection.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TValue>, IDisposable
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly StackallocSortedDictionary<TKey, TValue>* _handle;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Stack used for in-order traversal of the tree during enumeration.
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
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                private Enumerator(StackallocSortedDictionary<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _nodeStack = NativeStack<NativePtr<Node<TKey, TValue>>>.Create(2.WrappingMul(BitOperationsHelpers.Log2(U32.FromI32Unchecked(handle->_count.WrappingAdd(1)))));
                    _currentNode = null;
                    _current = default;
                    var node = handle->_root;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));
                        node = next;
                    }
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(StackallocSortedDictionary<TKey, TValue>* handle) => new(handle);

                /// <summary>
                ///     Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns>
                ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
                ///     <see langword="false" /> if the enumerator has passed the end of the collection.
                /// </returns>
                /// <exception cref="InvalidOperationException">
                ///     Thrown when the collection was modified after the enumerator was created.
                /// </exception>
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

                    _currentNode = result.Handle;
                    _current = _currentNode->Value;
                    var node = _currentNode->Right;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));
                        node = next;
                    }

                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    _nodeStack.Clear();
                    _currentNode = null;
                    _current = default;
                    var node = _handle->_root;
                    while (node != null)
                    {
                        var next = node->Left;
                        _nodeStack.Push(NativePtr<Node<TKey, TValue>>.From(node));
                        node = next;
                    }
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }

                /// <summary>
                ///     Performs application-defined tasks associated with
                ///     freeing, releasing, or resetting unmanaged resources.
                /// </summary>
                public readonly void Dispose() => _nodeStack.Dispose();
            }
        }
    }
}