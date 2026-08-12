using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides internal node structures
    ///     and helper utilities for ordered set collections
    ///     based on red‑black tree algorithms.
    /// </summary>
    internal static unsafe class NativeSortedSet
    {
        /// <summary>
        ///     Represents the color of a node in a red‑black tree.
        /// </summary>
        public enum NodeColor : byte
        {
            /// <summary>
            ///     Black node.
            /// </summary>
            Black,

            /// <summary>
            ///     Red node.
            /// </summary>
            Red
        }

        /// <summary>
        ///     Represents the type of tree rotation to perform.
        /// </summary>
        public enum TreeRotation : byte
        {
            /// <summary>
            ///     Single left rotation.
            /// </summary>
            Left,

            /// <summary>
            ///     Double rotation: left then right.
            /// </summary>
            LeftRight,

            /// <summary>
            ///     Single right rotation.
            /// </summary>
            Right,

            /// <summary>
            ///     Double rotation: right then left.
            /// </summary>
            RightLeft
        }

        /// <summary>
        ///     Represents a node in a red‑black tree
        ///     that stores a single value of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the stored value,
        ///     which must implement <see cref="IComparable{T}" />.
        /// </typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public struct Node<T> where T : unmanaged, IComparable<T>
        {
            /// <summary>
            ///     Determines whether the given node is non‑null and red.
            /// </summary>
            /// <param name="node">The node to check.</param>
            /// <returns>
            ///     <see langword="true" /> if the node is non‑null and red;
            ///     otherwise, <see langword="false" />.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsNonNullRed(Node<T>* node) => !UnsafeHelpers.IsNull(node) && node->IsRed;

            /// <summary>
            ///     Determines whether the given node is null or black.
            /// </summary>
            /// <param name="node">The node to check.</param>
            /// <returns>
            ///     <see langword="true" /> if the node is null or black;
            ///     otherwise, <see langword="false" />.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsNullOrBlack(Node<T>* node) => UnsafeHelpers.IsNull(node) || node->IsBlack;

            /// <summary>
            ///     The value stored in the node.
            /// </summary>
            public T Item;

            /// <summary>
            ///     Pointer to the left child.
            /// </summary>
            public Node<T>* Left;

            /// <summary>
            ///     Pointer to the right child.
            /// </summary>
            public Node<T>* Right;

            /// <summary>
            ///     The color of the node.
            /// </summary>
            public NodeColor Color;

            /// <summary>
            ///     Gets a value indicating whether the node is black.
            /// </summary>
            private readonly bool IsBlack
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Color == NodeColor.Black;
            }

            /// <summary>
            ///     Gets a value indicating whether the node is red.
            /// </summary>
            public readonly bool IsRed
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Color == NodeColor.Red;
            }

            /// <summary>
            ///     Gets a value indicating whether the node is a 2‑node (black with black children).
            /// </summary>
            public readonly bool Is2Node
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => IsBlack && IsNullOrBlack(Left) && IsNullOrBlack(Right);
            }

            /// <summary>
            ///     Gets a value indicating whether the node is a 4‑node (black with two red children).
            /// </summary>
            public readonly bool Is4Node
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => IsNonNullRed(Left) && IsNonNullRed(Right);
            }

            /// <summary>
            ///     Sets the node color to black.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ColorBlack() => Color = NodeColor.Black;

            /// <summary>
            ///     Sets the node color to red.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ColorRed() => Color = NodeColor.Red;

            /// <summary>
            ///     Determines the rotation needed
            ///     to balance the tree after an insertion or deletion.
            /// </summary>
            /// <param name="current">The child node that is currently unbalanced.</param>
            /// <param name="sibling">The sibling of the current node.</param>
            /// <returns>The appropriate <see cref="TreeRotation" />.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly TreeRotation GetRotation(Node<T>* current, Node<T>* sibling)
            {
                var currentIsLeftChild = Left == current;
                return IsNonNullRed(sibling->Left) ? currentIsLeftChild ? TreeRotation.RightLeft : TreeRotation.Right : currentIsLeftChild ? TreeRotation.Left : TreeRotation.LeftRight;
            }

            /// <summary>
            ///     Returns the sibling of the specified child node.
            /// </summary>
            /// <param name="node">The child node whose sibling is desired.</param>
            /// <returns>A pointer to the sibling node.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly Node<T>* GetSibling(Node<T>* node) => node == Left ? Right : Left;

            /// <summary>
            ///     Splits a 4‑node (black with two red children) by recoloring.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Split4Node()
            {
                ColorRed();
                Left->ColorBlack();
                Right->ColorBlack();
            }

            /// <summary>
            ///     Performs a tree rotation of the specified type
            ///     and returns the new subtree root.
            /// </summary>
            /// <param name="rotation">The type of rotation to perform.</param>
            /// <returns>The new root of the rotated subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<T>* Rotate(TreeRotation rotation)
            {
                Node<T>* removeRed;
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
            ///     Performs a left rotation.
            /// </summary>
            /// <returns>The new root of the subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<T>* RotateLeft()
            {
                var child = Right;
                Right = child->Left;
                child->Left = UnsafeHelpers.AsPointer(ref this);
                return child;
            }

            /// <summary>
            ///     Performs a left‑right rotation.
            /// </summary>
            /// <returns>The new root of the subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<T>* RotateLeftRight()
            {
                var child = Left;
                var grandChild = child->Right;
                Left = grandChild->Right;
                grandChild->Right = UnsafeHelpers.AsPointer(ref this);
                child->Right = grandChild->Left;
                grandChild->Left = child;
                return grandChild;
            }

            /// <summary>
            ///     Performs a right rotation.
            /// </summary>
            /// <returns>The new root of the subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<T>* RotateRight()
            {
                var child = Left;
                Left = child->Right;
                child->Right = UnsafeHelpers.AsPointer(ref this);
                return child;
            }

            /// <summary>
            ///     Performs a right‑left rotation.
            /// </summary>
            /// <returns>The new root of the subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<T>* RotateRightLeft()
            {
                var child = Right;
                var grandChild = child->Left;
                Right = grandChild->Left;
                grandChild->Left = UnsafeHelpers.AsPointer(ref this);
                child->Left = grandChild->Right;
                grandChild->Right = child;
                return grandChild;
            }

            /// <summary>
            ///     Merges two black children into a 2‑node by recoloring.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Merge2Nodes()
            {
                ColorBlack();
                Left->ColorRed();
                Right->ColorRed();
            }

            /// <summary>
            ///     Replaces a child pointer with a new node.
            /// </summary>
            /// <param name="child">The old child pointer to replace.</param>
            /// <param name="newChild">The new child pointer.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ReplaceChild(Node<T>* child, Node<T>* newChild)
            {
                if (Left == child)
                    Left = newChild;
                else
                    Right = newChild;
            }
        }

        /// <summary>
        ///     Represents a node in a red‑black tree that stores a key‑value pair.
        /// </summary>
        /// <typeparam name="TKey">
        ///     The type of the key,
        ///     which must implement <see cref="IComparable{TKey}" />.
        /// </typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public struct Node<TKey, TValue> where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Determines whether the given node is non‑null and red.
            /// </summary>
            /// <param name="node">The node to check.</param>
            /// <returns>
            ///     <see langword="true" /> if the node is non‑null and red;
            ///     otherwise, <see langword="false" />.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsNonNullRed(Node<TKey, TValue>* node) => !UnsafeHelpers.IsNull(node) && node->IsRed;

            /// <summary>
            ///     Determines whether the given node is null or black.
            /// </summary>
            /// <param name="node">The node to check.</param>
            /// <returns>
            ///     <see langword="true" /> if the node is null or black;
            ///     otherwise, <see langword="false" />.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsNullOrBlack(Node<TKey, TValue>* node) => UnsafeHelpers.IsNull(node) || node->IsBlack;

            /// <summary>
            ///     The key stored in the node.
            /// </summary>
            public TKey Key;

            /// <summary>
            ///     The value associated with the key.
            /// </summary>
            public TValue Value;

            /// <summary>
            ///     Pointer to the left child.
            /// </summary>
            public Node<TKey, TValue>* Left;

            /// <summary>
            ///     Pointer to the right child.
            /// </summary>
            public Node<TKey, TValue>* Right;

            /// <summary>
            ///     The color of the node.
            /// </summary>
            public NodeColor Color;

            /// <summary>
            ///     Gets a value indicating whether the node is black.
            /// </summary>
            private readonly bool IsBlack
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Color == NodeColor.Black;
            }

            /// <summary>
            ///     Gets a value indicating whether the node is red.
            /// </summary>
            public readonly bool IsRed
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Color == NodeColor.Red;
            }

            /// <summary>
            ///     Gets a value indicating whether the node is a 2‑node (black with black children).
            /// </summary>
            public readonly bool Is2Node
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => IsBlack && IsNullOrBlack(Left) && IsNullOrBlack(Right);
            }

            /// <summary>
            ///     Gets a value indicating whether the node is a 4‑node (black with two red children).
            /// </summary>
            public readonly bool Is4Node
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => IsNonNullRed(Left) && IsNonNullRed(Right);
            }

            /// <summary>
            ///     Sets the node color to black.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ColorBlack() => Color = NodeColor.Black;

            /// <summary>
            ///     Sets the node color to red.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ColorRed() => Color = NodeColor.Red;

            /// <summary>
            ///     Determines the rotation needed
            ///     to balance the tree after an insertion or deletion.
            /// </summary>
            /// <param name="current">The child node that is currently unbalanced.</param>
            /// <param name="sibling">The sibling of the current node.</param>
            /// <returns>The appropriate <see cref="TreeRotation" />.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly TreeRotation GetRotation(Node<TKey, TValue>* current, Node<TKey, TValue>* sibling)
            {
                var currentIsLeftChild = Left == current;
                return IsNonNullRed(sibling->Left) ? currentIsLeftChild ? TreeRotation.RightLeft : TreeRotation.Right : currentIsLeftChild ? TreeRotation.Left : TreeRotation.LeftRight;
            }

            /// <summary>
            ///     Returns the sibling of the specified child node.
            /// </summary>
            /// <param name="node">The child node whose sibling is desired.</param>
            /// <returns>A pointer to the sibling node.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly Node<TKey, TValue>* GetSibling(Node<TKey, TValue>* node) => node == Left ? Right : Left;

            /// <summary>
            ///     Splits a 4‑node (black with two red children) by recoloring.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Split4Node()
            {
                ColorRed();
                Left->ColorBlack();
                Right->ColorBlack();
            }

            /// <summary>
            ///     Performs a tree rotation of the specified type
            ///     and returns the new subtree root.
            /// </summary>
            /// <param name="rotation">The type of rotation to perform.</param>
            /// <returns>The new root of the rotated subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<TKey, TValue>* Rotate(TreeRotation rotation)
            {
                Node<TKey, TValue>* removeRed;
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
            ///     Performs a left rotation.
            /// </summary>
            /// <returns>The new root of the subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<TKey, TValue>* RotateLeft()
            {
                var child = Right;
                Right = child->Left;
                child->Left = UnsafeHelpers.AsPointer(ref this);
                return child;
            }

            /// <summary>
            ///     Performs a left‑right rotation.
            /// </summary>
            /// <returns>The new root of the subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<TKey, TValue>* RotateLeftRight()
            {
                var child = Left;
                var grandChild = child->Right;
                Left = grandChild->Right;
                grandChild->Right = UnsafeHelpers.AsPointer(ref this);
                child->Right = grandChild->Left;
                grandChild->Left = child;
                return grandChild;
            }

            /// <summary>
            ///     Performs a right rotation.
            /// </summary>
            /// <returns>The new root of the subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<TKey, TValue>* RotateRight()
            {
                var child = Left;
                Left = child->Right;
                child->Right = UnsafeHelpers.AsPointer(ref this);
                return child;
            }

            /// <summary>
            ///     Performs a right‑left rotation.
            /// </summary>
            /// <returns>The new root of the subtree.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Node<TKey, TValue>* RotateRightLeft()
            {
                var child = Right;
                var grandChild = child->Left;
                Right = grandChild->Left;
                grandChild->Left = UnsafeHelpers.AsPointer(ref this);
                child->Left = grandChild->Right;
                grandChild->Right = child;
                return grandChild;
            }

            /// <summary>
            ///     Merges two black children into a 2‑node by recoloring.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Merge2Nodes()
            {
                ColorBlack();
                Left->ColorRed();
                Right->ColorRed();
            }

            /// <summary>
            ///     Replaces a child pointer with a new node.
            /// </summary>
            /// <param name="child">The old child pointer to replace.</param>
            /// <param name="newChild">The new child pointer.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ReplaceChild(Node<TKey, TValue>* child, Node<TKey, TValue>* newChild)
            {
                if (Left == child)
                    Left = newChild;
                else
                    Right = newChild;
            }
        }
    }
}