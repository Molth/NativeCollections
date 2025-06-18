using System;
using System.Runtime.InteropServices;
using NativeCollections;

namespace Examples
{
    public sealed class ExampleString : IExample
    {
        public static void Start()
        {
            Span<char> buffer1 = stackalloc char[128];
            var buffer2 = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(buffer1), buffer1.Length);

            var str = new NativeString(buffer2, 0);
            str.Append("test1: ");
            str.AppendFormattable(100, "D4");
            Console.WriteLine(str.ToString());
            Console.WriteLine();

            str.Clear();

            str.AppendFormatted($"test2: {100:D4}");
            Console.WriteLine(str.ToString());
            Console.WriteLine();

            str.Clear();

            str.Append("test1 test2 test3 test4");
            foreach (var span in str.Split(' '))
                Console.WriteLine(span.ToString());
        }
    }
}