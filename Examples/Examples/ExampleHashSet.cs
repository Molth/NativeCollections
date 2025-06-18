using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeCollections;

namespace Examples
{
    public sealed class ExampleHashSet : IExample
    {
        public static void Start()
        {
            var list = new List<int>(1024);
            for (var i = 0; i < list.Capacity; ++i)
            {
                var item = Random.Shared.Next(list.Capacity / 2, list.Capacity);
                list.Add(item);
            }

            var list1 = list.Distinct().ToArray();

            var span = CollectionsMarshal.AsSpan(list);

            var builder1 = Distinct1((ReadOnlySpan<int>)span);
            var list2 = builder1.AsSpan().ToArray();
            builder1.Dispose();

            var builder2 = new NativeValueListBuilder<int>(0);
            Distinct3(span, ref builder2);
            var list4 = builder2.AsSpan().ToArray();
            builder2.Dispose();

            Distinct2(ref span);
            CollectionsMarshal.SetCount(list, span.Length);
            var list3 = list.ToArray();

            Console.WriteLine(list1.AsSpan().SequenceEqual(list2) && list2.AsSpan().SequenceEqual(list3) && list3.AsSpan().SequenceEqual(list4));
        }

        private static NativeValueListBuilder<T> Distinct1<T>(ReadOnlySpan<T> source) where T : unmanaged, IEquatable<T>
        {
            var builder = new NativeValueListBuilder<T>(16);
            var byteCount = StackallocHashSet<T>.GetByteCount(source.Length);
            Span<byte> bytes;
            var buffer = NativeTempPinnedBuffer<byte>.Empty;
            if (byteCount <= 1024)
            {
                Span<byte> bytes2 = stackalloc byte[byteCount];
                bytes = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(bytes2), byteCount);
            }
            else
            {
                buffer = new NativeTempPinnedBuffer<byte>(byteCount, true);
                bytes = buffer.AsSpan();
            }

            var hashSet = new StackallocHashSet<T>(bytes, source.Length);
            ref var reference = ref MemoryMarshal.GetReference(source);
            for (var i = 0; i < source.Length; ++i)
            {
                ref var item = ref Unsafe.Add(ref reference, i);
                var result = hashSet.TryAdd(item);
                if (result == InsertResult.Success)
                    builder.Append(item);
            }

            buffer.Dispose();
            return builder;
        }

        private static void Distinct2<T>(ref Span<T> source) where T : unmanaged, IEquatable<T>
        {
            var index = 0;
            var byteCount = StackallocHashSet<T>.GetByteCount(source.Length);
            Span<byte> bytes;
            var buffer = NativeTempPinnedBuffer<byte>.Empty;
            if (byteCount <= 1024)
            {
                Span<byte> bytes2 = stackalloc byte[byteCount];
                bytes = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(bytes2), byteCount);
            }
            else
            {
                buffer = new NativeTempPinnedBuffer<byte>(byteCount, true);
                bytes = buffer.AsSpan();
            }

            var hashSet = new StackallocHashSet<T>(bytes, source.Length);
            ref var reference = ref MemoryMarshal.GetReference(source);
            for (var i = 0; i < source.Length; ++i)
            {
                ref var item = ref Unsafe.Add(ref reference, i);
                var result = hashSet.TryAdd(item);
                if (result == InsertResult.Success)
                {
                    if (index != i)
                        source[index] = source[i];
                    ++index;
                }
            }

            buffer.Dispose();
            source = MemoryMarshal.CreateSpan(ref reference, index);
        }

        private static void Distinct3<T>(ReadOnlySpan<T> source, ref NativeValueListBuilder<T> builder) where T : unmanaged, IEquatable<T>
        {
            var byteCount = StackallocHashSet<T>.GetByteCount(source.Length);
            using var buffer = new NativeTempPinnedBuffer<byte>(byteCount, true);
            var hashSet = new StackallocHashSet<T>(buffer.AsSpan(), source.Length);
            ref var reference = ref MemoryMarshal.GetReference(source);
            for (var i = 0; i < source.Length; ++i)
            {
                ref var item = ref Unsafe.Add(ref reference, i);
                var result = hashSet.TryAdd(item);
                if (result == InsertResult.Success)
                    builder.Append(item);
            }
        }
    }
}