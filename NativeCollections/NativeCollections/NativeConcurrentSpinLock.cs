using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrent spinLock
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.None)]
    public readonly unsafe ref struct NativeConcurrentSpinLock
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        private struct NativeConcurrentSpinLockHandle
        {
            /// <summary>
            ///     Sequence number
            /// </summary>
            [FieldOffset(0)] public volatile int SequenceNumber;

            /// <summary>
            ///     Next sequence number
            /// </summary>
            [FieldOffset(4)] public volatile int NextSequenceNumber;

            /// <summary>
            ///     Reset
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                SequenceNumber = 0;
                NextSequenceNumber = 1;
            }

            /// <summary>
            ///     Acquire
            /// </summary>
            /// <returns>Sequence number</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Acquire() => Interlocked.Add(ref SequenceNumber, 1);

            /// <summary>
            ///     Wait
            /// </summary>
            /// <param name="sequenceNumber">Sequence number</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Wait(int sequenceNumber)
            {
                var spinWait = new NativeSpinWait();
                while (sequenceNumber != NextSequenceNumber)
                    spinWait.SpinOnce();
            }

            /// <summary>
            ///     Wait
            /// </summary>
            /// <param name="sequenceNumber">Sequence number</param>
            /// <param name="sleepThreshold">Sleep threshold</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Wait(int sequenceNumber, int sleepThreshold)
            {
                var spinWait = new NativeSpinWait();
                while (sequenceNumber != NextSequenceNumber)
                    spinWait.SpinOnce(sleepThreshold);
            }

            /// <summary>
            ///     Enter
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Enter()
            {
                var spinWait = new NativeSpinWait();
                var sequenceNumber = Interlocked.Add(ref SequenceNumber, 1);
                while (sequenceNumber != NextSequenceNumber)
                    spinWait.SpinOnce();
            }

            /// <summary>
            ///     Enter
            /// </summary>
            /// <param name="sleepThreshold">Sleep threshold</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Enter(int sleepThreshold)
            {
                var spinWait = new NativeSpinWait();
                var sequenceNumber = Interlocked.Add(ref SequenceNumber, 1);
                while (sequenceNumber != NextSequenceNumber)
                    spinWait.SpinOnce(sleepThreshold);
            }

            /// <summary>
            ///     Exit
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Exit() => Interlocked.Add(ref NextSequenceNumber, 1);
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeConcurrentSpinLockHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLock(void* buffer) => _handle = (NativeConcurrentSpinLockHandle*)buffer;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Sequence number
        /// </summary>
        public int SequenceNumber => _handle->SequenceNumber;

        /// <summary>
        ///     Next sequence number
        /// </summary>
        public int NextSequenceNumber => _handle->NextSequenceNumber;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentSpinLock other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("Cannot call Equals on NativeConcurrentSpinLock");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeConcurrentSpinLock";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => left._handle != right._handle;

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Acquire
        /// </summary>
        /// <returns>Sequence number</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Acquire() => _handle->Acquire();

        /// <summary>
        ///     Wait
        /// </summary>
        /// <param name="sequenceNumber">Sequence number</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int sequenceNumber) => _handle->Wait(sequenceNumber);

        /// <summary>
        ///     Wait
        /// </summary>
        /// <param name="sequenceNumber">Sequence number</param>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int sequenceNumber, int sleepThreshold) => _handle->Wait(sequenceNumber, sleepThreshold);

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter() => _handle->Enter();

        /// <summary>
        ///     Enter
        /// </summary>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(int sleepThreshold) => _handle->Enter(sleepThreshold);

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => _handle->Exit();

        /// <summary>
        ///     As native concurrent spinLock
        /// </summary>
        /// <returns>NativeConcurrentSpinLock</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeConcurrentSpinLock(void* buffer) => new(buffer);

        /// <summary>
        ///     As native concurrent spinLock
        /// </summary>
        /// <returns>NativeConcurrentSpinLock</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeConcurrentSpinLock(Span<byte> span) => new(Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)));

        /// <summary>
        ///     As native concurrent spinLock
        /// </summary>
        /// <returns>NativeConcurrentSpinLock</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeConcurrentSpinLock(ReadOnlySpan<byte> readOnlySpan) => new(Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentSpinLock Empty => new();
    }
}