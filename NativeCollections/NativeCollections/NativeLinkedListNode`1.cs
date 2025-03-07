using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native linked list node
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeLinkedListNode<T> where T : unmanaged
    {
        /// <summary>
        ///     Node list
        /// </summary>
        public NativeLinkedListNode NodeList;

        /// <summary>
        ///     Item
        /// </summary>
        public T Item;

        /// <summary>
        ///     Next
        /// </summary>
        public NativeLinkedListNode<T>* Next => (NativeLinkedListNode<T>*)((NativeLinkedListNode*)Unsafe.AsPointer(ref this))->Next;

        /// <summary>
        ///     Previous
        /// </summary>
        public NativeLinkedListNode<T>* Previous => (NativeLinkedListNode<T>*)((NativeLinkedListNode*)Unsafe.AsPointer(ref this))->Previous;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException($"Cannot call Equals on NativeLinkedListNode<{typeof(T).Name}>");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => throw new NotSupportedException($"Cannot call GetHashCode on NativeLinkedListNode<{typeof(T).Name}>");

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeLinkedListNode<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeLinkedListNode<T> left, NativeLinkedListNode<T> right) => throw new NotSupportedException($"Cannot call Equals on NativeLinkedListNode<{typeof(T).Name}>");

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeLinkedListNode<T> left, NativeLinkedListNode<T> right) => throw new NotSupportedException($"Cannot call Not Equals on NativeLinkedListNode<{typeof(T).Name}>");

        /// <summary>
        ///     Insert before
        /// </summary>
        /// <param name="newNode">New node</param>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode<T>* InsertBefore(NativeLinkedListNode<T>* newNode)
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
            return (NativeLinkedListNode<T>*)node->InsertBefore((NativeLinkedListNode*)newNode);
        }

        /// <summary>
        ///     Insert after
        /// </summary>
        /// <param name="newNode">New node</param>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode<T>* InsertAfter(NativeLinkedListNode<T>* newNode)
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
            return (NativeLinkedListNode<T>*)node->InsertAfter((NativeLinkedListNode*)newNode);
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode<T>* Remove()
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
            return (NativeLinkedListNode<T>*)node->Remove();
        }

        /// <summary>
        ///     Move before
        /// </summary>
        /// <param name="first">First</param>
        /// <param name="last">Last</param>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode<T>* MoveBefore(NativeLinkedListNode<T>* first, NativeLinkedListNode<T>* last)
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
            return (NativeLinkedListNode<T>*)node->MoveBefore((NativeLinkedListNode*)first, (NativeLinkedListNode*)last);
        }

        /// <summary>
        ///     Move after
        /// </summary>
        /// <param name="first">First</param>
        /// <param name="last">Last</param>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode<T>* MoveAfter(NativeLinkedListNode<T>* first, NativeLinkedListNode<T>* last)
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
            return (NativeLinkedListNode<T>*)node->MoveAfter((NativeLinkedListNode*)first, (NativeLinkedListNode*)last);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeLinkedListNode<T> Empty => new();
    }
}