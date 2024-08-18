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
            ///     Iterations
            /// </summary>
            public int Iterations;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeSpinLockHandle* _handle;

        /// <summary>
        ///     Iterations
        /// </summary>
        public int Iterations => _handle->Iterations;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="iterations">Iterations</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSpinLock(int iterations)
        {
            if (iterations < 0)
                throw new ArgumentOutOfRangeException(nameof(iterations), iterations, "MustBeNonNegative");
            if (iterations == 0)
                iterations = 1;
            _handle = (NativeSpinLockHandle*)NativeMemoryAllocator.Alloc(sizeof(NativeSpinLockHandle));
            _handle->SequenceNumber = 0;
            _handle->NextSequenceNumber = 1;
            _handle->Iterations = iterations;
        }

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
                var iterations = _handle->Iterations;
                do
                {
                    Thread.SpinWait(iterations);
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