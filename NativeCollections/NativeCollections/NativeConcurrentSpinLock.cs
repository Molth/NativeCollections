﻿using System;
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
    [NativeCollection]
    public readonly unsafe struct NativeConcurrentSpinLock : IDisposable, IEquatable<NativeConcurrentSpinLock>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeConcurrentSpinLockHandle
        {
            /// <summary>
            ///     Sequence number
            /// </summary>
            public int SequenceNumber;

            /// <summary>
            ///     Next sequence number
            /// </summary>
            public int NextSequenceNumber;

            /// <summary>
            ///     Sleep threshold
            /// </summary>
            public int SleepThreshold;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeConcurrentSpinLockHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLock(int sleepThreshold)
        {
            if (sleepThreshold < -1)
                sleepThreshold = -1;
            else if (sleepThreshold >= 0 && sleepThreshold < 10)
                sleepThreshold = 10;
            var handle = (NativeConcurrentSpinLockHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeConcurrentSpinLockHandle));
            handle->SequenceNumber = 0;
            handle->NextSequenceNumber = 1;
            handle->SleepThreshold = sleepThreshold;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Sleep threshold
        /// </summary>
        public int SleepThreshold => _handle->SleepThreshold;

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
        public override bool Equals(object? obj) => obj is NativeConcurrentSpinLock nativeConcurrentSpinLock && nativeConcurrentSpinLock == this;

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
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            var handle = _handle;
            var sequenceNumber = Interlocked.Add(ref handle->SequenceNumber, 1);
            if (sequenceNumber != handle->NextSequenceNumber)
            {
                var count = 0;
                var sleepThreshold = handle->SleepThreshold;
                do
                {
                    if ((count >= 10 && ((count >= sleepThreshold && sleepThreshold >= 0) || (count - 10) % 2 == 0)) || Environment.ProcessorCount == 1)
                    {
                        if (count >= sleepThreshold && sleepThreshold >= 0)
                        {
                            Thread.Sleep(1);
                        }
                        else
                        {
                            var yieldsSoFar = count >= 10 ? (count - 10) / 2 : count;
                            if (yieldsSoFar % 5 == 4)
                                Thread.Sleep(0);
                            else
                                Thread.Yield();
                        }
                    }
                    else
                    {
                        var iterations = Environment.ProcessorCount / 2;
                        if (count <= 30 && 1 << count < iterations)
                            iterations = 1 << count;
                        Thread.SpinWait(iterations);
                    }

                    count = count == int.MaxValue ? 10 : count + 1;
                } while (sequenceNumber != handle->NextSequenceNumber);
            }
        }

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Interlocked.Add(ref _handle->NextSequenceNumber, 1);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentSpinLock Empty => new();
    }
}