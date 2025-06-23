using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Threading;
using System.Threading.Tasks;
using mimalloc;
using NativeCollections;

// ReSharper disable ALL

namespace Examples
{
    internal sealed unsafe class Program
    {
        public static int GetByteCount<T>(uint elementCount, out uint byteCount, out uint alignment, out uint byteOffset) where T : unmanaged
        {
            byteCount = elementCount * (uint)sizeof(T);
            alignment = (uint)NativeMemoryAllocator.AlignOf<T>();
            byteOffset = alignment - 1 + (uint)sizeof(nint);
            return (int)(byteCount + (uint)byteOffset);
        }

        public static T* ToAlignedPtr<T>(void* ptr, uint byteCount, uint alignment, uint byteOffset) where T : unmanaged
        {
            var result = (void*)(((nint)ptr + (nint)byteOffset) & ~((nint)alignment - 1));
            ((void**)result)[-1] = ptr;
            return (T*)result;
        }

        private static void Main()
        {
            void* ptr = stackalloc byte[GetByteCount<Vector512<byte>>(4, out uint byteCount, out uint alignment, out uint byteOffset)];
            var buffer = ToAlignedPtr<Vector512<byte>>(ptr, byteCount, alignment, byteOffset);
            Console.WriteLine(((nint)buffer) % alignment);
        }

        private static void TestConcurrent()
        {
            Custom();

            const int testNumTotal = 4096 + 3333;
            var testThreadCount = Math.Min(Environment.ProcessorCount, 16);

            var queue = new NativeConcurrentQueue<int>(1, 1);
            var enqueueSum = 0;

            Parallel.For(0, testThreadCount, threadId =>
            {
                var localSum = 0;
                for (var i = threadId; i < testNumTotal; i += testThreadCount)
                {
                    try
                    {
                        queue.Enqueue(i);
                        localSum += i;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Thread {threadId} failed on {i}: {e}");
                        throw;
                    }
                }

                Interlocked.Add(ref enqueueSum, localSum);
            });

            var dequeueSum = 0;
            var dequeueThreads = new Task[testThreadCount];

            for (var t = 0; t < testThreadCount; t++)
            {
                dequeueThreads[t] = new Task(() =>
                {
                    var localSum = 0;
                    while (true)
                    {
                        if (queue.TryDequeue(out var item))
                            localSum += item;
                        else
                            break;
                    }

                    Interlocked.Add(ref dequeueSum, localSum);
                });
            }

            foreach (var task in dequeueThreads)
                task.Start();

            Task.WaitAll(dequeueThreads);
            queue.Dispose();

            var sum = 0;
            for (var i = 0; i < testNumTotal; ++i)
                sum += i;

            Console.WriteLine(enqueueSum == dequeueSum && dequeueSum == sum ? "Success" : "Mismatch");
        }

        private static void Test()
        {
            const int testNumTotal = 4096 + 3333;
            var testThreadCount = Math.Min(Environment.ProcessorCount, 16);

            var queue = new Queue<int>(1024);
            var enqueueSum = 0;

            Parallel.For(0, testThreadCount, threadId =>
            {
                var localSum = 0;
                for (var i = threadId; i < testNumTotal; i += testThreadCount)
                {
                    try
                    {
                        queue.Enqueue(i);
                        localSum += i;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Thread {threadId} failed on {i}: {e}");
                        throw;
                    }
                }

                Interlocked.Add(ref enqueueSum, localSum);
            });

            var dequeueSum = 0;
            var dequeueThreads = new Task[testThreadCount];

            for (var t = 0; t < testThreadCount; t++)
            {
                dequeueThreads[t] = new Task(() =>
                {
                    var localSum = 0;
                    while (true)
                    {
                        if (queue.TryDequeue(out var item))
                            localSum += item;
                        else
                            break;
                    }

                    Interlocked.Add(ref dequeueSum, localSum);
                });
            }

            foreach (var task in dequeueThreads)
                task.Start();

            Task.WaitAll(dequeueThreads);

            var sum = 0;
            for (var i = 0; i < testNumTotal; ++i)
                sum += i;

            Console.WriteLine(enqueueSum == dequeueSum && dequeueSum == sum ? "Success" : "Mismatch");
        }

        private static unsafe void Custom()
        {
            try
            {
                _ = MiMalloc.mi_version();
            }
            catch
            {
                return;
            }

            NativeMemoryAllocator.Custom(&Alloc, &AllocZeroed, &Free);
            return;

            static void* Alloc(uint byteCount) => MiMalloc.mi_malloc(byteCount);
            static void* AllocZeroed(uint byteCount) => MiMalloc.mi_zalloc(byteCount);
            static void Free(void* ptr) => MiMalloc.mi_free(ptr);
        }
    }
}