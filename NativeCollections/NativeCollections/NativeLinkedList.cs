using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native linked list
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public unsafe struct NativeLinkedList
    {
        /// <summary>
        ///     Sentinel
        /// </summary>
        public NativeLinkedListNode Sentinel;

        /// <summary>
        ///     Head
        /// </summary>
        public NativeLinkedListNode* Head
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Sentinel.Next;
        }

        /// <summary>
        ///     Tail
        /// </summary>
        public NativeLinkedListNode* Tail
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (NativeLinkedListNode*)Unsafe.AsPointer(ref Sentinel);
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated
        {
            get
            {
                var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref Sentinel);
                return node->Next != null && node->Previous != null;
            }
        }

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => Head == Tail;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("Cannot call Equals on NativeLinkedList");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => throw new NotSupportedException("Cannot call GetHashCode on NativeLinkedList");

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeLinkedList";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeLinkedList left, NativeLinkedList right) => throw new NotSupportedException("Cannot call Equals on NativeLinkedList");

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeLinkedList left, NativeLinkedList right) => throw new NotSupportedException("Cannot call Equals on NativeLinkedList");

        /// <summary>
        ///     Count
        /// </summary>
        /// <returns>Count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count()
        {
            var tail = Tail;
            var count = 0;
            for (var node = Head; node != tail; node = node->Next)
                ++count;
            return count;
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var node = (NativeLinkedListNode*)Unsafe.AsPointer(ref Sentinel);
            node->Next = node;
            node->Previous = node;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeLinkedList Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new((NativeLinkedList*)Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     Native linked list
            /// </summary>
            private readonly NativeLinkedList* _nativeLinkedList;

            /// <summary>
            ///     Native linked list node
            /// </summary>
            private NativeLinkedListNode* _node;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeLinkedList">NativeLinkedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeLinkedList* nativeLinkedList)
            {
                _nativeLinkedList = nativeLinkedList;
                _node = nativeLinkedList->Tail;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                ref var node = ref _node;
                node = node->Next;
                return node != _nativeLinkedList->Tail;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public NativeLinkedListNode* Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _node;
            }
        }
    }
}