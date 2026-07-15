// ReSharper disable ALL

namespace NativeCollections
{
    /// Atomic memory orderings
    /// <br />
    /// Memory orderings specify the way atomic operations synchronize memory.
    /// In its weakest [`Ordering::Relaxed`], only the memory directly touched by the
    /// operation is synchronized. On the other hand, a store-load pair of [`Ordering::SeqCst`]
    /// operations synchronize other memory while additionally preserving a total order of such
    /// operations across all threads.
    /// <br />
    /// Rust's memory orderings are [the same as those of
    /// C++20](https://en.cppreference.com/w/cpp/atomic/memory_order).
    /// <br />
    /// For more information see the [nomicon].
    /// <br />
    /// [nomicon]: ../../../nomicon/atomics.html
    public enum Ordering
    {
        /// No ordering constraints, only atomic operations.
        /// <br />
        /// Corresponds to [`memory_order_relaxed`] in C++20.
        /// <br />
        /// [`memory_order_relaxed`]: https://en.cppreference.com/w/cpp/atomic/memory_order#Relaxed_ordering
        Relaxed,

        /// When coupled with a store, all previous operations become ordered
        /// before any load of this value with [`Acquire`] (or stronger) ordering.
        /// In particular, all previous writes become visible to all threads
        /// that perform an [`Acquire`] (or stronger) load of this value.
        /// <br />
        /// Notice that using this ordering for an operation that combines loads
        /// and stores leads to a [`Relaxed`] load operation!
        /// <br />
        /// This ordering is only applicable for operations that can perform a store.
        /// <br />
        /// Corresponds to [`memory_order_release`] in C++20.
        /// <br />
        /// [`memory_order_release`]: https://en.cppreference.com/w/cpp/atomic/memory_order#Release-Acquire_ordering
        Release,

        /// When coupled with a load, if the loaded value was written by a store operation with
        /// [`Release`] (or stronger) ordering, then all subsequent operations
        /// become ordered after that store. In particular, all subsequent loads will see data
        /// written before the store.
        /// <br />
        /// Notice that using this ordering for an operation that combines loads
        /// and stores leads to a [`Relaxed`] store operation!
        /// <br />
        /// This ordering is only applicable for operations that can perform a load.
        /// <br />
        /// Corresponds to [`memory_order_acquire`] in C++20.
        /// <br />
        /// [`memory_order_acquire`]: https://en.cppreference.com/w/cpp/atomic/memory_order#Release-Acquire_ordering
        Acquire,

        /// Has the effects of both [`Acquire`] and [`Release`] together:
        /// For loads it uses [`Acquire`] ordering. For stores it uses the [`Release`] ordering.
        /// <br />
        /// Notice that in the case of `compare_and_swap`, it is possible that the operation ends up
        /// not performing any store and hence it has just [`Acquire`] ordering. However,
        /// `AcqRel` will never perform [`Relaxed`] accesses.
        /// <br />
        /// This ordering is only applicable for operations that combine both loads and stores.
        /// <br />
        /// Corresponds to [`memory_order_acq_rel`] in C++20.
        /// <br />
        /// [`memory_order_acq_rel`]: https://en.cppreference.com/w/cpp/atomic/memory_order#Release-Acquire_ordering
        AcqRel,

        /// Like [`Acquire`]/[`Release`]/[`AcqRel`] (for load, store, and load-with-store
        /// operations, respectively) with the additional guarantee that all threads see all
        /// sequentially consistent operations in the same order.
        /// <br />
        /// Corresponds to [`memory_order_seq_cst`] in C++20.
        /// <br />
        /// [`memory_order_seq_cst`]: https://en.cppreference.com/w/cpp/atomic/memory_order#Sequentially-consistent_ordering
        SeqCst
    }
}