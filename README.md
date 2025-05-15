# NativeCollections

[![NuGet](https://img.shields.io/nuget/v/NativeCollections.svg?style=flat-square)](https://www.nuget.org/packages/NativeCollections/)

## Introduction

- It is fully based on unmanaged memory, allowing developers to manipulate memory with lower overhead and more control, while offering a wide range of container types.
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
- In many cases, types like `string` are overkill when only a `ReadOnlySpan<char>` is needed. By using `NativeString`, it’s possible to avoid frequent string allocations and reduce GC pressure significantly.

---

## How to use?

- All containers are value types or wrap unmanaged pointers. Please avoid using them before proper initialization to prevent undefined behavior.
- Most containers require calling `Dispose()` after use to free unmanaged resources properly.
- Usage of most containers is very similar to the standard library counterparts, with added convenience methods like converting to `Span<T>` or `ReadOnlySpan<T>`.
- For short-lived containers, you can take advantage of the `using` statement to automatically call `Dispose()`.
- For `StackallocCollection` series, you can use the `stackalloc` syntax. Alternatively, you can provide any fixed buffer from outside, such as unmanaged memory or fixed managed memory.
- For `NativeCollection` series, they act as wrappers around the `UnsafeCollection` series and additionally store a handle pointer for managing the underlying resource.
- For `UnsafeCollection` series, they do not store a handle pointer themselves and are implemented directly as structs, providing a more lightweight but less managed usage.
- You can use `NativeMemoryAllocator.Custom` to override: `Alloc`, `AllocZeroed`, `Free`.
- You can use `NativeHashCode.Custom` to override: `GetHashCode`.
- You can use `NativeString.Custom` to override: `GetHashCode`.

---

## Project status

This project is actively under development. Welcome your [suggestions and feedback](https://github.com/Molth/NativeCollections/issues).

---

## License

This project is licensed under the MIT License.

---

## Some implemented types

1. `BitArray`
2. `ConcurrentDictionary<TKey, TValue>`
3. `ConcurrentHashSet<T>`
4. `ConcurrentQueue<T>`
5. `ConcurrentStack<T>`
6. `Deque<T>`
7. `Dictionary<TKey, TValue>`
8. `HashSet<T>`
9. `List<T>`
10. `MemoryStream`
11. `OrderedDictionary<TKey, TValue>`
12. `OrderedHashSet<T>`
13. `PriorityQueue<TKey, TValue>`
14. `Queue<T>`
15. `SortedDictionary<TKey, TValue>`
16. `SortedList<TKey, TValue>`
17. `SortedSet<T>`
18. `SparseSet<T>`
19. `Stack<T>`
20. `String`
21. `StringBuilder<T>`