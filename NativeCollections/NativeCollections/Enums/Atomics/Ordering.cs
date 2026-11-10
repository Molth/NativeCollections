// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Atomic memory orderings.
    /// </summary>
    /// <remarks>
    ///     Memory orderings specify the way atomic operations synchronize memory. In its weakest
    ///     <see cref="Relaxed" />, only the memory directly touched by the operation is synchronized.
    ///     On the other hand, a store-load pair of <see cref="SeqCst" /> operations synchronize other
    ///     memory while additionally preserving a total order of such operations across all threads.
    ///     <br />
    ///     Memory orderings are
    ///     <see href="https://en.cppreference.com/w/cpp/atomic/memory_order">
    ///         the same as those of C++20
    ///     </see>
    ///     .
    /// </remarks>
    public enum Ordering
    {
        /// <summary>
        ///     No ordering constraints, only atomic operations.
        /// </summary>
        /// <remarks>
        ///     Corresponds to <c>memory_order_relaxed</c> in C++20.
        /// </remarks>
        Relaxed,

        /// <summary>
        ///     When coupled with a store, all previous operations become ordered before
        ///     any load of this value with <see cref="Acquire" /> (or stronger) ordering.
        /// </summary>
        /// <remarks>
        ///     In particular, all previous writes become visible to all threads that
        ///     perform an <see cref="Acquire" /> (or stronger) load of this value.
        ///     <br />
        ///     Notice that using this ordering for an operation that combines
        ///     loads and stores leads to a <see cref="Relaxed" /> load operation!
        ///     <br />
        ///     This ordering is only applicable for operations that can perform a store.
        ///     <br />
        ///     Corresponds to <c>memory_order_release</c> in C++20.
        /// </remarks>
        Release,

        /// <summary>
        ///     When coupled with a load, if the loaded value was written by a
        ///     store operation with <see cref="Release" /> (or stronger) ordering,
        ///     then all subsequent operations become ordered after that store.
        /// </summary>
        /// <remarks>
        ///     In particular, all subsequent loads will see data written before the store.
        ///     <br />
        ///     Notice that using this ordering for an operation that combines
        ///     loads and stores leads to a <see cref="Relaxed" /> store operation!
        ///     <br />
        ///     This ordering is only applicable for operations that can perform a load.
        ///     <br />
        ///     Corresponds to <c>memory_order_acquire</c> in C++20.
        /// </remarks>
        Acquire,

        /// <summary>
        ///     Has the effects of both <see cref="Acquire" /> and <see cref="Release" /> together:
        /// </summary>
        /// <remarks>
        ///     For loads it uses <see cref="Acquire" /> ordering. For
        ///     stores it uses the <see cref="Release" /> ordering.
        ///     <br />
        ///     Notice that in the case of <c>compare_and_swap</c>, it is possible that the operation
        ///     ends up not performing any store and hence it has just <see cref="Acquire" /> ordering.
        ///     However, <see cref="AcqRel" /> will never perform <see cref="Relaxed" /> accesses.
        ///     <br />
        ///     Corresponds to <c>memory_order_acq_rel</c> in C++20.
        /// </remarks>
        AcqRel,

        /// <summary>
        ///     Like <see cref="Acquire" />/<see cref="Release" />/<see cref="AcqRel" /> (for load,
        ///     store, and load-with-store operations, respectively) with the additional guarantee
        ///     that all threads see all sequentially consistent operations in the same order.
        /// </summary>
        /// <remarks>
        ///     Corresponds to <c>memory_order_seq_cst</c> in C++20.
        /// </remarks>
        SeqCst
    }
}