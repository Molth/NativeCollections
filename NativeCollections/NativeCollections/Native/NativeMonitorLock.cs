using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native monitorLock
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(Monitor))]
    public readonly struct NativeMonitorLock : IIsCreated, IDisposable, IEquatable<NativeMonitorLock>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="type">GCHandle type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMonitorLock(GCHandleType type) => _handle = GCHandle.Alloc(new object(), type);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsAllocated;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeMonitorLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeMonitorLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeMonitorLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeMonitorLock left, NativeMonitorLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeMonitorLock left, NativeMonitorLock right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;
            handle.Free();
        }

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter() => Monitor.Enter(_handle.Target!);

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(ref bool lockTaken) => Monitor.Enter(_handle.Target!, ref lockTaken);

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter() => Monitor.TryEnter(_handle.Target!);

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryEnter(ref bool lockTaken) => Monitor.TryEnter(_handle.Target!, ref lockTaken);

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter(int millisecondsTimeout) => Monitor.TryEnter(_handle.Target!, millisecondsTimeout);

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryEnter(int millisecondsTimeout, ref bool lockTaken) => Monitor.TryEnter(_handle.Target!, millisecondsTimeout, ref lockTaken);

        /// <summary>
        ///     Is entered
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEntered() => Monitor.IsEntered(_handle.Target!);

        /// <summary>
        ///     Wait
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(int millisecondsTimeout) => Monitor.Wait(_handle.Target!, millisecondsTimeout);

        /// <summary>
        ///     Pulse
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pulse() => Monitor.Pulse(_handle.Target!);

        /// <summary>
        ///     Pulse all
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PulseAll() => Monitor.PulseAll(_handle.Target!);

        /// <summary>
        ///     Try enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnter(TimeSpan timeout) => Monitor.TryEnter(_handle.Target!, timeout);

        /// <summary>
        ///     Try enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TryEnter(TimeSpan timeout, ref bool lockTaken) => Monitor.TryEnter(_handle.Target!, timeout, ref lockTaken);

        /// <summary>
        ///     Wait
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(TimeSpan timeout) => Monitor.Wait(_handle.Target!, timeout);

        /// <summary>
        ///     Wait
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait() => Monitor.Wait(_handle.Target!);

        /// <summary>
        ///     Wait
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(int millisecondsTimeout, bool exitContext) => Monitor.Wait(_handle.Target!, millisecondsTimeout, exitContext);

        /// <summary>
        ///     Wait
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Wait(TimeSpan timeout, bool exitContext) => Monitor.Wait(_handle.Target!, timeout, exitContext);

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Monitor.Exit(_handle.Target!);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMonitorLock Empty => default;
    }
}