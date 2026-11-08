using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a thread-local random number generator implementation
    ///     that may be used concurrently from any thread.
    /// </summary>
    /// <typeparam name="TRandom">The type of the underlying random number generator implementation.</typeparam>
    internal static class ThreadSafeRandom<TRandom> where TRandom : unmanaged, IIsCreated, IRandom
    {
        /// <summary>
        ///     The underlying generator implementation.
        /// </summary>
        [ThreadStatic] private static TRandom _impl;

        /// <summary>
        ///     Ensures the random is initialized, and returns a reference to it.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>The pseudo-random number generator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref TRandom EnsureInitialized(ref TRandom random)
        {
            if (!random.IsCreated)
                RandomHelpers.Initialize(ref random);

            return ref random;
        }

        /// <summary>
        ///     Gets a reference to the underlying generator implementation,
        ///     initializing it if necessary.
        /// </summary>
        /// <returns>A reference to the initialized generator implementation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TRandom GetLocalRandomRef() => ref EnsureInitialized(ref _impl);
    }
}