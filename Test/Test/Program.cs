using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NativeCollections;

// ReSharper disable ALL

namespace Examples
{
    public struct TestStruct
    {
        public Guid Guid1;
        public Guid Guid2;
    }

    public struct Sb : ISpanFormattable
    {
        public float Value;
        public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
        public override string ToString() => Value.ToString("F5", CultureInfo.GetCultureInfo("de-DE"));
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => Value.TryFormat(destination, out charsWritten, format, provider);
    }

    internal sealed unsafe class Program
    {
        private static RwLock<int> _lock;

        private static void Main()
        {
            var writerCount = Environment.ProcessorCount;
            var readerCount = Environment.ProcessorCount;
            var iterations = Random.Shared.Next(1000, 10000);
            var errors = 0;
            var writers = Task.Run(() =>
            {
                Parallel.For(0, writerCount, _ =>
                {
                    for (var j = 0; j < iterations; ++j)
                    {
                        using var write = _lock.BorrowMutable();
                        ref var value = ref write.Unwrap();
                        value++;
                    }
                });
            });

            var readers = Task.Run(() =>
            {
                Parallel.For(0, readerCount, _ =>
                {
                    var last = 0;
                    for (var j = 0; j < iterations; ++j)
                    {
                        using var read = _lock.Borrow();
                        ref readonly var value = ref read.Unwrap();
                        var current = value;
                        if (current < last)
                            Interlocked.Increment(ref errors);
                        last = current;
                    }
                });
            });

            Task.WaitAll(writers, readers);

            using (var read = _lock.Borrow())
            {
                ref readonly var final = ref read.Unwrap();
                var expected = writerCount * iterations;
                if (final != expected || errors != 0)
                    throw new Exception($"RwLock test failed: final={final}, expected={expected}, read-errors={errors}.");
                else
                    Console.WriteLine($"Success. {expected}");
            }
        }

        private static void TestStateMachine()
        {
            var stateMachine = new StateMachine(3);
            stateMachine.Change<TestStateA>();
            stateMachine.Update();
            stateMachine.Change<TestStateB>();
            stateMachine.Update();
            stateMachine.Change<TestStateA>();
            stateMachine.Update();
            stateMachine.Update();
        }

        private static void TestFill2()
        {
            var sb = new StringBuilder();
            sb.Append((char)(0x7F + 1), 16);

            var sb2 = new NativeStringBuilder<byte>();
            sb2.Append((char)(0x7F + 1), 16);

            Console.WriteLine(sb.ToString() == sb2.ToString());
        }

        private static void TestFill()
        {
            Span<TestStruct> guids = stackalloc TestStruct[4];
            Span<TestStruct> guids2 = stackalloc TestStruct[4];
            var value = new TestStruct { Guid1 = Guid.NewGuid(), Guid2 = Guid.NewGuid() };
            NativeMemoryAllocator.Fill(ref Unsafe.As<TestStruct, byte>(ref MemoryMarshal.GetReference(guids)), (uint)guids.Length, value);
            guids2.Fill(value);
            Console.WriteLine(MemoryMarshal.AsBytes(guids).SequenceEqual(MemoryMarshal.AsBytes(guids2)));
        }

        private static void TestString2()
        {
            var sb = new StringBuilder();
            sb.Append((char)(0x7F + 1), 4);
            sb.Append(new Sb { Value = 100.12345f });
            sb.Append(' ');
            sb.Append("sb".AsMemory());
            sb.Append(' ');
            sb.Append(new Sb { Value = 100.12345f });
            sb.Append(' ');
            sb.Append("sb".AsMemory());
            sb.Append(' ');
            sb.Append(100.12345f.ToString("F5", CultureInfo.GetCultureInfo("de-DE")));
            sb.Append(' ');
            sb.Append(CultureInfo.GetCultureInfo("de-DE"), $"Sb{1.250f:F5}");
            var a = CompositeFormat.Parse("First: {0,10:D5}, Second: {1,-10:D5}, {3} Third: {2:D4}, 4th: {4:F5}");
            sb.Append(' ');
            sb.AppendFormat(CultureInfo.GetCultureInfo("de-DE"), a, 100, 100, 100, 1, 1.250f);
            var str1 = sb.ToString();
            var str2 = TestString();
            Console.WriteLine(str1 == str2);
            Console.WriteLine(str1);
            Console.WriteLine(str2);
        }

        private static string TestString()
        {
            var sb = new NativeStringBuilder<byte>();
            sb.Append((char)(0x7F + 1), 4);
            sb.AppendFormat<Sb?>(new Sb { Value = 100.12345f }, "F5", CultureInfo.GetCultureInfo("de-DE"));
            sb.Append(' ');
            sb.AppendFormat<object>("sb".AsMemory());
            sb.Append(' ');
            Sb? b = new Sb { Value = 100.12345f };
            sb.AppendFormat(b, "F5", CultureInfo.GetCultureInfo("de-DE"));
            sb.Append(' ');
            sb.AppendFormat<object>("sb".AsMemory());
            sb.Append(' ');
            sb.AppendFormat<float?>(100.12345f, "F5", CultureInfo.GetCultureInfo("de-DE"));
            sb.Append(' ');
            sb.Append(CultureInfo.GetCultureInfo("de-DE"), $"Sb{1.250f:F5}");
            var a = NativeCompositeFormat.Parse("First: {0,10:D5}, Second: {1,-10:D5}, {3} Third: {2:D4}, 4th: {4:F5}");
            sb.Append(' ');
            sb.AppendFormat(CultureInfo.GetCultureInfo("de-DE"), a, 100, 100, 100, 1, 1.250f);
            var str2 = sb.ToString();
            sb.Dispose();
            return str2;
        }

        private static string TestString3()
        {
            Span<char> buffer = stackalloc char[1024];
            var sb = new NativeString(MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(buffer), buffer.Length), 0).AsRef();
            sb.AppendFormatted(CultureInfo.GetCultureInfo("de-DE"), $"Sb{1.250f:F5}");
            var str2 = sb.ToString();
            return str2;
        }

        private static string TestString4()
        {
            var sb = new NativeStringBuilder<char>();
            sb.AppendFormatted(CultureInfo.GetCultureInfo("de-DE"), $"Sb{1.250f:F5}");
            var str2 = sb.ToString();
            return str2;
        }

        private static void TestQueue()
        {
            const int capacity = 4;
            var queue1 = new Queue<int>(capacity);
            var queue2 = new NativeQueue<int>(capacity);
            for (var i = 0; i < Random.Shared.Next(128, 1024); ++i)
            {
                var item = Random.Shared.Next();
                queue1.Enqueue(item);
                queue2.Enqueue(item);
            }

            var array1 = new int[queue1.Count].AsSpan();
            Span<int> array2 = stackalloc int[queue2.Count];

            var index = 0;
            foreach (var item in queue1)
                array1[index++] = item;

            queue2.CopyTo(array2);

            Console.WriteLine(array1.SequenceEqual(array2));
        }

        private static void TestDictionary()
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