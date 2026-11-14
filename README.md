# NativeCollections

[![NuGet](https://img.shields.io/nuget/v/NativeCollections.svg)](https://www.nuget.org/packages/NativeCollections)

NativeCollections is a C# library for unmanaged collections, caller-backed collections, allocators and pools, text and binary I/O, atomics, and concurrent data structures. Its design makes storage, ownership, capacity, and lifetime explicit and limits managed allocations on documented hot paths.

The public API and its XML documentation define each member's contract. When they leave behavior unclear, trace the current implementation, its helpers and call chain, and the applicable target-specific branches. Record any unresolved contract gap.

## Storage and ownership

| Family        | Storage model                                                                                                                                                           |
|---------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Native*`     | Collection and resource wrappers over native storage, implementation state, or managed handles. The individual member determines whether it returns an owner or a view. |
| `Unsafe*`     | Lower-level types that expose or directly manage buffers, pointers, or implementation state. Ownership and caller obligations depend on the member.                     |
| `Stackalloc*` | Fixed-capacity collections over caller-provided storage. The caller controls the buffer's lifetime.                                                                     |

- Keep one logical owner for each allocation or handle and release it exactly once. Struct copies can refer to the same resource. Disposal through one copy leaves aliases dangling. Treat `IsCreated` as an observation of the queried value's stored state.
- Release resources through the owning value and its matching allocator or handle mechanism. Keep borrowed pointers, references, slices, and spans within the lifetime of the source and any required pin. A resize or other documented invalidation can end a view's lifetime before disposal.
- Honor `MustBePinned`, `MustBeDistinct`, `MustNotSelfAlias`, and `MustBeZeroed` where they apply. Keep the required address stable throughout its use and include padding when the contract requires a fully zeroed representation.

## Construction and behavior

- **Initialization.** `Create` performs the documented creation or initialization operation. `From` takes existing input and `FromBytes` takes byte storage. Ownership, allocation, and pinning requirements belong to the overload. Use the `default` literal for a deliberate zero-initialized state. Verify constructor semantics and the target type before rewriting a struct initialization.
- **Capacity and indexing.** `Count` describes logical occupancy and `Capacity` describes storage limits, in the units documented by the type. Fixed-capacity collections retain their backing storage and report exhaustion through the member's result contract. Bounds checks are member-specific. Unchecked access requires a valid index.
- **Results and exceptions.** Preserve the documented results for normal outcomes such as absence, duplicates, or insufficient capacity, as well as the exceptions for invalid arguments, invalid state, unsupported operations, overflow, and allocation failure. Propagate allocator failure according to its contract.
- **Enumeration.** An enumerator may hold a pointer to the source or track a mutation version. Keep the source and its storage valid throughout enumeration and follow the member's invalidation rules.
- **Concurrency.** Ordinary collections require caller synchronization when shared. Concurrent types define which operations may overlap and when disposal is safe. Concurrent observations such as `Count` may be stale. Follow the documented atomic operation and memory ordering, and keep the atomic at a stable address throughout use.
- **Representation.** Where equality or hashing uses the complete representation, padding is part of that contract. Configure the process-wide hooks `NativeMemoryAllocator.Custom`, `NativeHashCode.Custom`, and `UnsafeString.Custom` before dependent use. Callbacks must support the calling threads, avoid re-entering the same hook, and preserve allocation/free pairing.

## Source and contribution rules

The production project, `NativeCollections/NativeCollections.csproj`, targets `net10.0`, `net9.0`, `net8.0`, `net7.0`, `net6.0`, `net5.0`, and `netstandard2.1`. It enables nullable analysis, unsafe code, and checked arithmetic by default. The .NET Standard target uses C# 9 and references `System.Runtime.CompilerServices.Unsafe` 6.0.0. `CodeAnalysis/CodeAnalysis.csproj` contains the separate Roslyn analyzers, code fixes, and source rewriters.

- Preserve public signatures, generic constraints, conversions, ABI and field layout, ownership, capacity, result and exception behavior, enumerator invalidation, and threading guarantees. Changes to constructors and factories require explicit scope and review of affected call sites.
- Design disposal and synchronization around the type's ownership model. Per-copy disposal or ownership flags cannot invalidate other struct copies or borrowed views. Reference counting, finalization, and locking require an explicit design decision with their layout, allocation, and synchronization costs accounted for.
- Use `Unsafe.SizeOf<T>()` for unmanaged element sizes and route native memory access through `Unsafe`, `UnsafeHelpers`, `MemoryMarshalHelpers`, and `SpanHelpers` as appropriate. Use the project's conversion helpers at width and signedness boundaries, checked arithmetic for size and offset invariants, and the wrapping helpers for intentional overflow.
- Review corresponding Native, Unsafe, and Stackalloc implementations where they share a contract. Preserve deliberate differences and keep mechanical cleanup separate from behavior fixes.
- Evaluate optimizations for complexity, allocation, lifetime, overflow, and concurrency. Require repeatable before-and-after measurements under comparable conditions and regression coverage for the affected targets and state transitions.
- For source changes, compile all seven targets, verify the affected conditional branches, record commands and results, and run `git diff --check`.

## Verification and audit

Seven rounds of full-scope verification were run on isolated copies of recorded source snapshots. They covered the members in the audited source sets, all seven target frameworks, relevant conditional branches, call chains, and ownership and lifetime paths. The runs exercised millions of generated cases across random inputs, boundaries, state transitions, and comparisons between implementations. The recorded results apply to their respective snapshots and environments.

1. **Isolate each run.** Run CI and automated audits in a fresh external copy or isolated CI workspace. Use a unique run identifier with a purpose, UTC timestamp, and random suffix. Preserve the original checkout and user changes. Keep test harnesses, generated inputs, reports, reproductions, and build outputs in the isolated workspace.
2. **Establish scope and provenance.** Record the baseline revision, working-tree state, and toolchain. Verify source-copy hashes before building and testing. Derive each target's manifest from evaluated MSBuild `Compile` items and use its actual compiler inputs. For partitioned reviews, assign each item once and track coverage across targets and branches.
3. **Confirm findings.** Review the documented contract and implementation before assigning a failure. A confirmed defect requires a minimal baseline reproduction, a contract-backed expected result, and evidence that rules out caller misuse and harness failure. Exercise analyzer changes with synthetic inputs that trigger their diagnostics. Apply each changed code fix to a failing input, recompile, and check that the targeted diagnostic is gone.
4. **Record evidence.** Keep the run ledger, scope and coverage, commands, seeds, results, reproductions, and tool failures with the run. State any uncovered paths and substantiate conclusions with current-run evidence. A successful build establishes compilation for the configurations built.
5. **Keep audit and repair authorization distinct.** Audit work is read-only by default. For an authorized repair, retain the baseline, make the smallest contract-preserving change, rerun the reproduction and affected targets, and check the final diff.

## License

NativeCollections is licensed under the [MIT License](LICENSE).
