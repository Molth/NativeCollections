# NativeCollections

[![NuGet](https://img.shields.io/nuget/v/NativeCollections.svg?style=flat-square)](https://www.nuget.org/packages/NativeCollections/)

---

## Introduction

- It is almost based on unmanaged memory, allowing developers to manipulate memory with lower overhead and more control, while offering a wide range of container types.
- The library provides extensive support for `Span<T>` and `unsafe` operations, making it suitable for scenarios with strict performance and memory behavior requirements.

---

## Features

- ✅ **GC-Free Allocation**: Most container data is allocated using unmanaged memory, without relying on the CLR garbage collector.
- ✅ **High Performance**: Compact structures, memory alignment, zero boxing, and support for high-performance algorithms.
- ✅ **Span Support**: Most containers support `Span<T>` / `ReadOnlySpan<T>`, enabling in-place modification and zero-copy data passing.
- ✅ **Thread-Safe Containers**: Includes several concurrent container variants suitable for multithreaded environments.
- ✅ **Easy Integration**: Uses standard C#, making it easy to replace existing containers.

---

## Why?

Most containers in the standard library cannot be used in unmanaged environments, and many lack support for `Span<T>` or `ReadOnlySpan<T>`.

For example:

- `List<T>` does not allow fast access via `Span<T>` or `ReadOnlySpan<T>`, which limits both performance and development efficiency.
- In older versions of .NET, `Dictionary<TKey, TValue>` does not expose direct references to values, requiring operations to be performed on copies instead. This adds unnecessary overhead in performance-critical scenarios.
- In many cases, types like `string` are overkill when only a `ReadOnlySpan<char>` is needed. By using `UnsafeString`, it’s possible to avoid frequent string allocations and reduce GC pressure significantly.

---

## How to use?

- All containers are value types or wrap unmanaged pointers. Please avoid using them before proper initialization to prevent undefined behavior.
- Most containers require calling `Dispose()` after use to free unmanaged resources properly.
- Usage of most containers is very similar to the standard library counterparts, with added convenience methods like converting to `Span<T>` or `ReadOnlySpan<T>`.
- For short-lived containers, you can take advantage of the `using` statement to automatically call `Dispose()`.
- For `StackallocCollection` series, you can use the `stackalloc` syntax. Alternatively, you can provide any fixed buffer from outside, such as unmanaged memory or fixed managed memory.
- For `NativeCollection` series, they act as wrappers around the `UnsafeCollection` series and additionally store a handle pointer for managing the underlying resource.
- For `UnsafeCollection` series, they do not store a handle pointer themselves and are implemented directly as structs, providing a more lightweight but less managed usage.
- You can use `NativeMemoryAllocator.Custom` to override:
  - `public static void* AlignedAlloc(uint byteCount, uint alignment)`
  - `public static void* AlignedAllocZeroed(uint byteCount, uint alignment)`
  - `public static void AlignedFree(void* ptr)`
- You can use `NativeHashCode.Custom` to override:
  - `public static int GetHashCode(ReadOnlySpan<byte> buffer)`.
- You can use `UnsafeString.Custom` to override:
  - `public static int GetHashCode(ReadOnlySpan<char> buffer)`.

---

## Project status

This project is actively under development. Welcome your [suggestions and feedback](https://github.com/Molth/NativeCollections/issues).

---

## License

This project is licensed under the MIT License.

---

## Some implemented types

1. `BitArray`
2. `Deque<T>`
3. `Dictionary<TKey, TValue>`
4. `HashSet<T>`
5. `List<T>`
6. `MemoryStream`
7. `OrderedDictionary<TKey, TValue>`
8. `OrderedHashSet<T>`
9. `PriorityQueue<TKey, TValue>`
10. `Queue<T>`
11. `SortedDictionary<TKey, TValue>`
12. `SortedList<TKey, TValue>`
13. `SortedSet<T>`
14. `SparseSet<TValue>`
15. `Stack<T>`
16. `String`
17. `StringBuilder<T>`