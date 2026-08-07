using System.Runtime.CompilerServices;
using static crossbeam.Seg_Queue;

// ReSharper disable All

namespace crossbeam
{
    internal static class SegQueueExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty<T>(ref this SegQueue<T> queue) where T : unmanaged => queue.is_empty();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Count<T>(ref this SegQueue<T> queue) where T : unmanaged => (int)queue.len();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enqueue<T>(ref this SegQueue<T> queue, T item) where T : unmanaged => queue.push(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDequeue<T>(ref this SegQueue<T> queue, out T result) where T : unmanaged
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryDequeueMut<T>(ref this SegQueue<T> queue, out T result) where T : unmanaged
        {
            var option = queue.pop_mut();
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