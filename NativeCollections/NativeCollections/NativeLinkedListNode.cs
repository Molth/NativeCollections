using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native linked list node
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public unsafe struct NativeLinkedListNode
    {
        /// <summary>
        ///     Next
        /// </summary>
        public NativeLinkedListNode* Next;

        /// <summary>
        ///     Previous
        /// </summary>
        public NativeLinkedListNode* Previous;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("Cannot call Equals on NativeLinkedListNode");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => throw new NotSupportedException("Cannot call GetHashCode on NativeLinkedListNode");

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeLinkedListNode";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeLinkedListNode left, NativeLinkedListNode right) => throw new NotSupportedException("Cannot call Equals on NativeLinkedListNode");

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeLinkedListNode left, NativeLinkedListNode right) => throw new NotSupportedException("Cannot call Not Equals on NativeLinkedListNode");

        /// <summary>
        ///     Insert before
        /// </summary>
        /// <param name="newNode">New node</param>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode* InsertBefore(NativeLinkedListNode* newNode)
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
            newNode->Previous = Previous;
            newNode->Next = node;
            newNode->Previous->Next = newNode;
            Previous = newNode;
            return node;
        }

        /// <summary>
        ///     Insert after
        /// </summary>
        /// <param name="newNode">New node</param>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode* InsertAfter(NativeLinkedListNode* newNode)
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
            newNode->Next = Next;
            newNode->Previous = node;
            newNode->Next->Previous = newNode;
            Next = newNode;
            return node;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode* Remove()
        {
            Previous->Next = Next;
            Next->Previous = Previous;
            return (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
        }

        /// <summary>
        ///     Move before
        /// </summary>
        /// <param name="first">First</param>
        /// <param name="last">Last</param>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode* MoveBefore(NativeLinkedListNode* first, NativeLinkedListNode* last)
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
            first->Previous->Next = last->Next;
            last->Next->Previous = first->Previous;
            first->Previous = Previous;
            last->Next = node;
            first->Previous->Next = first;
            Previous = last;
            return node;
        }

        /// <summary>
        ///     Move after
        /// </summary>
        /// <param name="first">First</param>
        /// <param name="last">Last</param>
        /// <returns>Current</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedListNode* MoveAfter(NativeLinkedListNode* first, NativeLinkedListNode* last)
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref this);
            first->Previous->Next = last->Next;
            last->Next->Previous = first->Previous;
            first->Previous = node;
            last->Next = Next;
            Next = first;
            last->Next->Previous = last;
            return node;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeLinkedListNode Empty => new();
    }
}