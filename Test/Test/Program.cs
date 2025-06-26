using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Threading;
using System.Threading.Tasks;
using NativeCollections;

// ReSharper disable ALL

namespace Examples
{
    internal sealed unsafe class Program
    {
        private static void Main()
        {
            const uint a = 8;
            Console.WriteLine((nint)(-a));
        }

        static void TestDictionary()
        {
            const int capacity = 4;
            var byteCount = StackallocDictionary<int, Vector512<byte>>.GetByteCount(capacity);
            Console.WriteLine(byteCount);
            Span<byte> buffer = stackalloc byte[byteCount];
            var stack = new StackallocDictionary<int, Vector512<byte>>(buffer, capacity);
            stack.TryAdd(0, Vector512<byte>.One);
            if (stack.TryGetValueReference(0, out var reference))
                Console.WriteLine(reference.Value == Vector512<byte>.One);
        }

        private static void TestSortedDictionary()
        {
            const int capacity = 4;
            var byteCount = StackallocSortedDictionary<int, Vector512<byte>>.GetByteCount(capacity);
            var sortedSet = new StackallocSortedDictionary<int, Vector512<byte>>(stackalloc byte[byteCount], capacity);
            sortedSet.TryAdd(0, Vector512<byte>.One);
            if (sortedSet.TryGetValueReference(0, out var reference))
                Console.WriteLine(reference.Value == Vector512<byte>.One);
        }

        private static void TestLinearMemoryPool()
        {
            using var buffer = new NativeLinearMemoryPool(1024, 4);
            var ptr1 = buffer.Rent(64, 64);

            Console.WriteLine((nint)ptr1 % 64 == 0);

            var a1 = Vector512.LoadAligned((byte*)ptr1);
            Console.WriteLine("Load 1");

            var ptr2 = buffer.Rent(64, 64);
            Console.WriteLine((nint)ptr2 % 64 == 0);

            var a2 = Vector512.LoadAligned((byte*)ptr2);
            Console.WriteLine("Load 2");

            Console.WriteLine((nint)ptr2 - (nint)ptr1);

            NativeRandom.Next(ptr2, 64);

            var a3 = Vector512.LoadAligned((byte*)ptr1);

            Console.WriteLine(a1 == a3);

            buffer.Return(ptr1);
            buffer.Return(ptr2);
        }

        private static void TestStackalloc2()
        {
            var ptr = stackalloc byte[1024];
            var buffer = new NativeMemoryLinearAllocator(ptr, 1024);
            var result = buffer.TryAlignedAlloc<Vector512<byte>>(1, out var ptr1);

            var position1 = buffer.Position;

            if (result)
            {
                Console.WriteLine((nint)ptr1 % 64 == 0);
                Console.WriteLine(buffer.Position);
            }

            var result2 = buffer.TryAlignedAlloc<Vector512<byte>>(1, out var ptr2);
            if (result2)
            {
                Console.WriteLine((nint)ptr2 % 64 == 0);
                Console.WriteLine(buffer.Position);
            }

            Vector512.LoadAligned((byte*)ptr1);
            Console.WriteLine("Load 1");

            Vector512.LoadAligned((byte*)ptr2);
            Console.WriteLine("Load 2");

            Console.WriteLine((nint)ptr % 64 + " " + (position1 - 64));
        }

        private static void TestConcurrent()
        {
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
    }
}