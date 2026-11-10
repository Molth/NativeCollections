# NativeCollections

[![NuGet](https://img.shields.io/nuget/v/NativeCollections.svg)](https://www.nuget.org/packages/NativeCollections)

Pure C# collections, allocators, atomics, text/I/O types, and concurrency primitives. Types use unmanaged storage where their contract says so and expose ownership, capacity, lifetime, pinning, and concurrency requirements explicitly.

This is a compact contract summary. It is not a usage tutorial, example catalog, benchmark report, or replacement for the current public API/XML documentation. When this file is insufficient, read the exact public member and then its implementation; never choose a convenient interpretation or invent missing behavior.

## Scope and design goals

- Predictable unmanaged storage and explicit single ownership.
- Low managed allocation on documented hot paths.
- BCL-like APIs where useful, without claiming undocumented BCL parity.
- Explicit results, exceptions, capacity, overflow, layout, and lifetime semantics.
- One source evaluated across every declared target framework.
- Unsafe code that can be reviewed through helpers, analyzers, tests, and source evidence.

The library does not make unsafe memory safe by default, add transparent ownership tracking, or make ordinary collections thread-safe. These are design boundaries, not missing convenience features.

## Storage and ownership

| Flavor        | Storage model                                                                                                                         | Ownership                                                |
|---------------|---------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------|
| `Unsafe*`     | Direct buffer/pointer state or a handle to a wrapped value.                                                                           | The owning value; dispose exactly once.                  |
| `Native*`     | Usually a native wrapper around an `Unsafe*`, `GCHandle`, or directly owned buffer. The exact type documentation controls exceptions. | The owning value unless documented as a view/scope.      |
| `Stackalloc*` | Caller-provided fixed storage.                                                                                                        | The caller; never allocate, free, or grow it.            |
| Support types | Allocator, hashing, numeric, bitwise, random, or helper logic.                                                                        | No ownership merely because a name starts with `Native`. |

Hard rules:

- Copying an owning struct copies its pointer or handle, not ownership. Keep one owner and dispose it exactly once; there is no implicit move, reference count, finalizer, or idempotent disposal.
- Double-free, freeing another owner's pointer, disposing a non-owner, using a disposed value, or using a dangling view is caller misuse with undefined behavior.
- `GCHandle` wrappers such as `NativeObject<T>`, `NativeMemory<T>`, and `NativeArrayRef<T>` do not provide universal double-dispose protection. `IsCreated` observes stored state; it is not a lifetime guard.
- `From(Span<T>)`, `FromBytes`, `Slice`, `AsSpan`, and `AsReadOnlySpan` normally create non-owning views. Never free a view or an interior pointer. A `From` factory may transfer ownership only when its documentation says so.
- `[MustBePinned]`, `[MustBeDistinct]`, and `[MustBeZeroed]` are caller preconditions. The attributes do not enforce themselves.
- Do not add `_disposed`, `_ownsPtr`, `_ownsHandle`, hidden reference counting, `SafeHandle`, finalizers, or locks as generic safety patches. Such changes alter layout, ABI, allocation cost, copy semantics, concurrency, and the public contract.
- State fields are valid when they are part of a documented allocator, pool, free-list, CAS, or other algorithmic state machine. They are not valid merely to hide ownership mistakes.

Do not import managed ownership patterns into an unmanaged owning struct. A copied `_disposed` or `_owns*` field is copied with the pointer or handle, so different struct copies can disagree about the same resource. Updating one copy cannot invalidate the other copy. Such fields do not protect interior pointers, refs, spans, slices, or other views either. They create a false impression of safety while adding branches, bytes, and an undocumented state machine.

Use a reference type, `SafeHandle`, reference counting, a finalizer, or runtime disposal guards only when the type's explicit design requires that ownership model and explicitly accepts its allocation, synchronization, layout, and lifetime costs. Never introduce one as a generic AI safety patch. Debug-only poisoning or assertions may be used when explicitly scoped and when Release behavior remains unchanged.

## Construction and factory semantics

Factory names describe the source and ownership of a value. They are not interchangeable aliases:

- `Create(...)` creates or allocates a value or resource, or performs a documented initialization operation with creation semantics.
- `From(...)` accepts external input such as a pointer, handle, span, array, or existing object. It normally creates a view and may transfer ownership only when documented.
- `FromBytes(...)` accepts external byte input. It is not a generic name for an allocating factory.
- `New()` is only for a deliberately exposed no-argument default-state factory. It is not a universal replacement for `Create()` and must not be added merely to eliminate a `default` expression.

Do not add a factory to every struct. Decide from the type's actual design, constructor semantics, ownership model, public contract, and complete call chain. `NativeArray<T>` must not gain a zero-argument `Create()` or `New()` merely because a default value appears in source; its default state and parameterized allocation factories are distinct APIs. Do not invent a capacity, allocation, ownership transition, or extra overload to make a call site look uniform.

A public `New()` may be appropriate when a type intentionally exposes its default state as the uniform public entry point. That is type-specific, not mechanical. Do not add a zero-argument factory that only forwards through another factory and a private constructor, and never create recursive factories such as `New() => Type.New()`. `new T()` and `default(T)` are not assumed equivalent without Roslyn and implementation evidence.

## Behavioral contract

### Capacity, memory, and indexing

- `Count` is logical element count and `Capacity` is allocated element storage unless a type documents another unit.
- `NativeArray<T>.Length`/`Count` is fixed buffer length; it is not growable.
- `UnsafeStringBuilder<T>.Length`/`Count` is logical content length. `Capacity` is allocated capacity, `Buffer` is the full storage, and `Text` is logical content.
- `Stackalloc*` cannot grow caller storage. Capacity failure uses the member's documented `false` or `InsertResult`, never hidden allocation.
- Indexers are unchecked unless explicitly documented otherwise. Invalid unchecked access may read/write invalid memory; it is not expected to throw.
- Size, stride, offset, capacity, and index arithmetic is checked by default. Intentional wrapping must use the project's wrapping helpers.

### Results and exceptions

| Situation                                            | Contract-level interpretation                                                                           |
|------------------------------------------------------|---------------------------------------------------------------------------------------------------------|
| Normal absence, duplicate, or fixed-capacity failure | Documented `false` or `InsertResult`; do not turn it into an exception.                                 |
| Invalid argument or range                            | Documented argument exception; do not turn it into `false`.                                             |
| Invalid state                                        | Documented invalid-state exception, commonly `InvalidOperationException`.                               |
| Unsupported operation/interface                      | Documented `NotSupportedException` path.                                                                |
| Allocation failure                                   | `OutOfMemoryException` or configured allocator failure; terminal for the operation, not a retry signal. |

The exact member documentation controls the type, trigger, and timing. Do not catch, silently recover from, or reinterpret allocation failure. Do not change normal `false` results into exceptions or exceptions into normal results.

### Enumeration, concurrency, and atomics

- Enumerators may retain a pointer to their source. Moving, resizing, disposing, or otherwise invalidating the source during enumeration is undefined behavior unless the member explicitly guarantees otherwise.
- Version-bumping mutations invalidate enumerators. The exact version behavior is type-specific and must not be normalized by analogy with another collection.
- Ordinary collections are not synchronized. Cross-thread access is invalid unless the public type/member documentation explicitly permits it.
- Names such as `*Concurrent*`, `*TreiberStack*`, `*SegQueue*`, `*ArrayQueue*`, sharded collections, locks, and epoch types are navigation hints, not proof of safe usage. Read the exact API contract.
- Atomic values require a stable address for their complete use interval. Pin them or store them in unmanaged memory as required; a pointer/ref must not outlive its pin or stack scope, and an atomic must not be copied while in use.
- Atomic ordering is part of the contract. Invalid Load/Store ordering combinations must be rejected as documented, not silently weakened.
- Concurrent `Count` may be stale. Use the documented decision property, such as `IsEmpty`, for concurrency-sensitive decisions.

### Layout, equality, and global hooks

- Public API, ABI, struct layout, field order, generic constraints, implicit conversions, ownership, capacity, exception/result behavior, and thread-safety are compatibility-sensitive. Changes require explicit scope.
- Where documented, equality and hashing include the complete representation, including padding. `[MustBeZeroed]` inputs must satisfy that precondition.
- `NativeMemoryAllocator.Custom`, `NativeHashCode.Custom`, and `UnsafeString.Custom` are process-wide hooks. Install them before dependent types are used, make callbacks thread-safe, and do not re-enter code using the same hook.

## Frameworks and repository boundaries

The production project is `NativeCollections\NativeCollections.csproj` and currently targets:

```text
net10.0; net9.0; net8.0; net7.0; net6.0; net5.0; netstandard2.1
```

It enables nullable annotations, unsafe blocks, and `CheckForOverflowUnderflow`. `netstandard2.1` uses C# 9 and references `System.Runtime.CompilerServices.Unsafe` 6.0.0. `CodeAnalysis\CodeAnalysis.csproj` is a separate Roslyn analyzer/code-fix project and is not evidence of production runtime safety. Release packaging writes `nupkgs` under the project directory; generated package/build outputs are git-ignored and are not part of the source contract.

The main workspace intentionally contains no usage-example catalog, performance suite, test factory, fuzz corpus, or conventional unit-test project. External tests and reproductions are evidence for recorded conditions, not official usage patterns or permanent source artifacts.

## Usage and optimization rules

1. Read the complete current public API/XML documentation before writing usage code or claiming usage evidence: constructors, factories, parameters, constraints, attributes, lifetime, capacity, results, exceptions, threading, and target-framework conditions.
2. Old snippets, copied tests, historical reports, generated artifacts, blogs, and model knowledge are not contracts. Do not use them as the implementation or proof of behavior.
3. If the public API does not establish safe usage, read the implementation, helpers, mirror types, conditional branches, and call chain. If they still do not establish it, report a contract gap; do not invent an algorithm or usage recipe.
4. External performance tests are local measurements for one commit, TFM, compiler/runtime, CPU, allocator, workload, and configuration. They are not universal performance evidence.
5. "High performance", fewer allocations, less duplication, shorter code, reflection, hidden dispatch, incidental layout, JIT assumptions, or an external dependency never justifies a semantic or safety regression.
6. An optimization requires algorithm/complexity analysis, memory/lifetime/overflow/concurrency analysis, controlled before/after measurement, and correctness regression tests covering boundaries, random/state transitions, mirrors, and affected TFMs.
7. Preserve the simplest correct implementation when an improvement is not material and repeatable. Similar source text does not prove equivalent semantics.

Under no circumstances may an implementation detail be guessed. If the source, Roslyn semantic model, data/control-flow analysis, call chain, conditional branch, or reproducible experiment does not establish a fact, mark it unknown or unverified. Do not fill the gap with BCL behavior, naming, directory layout, a familiar design pattern, an old report, or an assumed allocation, ownership, lifetime, overflow, or threading rule.

## Verification status and evidence limits

The project has received multiple rounds of full-scope verification in external shadow copies, including all functions and members in the audited source set, all declared TFMs and relevant conditional branches, reachability/call-chain analysis, ownership/lifetime paths, and millions of generated, randomized, boundary, state-transition, and cross-implementation cases. The absence of test projects from the production checkout is intentional isolation, not evidence that the project was never tested. This statement describes prior verification of recorded snapshots; it is not a certificate for every future commit or environment.

- A successful build proves compilation only, not ownership, lifetime, concurrency, layout, overflow, or semantic correctness.
- A static diagnostic is a candidate. `confirmed_bug` requires an old-code minimal reproduction, observable failure, expected behavior, and proof that the result is not design behavior, caller misuse, OOM, or a harness failure.
- A "no finding" result means no finding under the recorded scope and evidence, not proof that every possible defect is impossible.
- Every run records commit, per-TFM MSBuild Compile set, SDK/runtime, commands, source-copy hashes, seeds, results, tool failures, and uncovered scope.

## CI and external isolation

GitHub Actions and automatic tests must run in a fresh shadow project or isolated CI workspace. They must not write test factories, generated cases, fuzz seeds, repro projects, logs, reports, `bin`, `obj`, or `nupkgs` into the production checkout. This prevents CI noise, AI misclassification of test factories as production code, and reuse of stale conclusions.

Use a unique UTC-timestamped name for every run, branch, artifact, and report set:

```text
NativeCollections-<purpose>-<UTC-YYYYMMDD-HHMMSS>-<6-char-random>
```

Never use `latest`, `test`, `new`, `final`, or `backup` as the sole identifier. Each run records `run_id`, purpose, audited commit, parent run or `none`, start time, toolchain, shadow root, and output paths. Never overwrite or merge runs.

## AI audit rules

AI testing is read-only by default. The AI acts as an orchestrator; subagents review disjoint ownership partitions. Testing permission does not grant repair permission.

1. Record HEAD, branch, status, SDK, project settings, and hashes of existing user modifications. Do not alter them.
2. Create a new external run directory before analysis. Do not read or reuse old audit directories, copied workspaces, reports, test factories, logs, or conclusions.
3. Derive the file scope separately for every TFM from real MSBuild `Compile` items. Every item has exactly one owner; no duplicates or omissions. Review themes may cover Native collections, Stackalloc collections, Unsafe collections, text/I/O, atomics/concurrency, pools/handles/memory, helpers, Community ports, and support types, but the actual manifest comes from MSBuild.
4. Build and test only the SHA-256-verified shadow copy. Obtain real per-TFM compiler inputs; do not concatenate files into an artificial compilation.
5. Each reviewer reads this contract and the complete public API before deriving behavior, then reads implementation/call chains where necessary. Review every assigned member and relevant boundary, not only search matches.
6. Keep analyzer diagnostics, suspected findings, design behavior, caller misuse, OOM exclusions, documentation gaps, and confirmed defects separate. Diagnostics never directly authorize a rewrite.
7. A confirmed defect requires reproducible old-code failure. Reports include owner, file/line, TFM/branch, invariant, evidence, consequence, reproduction, expected result, and uncovered scope.
8. If repair is separately authorized, preserve the baseline, make one minimal change at a time, rerun the reproduction and affected TFMs, check mirrors/API/layout/ownership, compare user-file hashes, and run `git diff --check`.

The analyzer lead must use real per-TFM MSBuild compiler inputs rather than one artificial compilation. Analyzer rules require synthetic self-tests that prove they can emit diagnostics; zero production hits must be explained. A code fix requires synthetic bad code, application of the fix, recompilation, and verification that no same-rule diagnostic remains. Analyzer output is candidate evidence only.

At minimum, the audit should consider control flow after `ThrowHelpers`, `throw new` policy, pointer/ref escape, allocation/free pairing, numeric-width and overflow behavior, checked/unchecked differences, `Span`/`MemoryMarshal` boundaries, atomics and CAS loops, Count/Capacity invariants, disposal paths, conditional compilation, public API contract mismatches, cross-file ownership flow, and Native/Unsafe/Stackalloc mirror differences.

When a tool fails, record the complete command and error, use a read-only alternative, reduce scope to a TFM/file/member or conditional branch when possible, and report the uncovered scope. Never fabricate a reproduction, mark an unverified issue as confirmed, or silently omit a failed partition.

External audit output must include a fresh run ledger, per-TFM scope and ownership matrix, coverage report, analyzer logs and diagnostics, reviewer reports and findings, reproductions where applicable, tool-failure records, and a final report that separates current-run evidence from historical background. No audit artifact belongs in the production checkout.

## Source and contribution rules

- Preserve public API, ABI, layout, ownership, capacity, exception/result, version, and thread-safety semantics.
- Do not change public constructors, attribute constructors, compiler-required constructors, or add public factories as a side effect of a mechanical rewrite. Any such API change requires explicit scope and a complete call-site review.
- Use `Unsafe.SizeOf<T>()` for unmanaged sizes and the project's numeric conversion helpers for width and signedness.
- Access native memory through `Unsafe`, `UnsafeHelpers`, `MemoryMarshalHelpers`, and `SpanHelpers`; do not add ad-hoc pointer arithmetic or indexing.
- Use checked arithmetic for invariants and wrapping helpers for intentional machine-width wrap.
- Keep Native, Unsafe, and Stackalloc mirrors aligned only when their contracts are intended to align; document deliberate differences.
- Keep formatting, refactoring, renaming, warning cleanup, generated files, and unrelated optimization out of behavior fixes.
- Never infer an implementation detail from a name, pattern, or similar type. Read the source and call chain or record the point as unverified.
- Compile all seven TFMs and affected conditional branches, record exact commands/results, and run `git diff --check` before submission.

## License

This project is licensed under the MIT License.
