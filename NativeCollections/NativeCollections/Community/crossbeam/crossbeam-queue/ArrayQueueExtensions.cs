using System.Runtime.CompilerServices;
using NativeCollections;
using static crossbeam.Array_Queue;

// ReSharper disable All

namespace crossbeam
{
    internal static class ArrayQueueExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty<T>(ref this ArrayQueue<T> queue) where T : unmanaged => queue.is_empty();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFull<T>(ref this ArrayQueue<T> queue) where T : unmanaged => queue.is_full();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>(ref this ArrayQueue<T> queue) where T : unmanaged => (int)queue.len();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Capacity<T>(ref this ArrayQueue<T> queue) where T : unmanaged => (int)queue.capacity();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryEnqueue<T>(ref this ArrayQueue<T> queue, T item) where T : unmanaged => queue.push(item).is_ok();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static InsertResult Enqueue<T>(ref this ArrayQueue<T> queue, T item, out T overwritten) where T : unmanaged
        {
            var option = queue.force_push(item);
            if (option.is_some())
            {
                overwritten = option.unwrap_unchecked();
                return InsertResult.Overwritten;
            }

            overwritten = default;
            return InsertResult.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDequeue<T>(ref this ArrayQueue<T> queue, out T result) where T : unmanaged
        {
            var option = queue.pop();
            if (option.is_some())
            {
                result = option.unwrap_unchecked();
                return true;
            }

            result = default;
            return false;
        }
    }
}