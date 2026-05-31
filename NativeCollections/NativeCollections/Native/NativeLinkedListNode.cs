using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native linked list node
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
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
        ///     Empty
        /// </summary>
        public static NativeLinkedListNode Empty => new();
    }
}