using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
using System.Threading;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberHidesStaticFromOuterClass

namespace NativeCollections
{
    /// <summary>
    ///     Native spinLock
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct NativeSpinLock : IDisposable, IEquatable<NativeSpinLock>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeSpinLockHandle
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
        private readonly NativeSpinLockHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpinLock(int sleepThreshold)
        {
            if (sleepThreshold < -1)
                sleepThreshold = -1;
            else if (sleepThreshold >= 0 && sleepThreshold < 10)
                sleepThreshold = 10;
            _handle = (NativeSpinLockHandle*)NativeMemoryAllocator.Alloc(sizeof(NativeSpinLockHandle));
            _handle->SequenceNumber = 0;
            _handle->NextSequenceNumber = 1;
            _handle->SleepThreshold = sleepThreshold;
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
        public bool Equals(NativeSpinLock other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeSpinLock nativeSpinLock && nativeSpinLock == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_handle;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeSpinLock";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeSpinLock left, NativeSpinLock right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeSpinLock left, NativeSpinLock right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_handle == null)
                return;
            NativeMemoryAllocator.Free(_handle);
        }

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            var sequenceNumber = Interlocked.Add(ref _handle->SequenceNumber, 1);
            if (sequenceNumber != _handle->NextSequenceNumber)
            {
                var count = 0;
                var sleepThreshold = _handle->SleepThreshold;
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
                } while (sequenceNumber != _handle->NextSequenceNumber);
            }
        }

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Interlocked.Add(ref _handle->NextSequenceNumber, 1);
    }
}