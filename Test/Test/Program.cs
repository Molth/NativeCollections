using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NativeCollections;

// ReSharper disable ALL

namespace Examples
{
    public static unsafe class SimpleAllocator
    {
        public static int AllocCount;

        public static void Custom() => NativeMemoryAllocator.Custom(&AlignedAlloc, &AlignedAllocZeroed, &AlignedFree);

        public static void* AlignedAlloc(uint byteCount, uint alignment)
        {
            Interlocked.Increment(ref AllocCount);
            return NativeMemory.AlignedAlloc(byteCount, alignment);
        }

        public static void* AlignedAllocZeroed(uint byteCount, uint alignment)
        {
            Interlocked.Increment(ref AllocCount);
            var ptr = NativeMemory.AlignedAlloc(byteCount, alignment);
            Unsafe.InitBlockUnaligned(ptr, 0, byteCount);
            return ptr;
        }

        public static void AlignedFree(void* ptr)
        {
            if (ptr == null)
                return;

            Interlocked.Decrement(ref AllocCount);
            NativeMemory.AlignedFree(ptr);
        }
    }

    internal sealed unsafe class Program
    {
        private static void Main()
        {
            SimpleAllocator.Custom();
            TestConcurrentQueue1();
            TestConcurrentQueue2();
            TestConcurrentStack();
            TestConcurrentDictionary();
            Console.WriteLine(SimpleAllocator.AllocCount == 0 ? "Success" : "Mismatch");
        }

        private static void TestConcurrentDictionary()
        {
            int testNumTotal = 4096 + Random.Shared.Next(0, 10000);
            int testThreadCount = Math.Min(Environment.ProcessorCount, 16);

            int sum = 0;
            for (int i = 0; i < testNumTotal; ++i)
                sum += i;

            var dict = UnsafeShardedDictionary<int, int>.Create();

            int addSum = 0;
            int removeSum = 0;

            var addTasks = new Task[testThreadCount];
            for (int t = 0; t < testThreadCount; t++)
            {
                int threadId = t;
                addTasks[threadId] = new Task(() =>
                {
                    int localSum = 0;
                    for (int i = threadId; i < testNumTotal; i += testThreadCount)
                    {
                        if (!dict.TryAdd(i, i))
                            throw new InvalidOperationException($"failed to add key {i} on thread {threadId}");
                        localSum += i;
                    }

                    Interlocked.Add(ref addSum, localSum);
                });
            }

            var removeTasks = new Task[testThreadCount];
            for (int t = 0; t < testThreadCount; t++)
            {
                int threadId = t;
                removeTasks[threadId] = new Task(() =>
                {
                    int localSum = 0;
                    for (int i = threadId; i < testNumTotal; i += testThreadCount)
                    {
                        int value;
                        while (!dict.TryRemove(i, out value))
                            Thread.SpinWait(1);

                        localSum += value;
                    }

                    Interlocked.Add(ref removeSum, localSum);
                });
            }

            var allTasks = new List<Task>(addTasks.Length + removeTasks.Length);
            allTasks.AddRange(addTasks);
            allTasks.AddRange(removeTasks);

            foreach (var task in allTasks)
                task.Start();

            Task.WaitAll(allTasks.ToArray());

            int mainRemoveSum = 0;
            for (int i = 0; i < testNumTotal; i++)
            {
                if (dict.TryRemove(i, out int value))
                    mainRemoveSum += value;
            }

            removeSum += mainRemoveSum;

            dict.Dispose();

            bool sumMatch = (addSum == removeSum) && (removeSum == sum);
            Console.WriteLine(sumMatch ? "Success" : "Mismatch");
        }

