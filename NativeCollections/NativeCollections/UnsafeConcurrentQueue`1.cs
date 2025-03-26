﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8602
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(NativeCollectionType.Standard)]
    public unsafe struct UnsafeConcurrentQueue<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private UnsafeConcurrentQueue _handle;

        /// <summary>
        ///     Not arm64
        /// </summary>
        private NativeConcurrentQueueNotArm64<T>* NotArm64Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (NativeConcurrentQueueNotArm64<T>*)Unsafe.AsPointer(ref _handle);
        }

        /// <summary>
        ///     Arm64
        /// </summary>
        private NativeConcurrentQueueArm64<T>* Arm64Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (NativeConcurrentQueueArm64<T>*)Unsafe.AsPointer(ref _handle);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeConcurrentQueue(int size, int maxFreeSlabs)
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
                *NotArm64Handle = new NativeConcurrentQueueNotArm64<T>(size, maxFreeSlabs);
            else
                *Arm64Handle = new NativeConcurrentQueueArm64<T>(size, maxFreeSlabs);
        }

        /// <summary>
        ///     IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => RuntimeInformation.ProcessArchitecture != Architecture.Arm64 ? NotArm64Handle->IsEmpty : Arm64Handle->IsEmpty;
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => RuntimeInformation.ProcessArchitecture != Architecture.Arm64 ? NotArm64Handle->Count : Arm64Handle->Count;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
                NotArm64Handle->Dispose();
            else
                Arm64Handle->Dispose();
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
                NotArm64Handle->Clear();
            else
                Arm64Handle->Clear();
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in T item)
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
                NotArm64Handle->Enqueue(item);
            else
                Arm64Handle->Enqueue(item);
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result) => RuntimeInformation.ProcessArchitecture != Architecture.Arm64 ? NotArm64Handle->TryDequeue(out result) : Arm64Handle->TryDequeue(out result);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentQueue<T> Empty => new();
    }

    /// <summary>
    ///     Unsafe concurrentQueue
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct UnsafeConcurrentQueue
    {
        /// <summary>
        ///     Cross segment lock
        /// </summary>
        private GCHandle _crossSegmentLock;

        /// <summary>
        ///     Segment pool
        /// </summary>
        private NativeMemoryPool _segmentPool;

        /// <summary>
        ///     Tail
        /// </summary>
        private nint _tail;

        /// <summary>
        ///     Head
        /// </summary>
        private nint _head;
    }
}