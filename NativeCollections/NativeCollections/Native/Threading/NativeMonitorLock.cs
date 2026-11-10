using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a reference to an object pinned in memory using a <see cref="GCHandle" />.
    ///     Provides a mechanism that synchronizes access to this object.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(Monitor))]
    public readonly struct NativeMonitorLock : IIsCreated, IDisposable, IEquatable<NativeMonitorLock>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeMonitorLock(GCHandle handle) => _handle = handle;

        /// <summary>
        ///     Creates a monitor lock backed by a new object and GC handle.
        /// </summary>
        /// <param name="type">The type of GC handle to create.</param>
        /// <returns>A new monitor lock.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMonitorLock Create(GCHandleType type) => new(GCHandle.Alloc(new object(), type));

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsAllocated;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeMonitorLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeMonitorLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeMonitorLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeMonitorLock left, NativeMonitorLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeMonitorLock left, NativeMonitorLock right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;

            handle.Free();
        }

        /// <summary>
        ///     Enters the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter() => Monitor.Enter(_handle.Target!);

        /// <summary>
        ///     Acquires an exclusive lock on the specified object, and
        ///     atomically sets a value that indicates whether the lock was taken.
        /// </summary>
        /// <param name="lockTaken">
        ///     The result of the attempt to acquire the lock, passed by reference. The input must
        ///     be <see langword="false" />. The output is <see langword="true" /> if the lock
        ///     is acquired; otherwise, the output is <see langword="false" />. The output is set
        ///     even if an exception occurs during the attempt to acquire the lock. Note: If no
        ///     exception occurs, the output of this method is always <see langword="true" />.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="lockTaken" /> is not <see langword="false" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(ref bool lockTaken) => Monitor.Enter(_handle.Target!, ref lockTaken);

        /// <summary>
        ///     Attempts to acquire an exclusive lock on the specified object.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the lock was acquired; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter() => Monitor.TryEnter(_handle.Target!);

        /// <summary>
        ///     Attempts to acquire an exclusive lock on the specified object, and
        ///     atomically sets a value that indicates whether the lock was taken.
        /// </summary>
        /// <param name="lockTaken">
        ///     The result of the attempt to acquire the lock, passed by reference. The input
        ///     must be <see langword="false" />. The output is <see langword="true" /> if the
        ///     lock is acquired; otherwise, the output is <see langword="false" />. The output
        ///     is set even if an exception occurs during the attempt to acquire the lock.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryEnter(ref bool lockTaken) => Monitor.TryEnter(_handle.Target!, ref lockTaken);

        /// <summary>
        ///     Attempts, for the specified number of milliseconds,
        ///     to acquire an exclusive lock on the specified object.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for the lock.</param>
        /// <returns>
        ///     <see langword="true" /> if the lock was acquired; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="millisecondsTimeout" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter(int millisecondsTimeout) => Monitor.TryEnter(_handle.Target!, millisecondsTimeout);

        /// <summary>
        ///     Attempts, for the specified number of milliseconds, to acquire an exclusive lock on the
        ///     specified object, and atomically sets a value that indicates whether the lock was taken.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for the lock.</param>
        /// <param name="lockTaken">
        ///     The result of the attempt to acquire the lock, passed by reference. The input
        ///     must be <see langword="false" />. The output is <see langword="true" /> if the
        ///     lock is acquired; otherwise, the output is <see langword="false" />. The output
        ///     is set even if an exception occurs during the attempt to acquire the lock.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="millisecondsTimeout" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryEnter(int millisecondsTimeout, ref bool lockTaken) => Monitor.TryEnter(_handle.Target!, millisecondsTimeout, ref lockTaken);

        /// <summary>
        ///     Determines whether the current thread holds the lock on the specified object.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the current thread holds the lock; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEntered() => Monitor.IsEntered(_handle.Target!);

        /// <summary>
        ///     Releases the lock on an object and blocks the current thread until it reacquires the
        ///     lock. If the specified time-out interval elapses, the thread enters the ready queue.
        /// </summary>
        /// <param name="millisecondsTimeout">
        ///     The number of milliseconds to wait before the thread enters the ready queue.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the lock was reacquired; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="millisecondsTimeout" /> is less than -1.
        /// </exception>
        /// <exception cref="SynchronizationLockException">
        ///     Thrown when the current thread does not own the lock.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(int millisecondsTimeout) => Monitor.Wait(_handle.Target!, millisecondsTimeout);

        /// <summary>
        ///     Notifies a thread in the waiting queue of a change in the locked object's state.
        /// </summary>
        /// <exception cref="SynchronizationLockException">
        ///     Thrown when the current thread does not own the lock.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pulse() => Monitor.Pulse(_handle.Target!);

        /// <summary>
        ///     Notifies all waiting threads of a change in the object's state.
        /// </summary>
        /// <exception cref="SynchronizationLockException">
        ///     Thrown when the current thread does not own the lock.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PulseAll() => Monitor.PulseAll(_handle.Target!);

        /// <summary>
        ///     Attempts, for the specified amount of time, to acquire an exclusive lock on the specified object.
        /// </summary>
        /// <param name="timeout">
        ///     A <see cref="TimeSpan" /> representing the amount of time to wait
        ///     for the lock. A value of -1 millisecond specifies an infinite wait.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the lock was acquired; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the total milliseconds of <paramref name="timeout" /> are less than -1 or greater than
        ///     <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter(TimeSpan timeout) => Monitor.TryEnter(_handle.Target!, timeout);

        /// <summary>
        ///     Attempts, for the specified amount of time, to acquire an exclusive lock on the
        ///     specified object, and atomically sets a value that indicates whether the lock was taken.
        /// </summary>
        /// <param name="timeout">
        ///     The amount of time to wait for the lock. A value of -1 millisecond specifies an infinite wait.
        /// </param>
        /// <param name="lockTaken">
        ///     The result of the attempt to acquire the lock, passed by reference. The input
        ///     must be <see langword="false" />. The output is <see langword="true" /> if the
        ///     lock is acquired; otherwise, the output is <see langword="false" />. The output
        ///     is set even if an exception occurs during the attempt to acquire the lock.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the total milliseconds of <paramref name="timeout" /> are less than -1 or greater than
        ///     <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryEnter(TimeSpan timeout, ref bool lockTaken) => Monitor.TryEnter(_handle.Target!, timeout, ref lockTaken);

        /// <summary>
        ///     Releases the lock on an object and blocks the current thread until it reacquires the
        ///     lock. If the specified time-out interval elapses, the thread enters the ready queue.
        /// </summary>
        /// <param name="timeout">
        ///     A <see cref="TimeSpan" /> representing the amount of
        ///     time to wait before the thread enters the ready queue.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the lock was reacquired; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the total milliseconds of <paramref name="timeout" /> are less than -1 or greater than
        ///     <see cref="int.MaxValue" />.
        /// </exception>
        /// <exception cref="SynchronizationLockException">
        ///     Thrown when the current thread does not own the lock.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(TimeSpan timeout) => Monitor.Wait(_handle.Target!, timeout);

        /// <summary>
        ///     Releases the lock on an object and blocks the current thread until it reacquires the lock.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the lock was reacquired; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="SynchronizationLockException">
        ///     Thrown when the current thread does not own the lock.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait() => Monitor.Wait(_handle.Target!);

        /// <summary>
        ///     Releases the lock on an object and blocks the current thread until it reacquires
        ///     the lock. If the specified time-out interval elapses, the thread enters the ready
        ///     queue. This method also specifies whether the synchronization domain for the context
        ///     (if in a synchronized context) is exited before the wait and reacquired afterward.
        /// </summary>
        /// <param name="millisecondsTimeout">
        ///     The number of milliseconds to wait before the thread enters the ready queue.
        /// </param>
        /// <param name="exitContext">
        ///     <see langword="true" /> to exit and reacquire the synchronization domain for the context
        ///     (if in a synchronized context) before the wait; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the lock was reacquired; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="millisecondsTimeout" /> is less than -1.
        /// </exception>
        /// <exception cref="SynchronizationLockException">
        ///     Thrown when the current thread does not own the lock.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(int millisecondsTimeout, bool exitContext) => Monitor.Wait(_handle.Target!, millisecondsTimeout, exitContext);

        /// <summary>
        ///     Releases the lock on an object and blocks the current thread until it reacquires the lock. If
        ///     the specified time-out interval elapses, the thread enters the ready queue. Optionally exits the
        ///     synchronization domain for the synchronized context before the wait and reacquires the domain afterward.
        /// </summary>
        /// <param name="timeout">
        ///     A <see cref="TimeSpan" /> representing the amount of
        ///     time to wait before the thread enters the ready queue.
        /// </param>
        /// <param name="exitContext">
        ///     <see langword="true" /> to exit and reacquire the synchronization domain for the context
        ///     (if in a synchronized context) before the wait; otherwise, <see langword="false" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the lock was reacquired; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the total milliseconds of <paramref name="timeout" /> are less than -1 or greater than
        ///     <see cref="int.MaxValue" />.
        /// </exception>
        /// <exception cref="SynchronizationLockException">
        ///     Thrown when the current thread does not own the lock.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(TimeSpan timeout, bool exitContext) => Monitor.Wait(_handle.Target!, timeout, exitContext);

        /// <summary>
        ///     Exits the lock.
        /// </summary>
        /// <exception cref="SynchronizationLockException">
        ///     Thrown when the current thread does not own the lock.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Monitor.Exit(_handle.Target!);
    }
}