        private static void TestConcurrentStack()
        {
            int testNumTotal = 4096 + Random.Shared.Next(0, 10000);
            var testThreadCount = Math.Min(Environment.ProcessorCount, 16);

            var sum = 0;
            for (var i = 0; i < testNumTotal; ++i)
                sum += i;

            var queue = new UnsafeTreiberStack<int>();

            var enqueueSum = 0;
            var dequeueSum = 0;

            var enqueueThreads = new Task[testThreadCount];

            for (var t = 0; t < testThreadCount; t++)
            {
                var threadId = t;
                enqueueThreads[threadId] = new Task(() =>
                {
                    var localSum = 0;
                    for (var i = threadId; i < testNumTotal; i += testThreadCount)
                    {
                        try
                        {
                            queue.Push(i);
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
            }

            var dequeueThreads = new Task[testThreadCount];

            for (var t = 0; t < testThreadCount; t++)
            {
                dequeueThreads[t] = new Task(() =>
                {
                    var localSum = 0;
                    while (true)
                    {
                        if (queue.TryPop(out var item))
                            localSum += item;
                        else
                            break;
                    }

                    Interlocked.Add(ref dequeueSum, localSum);
                });
            }

            var a = new List<Task>();
            a.AddRange(enqueueThreads);
            a.AddRange(dequeueThreads);

            foreach (var task in a)
                task.Start();

            Task.WaitAll(a);

            {
                var localSum = 0;
                while (true)
                {
                    if (queue.TryPop(out var item))
                        localSum += item;
                    else
                        break;
                }

                Interlocked.Add(ref dequeueSum, localSum);
            }

            queue.Dispose();
        }

        private static void TestConcurrentQueue1()
        {
            int testNumTotal = 4096 + Random.Shared.Next(0, 10000);
            var testThreadCount = Math.Min(Environment.ProcessorCount, 16);

            var sum = 0;
            for (var i = 0; i < testNumTotal; ++i)
                sum += i;

            var queue = UnsafeSegQueue<int>.Create();

            var enqueueSum = 0;
            var dequeueSum = 0;

            var enqueueThreads = new Task[testThreadCount];

            for (var t = 0; t < testThreadCount; t++)
            {
                var threadId = t;
                enqueueThreads[threadId] = new Task(() =>
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
            }

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

            var a = new List<Task>();
            a.AddRange(enqueueThreads);
            a.AddRange(dequeueThreads);

            foreach (var task in a)
                task.Start();

            Task.WaitAll(a);

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
            }

            queue.Dispose();

            Console.WriteLine(enqueueSum == dequeueSum && dequeueSum == sum ? "Success" : "Mismatch");
        }

        private static void TestConcurrentQueue2()
        {
            int testNumTotal = 4096 + Random.Shared.Next(0, 10000);
            var testThreadCount = Math.Min(Environment.ProcessorCount, 16);

            var sum = 0;
            for (var i = 0; i < testNumTotal; ++i)
                sum += i;

            var queue = UnsafeArrayQueue<int>.Create(3333);

            var enqueueSum = 0;
            var dequeueSum = 0;

            int remainingProducers = testThreadCount;

            var enqueueThreads = new Task[testThreadCount];
            for (var t = 0; t < testThreadCount; t++)
            {
                var threadId = t;
                enqueueThreads[threadId] = new Task(() =>
                {
                    var localSum = 0;
                    for (var i = threadId; i < testNumTotal; i += testThreadCount)
                    {
                        var spinWait = new SpinWait();
                        while (!queue.TryEnqueue(i))
                            spinWait.SpinOnce();
                        localSum += i;
                    }

                    Interlocked.Add(ref enqueueSum, localSum);
                    Interlocked.Decrement(ref remainingProducers);
                });
            }

            int consumerCount = testThreadCount + 1;
            var dequeueThreads = new Task[consumerCount];
            for (var t = 0; t < consumerCount; t++)
            {
                dequeueThreads[t] = new Task(() =>
                {
                    var localSum = 0;
                    while (true)
                    {
                        if (queue.TryDequeue(out var item))
                        {
                            localSum += item;
                            continue;
                        }

                        if (Volatile.Read(ref remainingProducers) == 0)
                            break;

                        Thread.SpinWait(10);
                    }

                    Interlocked.Add(ref dequeueSum, localSum);
                });
            }

            var allTasks = new List<Task>(enqueueThreads);
            allTasks.AddRange(dequeueThreads);

            foreach (var task in allTasks)
                task.Start();

            Task.WaitAll(allTasks.ToArray());

            queue.Dispose();

            Console.WriteLine(enqueueSum == dequeueSum && dequeueSum == sum ? "Success" : "Mismatch");
        }
    }
}