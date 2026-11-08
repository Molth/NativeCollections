# NativeCollections

[![NuGet](https://img.shields.io/nuget/v/NativeCollections.svg)](https://www.nuget.org/packages/NativeCollections)

Pure C# collections, atomics, allocators, and concurrency primitives backed by unmanaged memory, with managed fallbacks only where documented. The public surface mirrors the .NET BCL where practical, and types backed by native storage do not allocate managed objects on their hot paths.

## Design model

The prefix defines ownership only for the wrappers below. Static support types (`NativeMemoryAllocator`, `NativeHashCode`, `NativeBitwise`, `NativeRandom`) also use the `Native` prefix but hold no resource.

| Flavor                 | Storage and ownership                                                                                                                                                                                                                                                                                    | Lifetime                                                         |
|------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------|
| `Native*`              | Holds the resource behind a handle: usually a pointer to the underlying `Unsafe*` value, a `GCHandle` for BCL-backed types, or the buffer itself for the `NativeArray` family. `Dispose` frees it; scope types such as `NativeReaderWriterLockScope` release a lock instead.                             | Dispose the owning value exactly once.                           |
| `Unsafe*`              | Stores its state directly in the struct: a buffer pointer for collections and views, or a `NativeObject<T>` handle for BCL-backed wrappers. `Dispose` frees an owning value's buffer or handle; `UnsafeDisposable<T>` instead disposes the wrapped value, and views such as `UnsafeString` free nothing. | Dispose only the owning values, exactly once.                    |
| `Stackalloc*`          | Operates on a caller-provided buffer. Never allocates or frees it.                                                                                                                                                                                                                                       | No `Dispose`; the caller owns the buffer lifetime.               |
| Shared support         | Allocators, hashing, bitwise, and random. These hold no handle and define no `Dispose`.                                                                                                                                                                                                                  | No resource lifetime.                                            |

Where an `Unsafe*` counterpart exists, `Native*` forwards to it; the `NativeArray` family stores its buffer pointer directly. `Stackalloc*` offers the same core operations over fixed storage, but no growth members. Member names, results, and semantics are kept aligned across flavors; any difference must be documented. `[BindingType]` records the type that the decorated type is bound to.

- **Stream growth differs by flavor.** `UnsafeMemoryStream.SetLength` returns `void` and may grow its backing buffer; `StackallocMemoryStream.SetLength` returns `bool` because it cannot grow a caller-provided buffer, returning `false` when the requested length exceeds `Capacity`.
- **No ownership tracking.** There are no ownership flags, reference counts, or idempotent `Dispose`. Double free, freeing memory you did not allocate, and using a disposed instance are caller errors with undefined behavior, not library bugs. The `GCHandle` wrappers `NativeObject<T>`, `NativeMemory<T>`, `NativeArrayRef<T>`, and `NativeMonitorLock` follow the same rule: they keep no ownership state, and `IsCreated` only observes the instance's `readonly` handle field.
- **Owning values are not shareable.** Copying an owning `Native*` or `Unsafe*` value copies its handle or pointer, not the resource; only one copy may be disposed.
- **Repeated or copied `GCHandle` disposal is caller error.** `IsCreated` in `NativeObject<T>`, `NativeMemory<T>`, `NativeArrayRef<T>`, and `NativeMonitorLock` only observes the instance's `readonly` handle field. Because that field cannot be cleared, a second `Dispose()` on the same instance, or a `Dispose()` on any copy after one copy has freed the handle, frees the same `GCHandle` again. That is caller error with undefined behavior and must not be reported as a library bug. Keep exactly one owning instance, dispose it once, and pass references or views to everything else.
- **Views are not owned.** `From(Span<T>)`, `FromBytes`, `Slice`, and span accessors such as `AsSpan`/`AsReadOnlySpan` create views over existing memory. Never dispose a view, and never free an interior pointer. A factory that accepts an existing pointer or handle, such as `NativeObject<T>.From(GCHandle)` or `NativePtr<T>.From(T*)`, transfers ownership of it to the returned value.

## Behavior contract

- **BCL parity.** Types marked `FromType.Standard` follow the corresponding BCL type. Documented deviations are intentional. `FromType.NotImplemented` means the type wraps a managed BCL object instead of using native storage; its API and thread safety are those of the wrapped type. `FromType.None` marks a type with no BCL counterpart.
- **Sizing.** `Count` is the logical element count. `Capacity` is allocated storage. `NativeArray<T>.Length` and `Count` are the fixed element count of the buffer it wraps. `UnsafeStringBuilder<T>.Length`/`Count` is the logical element count, `Capacity` the allocated element count, `Buffer` a span over the whole storage, and `Text` a span over the logical content.
- **Unchecked indexing.** Indexers are unchecked unless a type documents otherwise; the checked ones go through `Span<T>` and throw `IndexOutOfRangeException` (for example `UnsafeStringBuilder<T>`). Where unchecked, callers must stay within `0 .. Count - 1` or the documented range, and violations are caller errors that yield garbage values or memory corruption, never exceptions.
- **Bool and Try results.** `Try*`, `Write*`, and other bool APIs return `false` for "already present", "not found", or "insufficient capacity" exactly as documented; they never throw for these outcomes. `InsertResult` reports `None` (the default), `Success`, `AlreadyExists`, `Overwritten`, or `InsufficientCapacity`. None of these outcomes is an exception.
- **Exceptions.** Argument errors throw `ArgumentException`/`ArgumentOutOfRangeException`; invalid states throw `InvalidOperationException`; unsupported operations throw `NotSupportedException`. Public members document the exceptions they throw, with their trigger conditions.
- **Allocation failure.** Allocation failure throws `OutOfMemoryException`, unless an abort callback was installed through `NativeMemoryAllocator.Custom`; that callback then runs, and the allocation helper throws `OutOfMemoryException` when it returns. The library does not retry, clean up, or roll back on allocation failure; that policy belongs to the caller.
- **Enumeration.** `foreach` uses pattern-based `GetEnumerator()`, which returns an enumerator holding a pointer to the source, so the source must not move while it is enumerated. `IEnumerable<T>.GetEnumerator()` throws `NotSupportedException` to avoid boxing, so LINQ and interface-based enumeration are unsupported. Mutations that bump the collection version invalidate enumerators, which then throw `InvalidOperationException`; each type documents which operations bump it (for example, `UnsafeDictionary.Remove` does not). Enumerators that own traversal state must be disposed when used manually.
- **Equality and hashing.** Collection and handle values compare the whole struct byte for byte, padding included, through `SpanHelpers`; buffers passed to constructors marked `[MustBeZeroed]` must be zero-initialized; text types compare content. Types that declare `IEquatable<T>` provide `Equals(T)`; equality operators, `GetHashCode`, and `ToString` are not guaranteed and exist only where documented. Atomics are the exception: their obsolete `Equals(object)`/`GetHashCode` throw `NotSupportedException`.
- **Atomics.** `UnsafeAtomic*` values must have a stable address: pin them with `fixed` or store them in unmanaged memory. Default operations are sequentially consistent; `Load`/`Store` take an explicit `Ordering` (`Relaxed` through `SeqCst`), and `Load` rejects `Release` while `Store` rejects `Acquire` with `NotSupportedException`. Do not copy an atomic while it is in use.
- **Concurrency.** Thread safety is limited to `UnsafeAtomic*` types, `Concurrency/` and `Threading/` types, and types documented as thread-safe. Ordinary collections have no internal locking; concurrent use is a caller error. On concurrent collections, `Count` is an observation that may be stale; use `IsEmpty` for decisions.
- **Customization.** `NativeMemoryAllocator.Custom`, `NativeHashCode.Custom`, and `UnsafeString.Custom` are process-wide hooks. Install them once, before any dependent type is used. Callbacks must be thread-safe and must not re-enter types that use the same hook.

## Source conventions

- One public type per file; the file name is the type name, including the generic arity backtick (``UnsafeList`1.cs``). Nested implementation types stay nested, and partial files add a dot-separated suffix before `.cs` (`UnsafeStringBuilderExtensions.Format.cs`). Directories follow flavor and category: `Native/Collections`, `Unsafe/Atomics`, `Stackalloc/Pools`, `Specialized/Allocators`, and so on.
- Public value types use `[StructLayout(LayoutKind.Sequential)]` and carry their flavor attribute with `FromType` flags; the only exceptions are the `ref struct` interpolated-string handlers. Native wrappers are `readonly struct`; owning `Unsafe*` collections are not, because their capacity and count change in place.
- Every member, including private and internal ones, has XML documentation. Remarks state capacity, lifetime, ordering, and invariants; `<exception>` tags list thrown exceptions. Comments must not restate the code.
- Use `Unsafe.SizeOf<T>()` or `sizeof(T)` for unmanaged element size and stride; never `Marshal.SizeOf`.
- Use the numeric conversion helpers (`I32`, `U32`, `Usize`, `Isize`, and the sized variants) for size and index conversions. Signedness or width changes go through explicit `From*`/`To*` conversions, or `*Unchecked` only after a proven range check. Do not add ad-hoc casts or implicit conversions.
- Access memory only through `Unsafe`, `UnsafeHelpers`, `MemoryMarshalHelpers`, and `SpanHelpers`. Do not dereference, index, or perform arithmetic on raw pointers. `CodeAnalysis` enforces this rule.
- The build enables `CheckForOverflowUnderflow`. Use checked arithmetic for invariants, `WrappingAdd`/`WrappingSub`/`WrappingMul`/`WrappingNeg` for intentional wrap, and no ad-hoc `unchecked` blocks outside conversion or wrapping helpers.
- `[MustBePinned]` marks a constructor, method, property, or parameter whose value must be pinned so the GC cannot relocate it. `[MustBeDistinct]` marks a constructor or parameter that must receive distinct values. `[MustBeZeroed]` marks a constructor or parameter that must be zero-initialized. These marker attributes document intent; nothing in the library enforces them at compile time or run time.
- All target frameworks must compile: `net10.0`, `net9.0`, `net8.0`, `net7.0`, `net6.0`, `net5.0`, and `netstandard2.1` (C# 9). Guard newer APIs with `NET*_0_OR_GREATER`.
- `MethodImplOptions.AggressiveInlining` marks hot forwarding and leaf members; `NoInlining` marks throw, allocation, and slow path helpers. Change inlining only with benchmark evidence.
- There are no third-party dependencies. `netstandard2.1` alone references `System.Runtime.CompilerServices.Unsafe`.

## Contributing

1. Open an issue with the target framework, a minimal repro, expected behavior, and actual behavior.
2. Keep one logical change per commit or pull request. Do not mix formatting, refactoring, and behavior changes.
3. Preserve public API, ABI, struct layout, ownership, capacity, and exception semantics. Treat changes to them as breaking changes and obtain explicit approval first.
4. Document new or changed members, and update remarks whenever behavior, capacity, or lifetime changes.
5. Satisfy the `CodeAnalysis` rules and compile every target framework. For concurrent types, provide a multithreaded stress repro and run it on more than one target framework; the solution has no test project, so keep that repro outside the repository.
6. Remove generated files and run `git diff --check` before submitting.
7. Report the exact commands run, the target frameworks exercised, and the repro result before and after the change.

## License

This project is licensed under the MIT License.

## Rules

1. Keep the flavor contracts intact: owning `Native*` and `Unsafe*` values free their resource exactly once; views and `Stackalloc*` never free.
2. Never add ownership tracking, reference counting, or automatic skipping of frees beyond the existing `GCHandle` validity check.
3. Never catch, recover from, clean up after, or retry an allocation failure. `OutOfMemoryException` is a terminal contract, not a handled state.
4. Never convert a documented `false` result (capacity, absence, or `Try` failure) into an exception.
5. Never add locks, atomic state, or concurrency fallbacks to single-threaded types. Concurrent misuse is a caller error.
6. Preserve sizing semantics exactly: `NativeArray<T>.Length` is the fixed buffer length; `UnsafeStringBuilder` keeps logical length and capacity separate; `TrimExcess` keeps its documented behavior.
7. Preserve unchecked indexers and their documented valid ranges. Do not add hidden bounds checks.
8. Preserve version and enumerator invalidation semantics precisely; do not add or remove version bumps.
9. Preserve bitwise equality and hashing. Zero padding before hashing or comparing structs that contain it.
10. Access pointers only through `Unsafe` helpers. Never add raw pointer arithmetic, indexing, or dereferencing.
11. Convert numeric types only through the conversion helpers, with explicit checked or intentionally unchecked conversions.
12. Use checked arithmetic unless wrap is intentional; express intentional wrap only through the `Wrapping*` helpers.
13. Document every member: summary, type parameters, parameters, return value, exceptions, and remarks for capacity, lifetime, ordering, and invariants.
14. Keep `Native*`, `Unsafe*`, and `Stackalloc*` APIs aligned. A missing member or a behavioral difference must be a documented design decision.
15. Compile and verify all seven target frameworks and every affected conditional-compilation branch.
16. Do not add dependencies, change the public API, alter layout, or change exception behavior without explicit approval.
17. Do not mix unrelated cleanup, formatting, naming, comments, or warnings into a behavior fix.
18. Do not copy BCL behavior blindly. Match only the semantics this type documents, and record deliberate deviations in its remarks.
19. Prove a bug with a minimal repro before changing semantics; state the observed result before and after.
20. Ship no generated artifacts. Run `git diff --check` and review the exact diff before every commit.
