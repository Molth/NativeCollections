using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a set of methods and properties that you can use to accurately measure elapsed time.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public struct UnsafeStopwatch : IEquatable<UnsafeStopwatch>
    {
        /// <summary>
        ///     Gets the frequency of the timer as the number of ticks per second. This field is read-only.
        /// </summary>
        public static long Frequency => Stopwatch.Frequency;

        /// <summary>
        ///     Indicates whether the timer is based on a high-resolution performance counter. This field is read-only.
        /// </summary>
        public static bool IsHighResolution => Stopwatch.IsHighResolution;

        /// <summary>
        ///     Multiplier used to convert performance counter ticks to <see cref="TimeSpan" /> ticks.
        /// </summary>
        /// <remarks>
        ///     This value is precomputed as <c>TimeSpan.TicksPerSecond / Stopwatch.Frequency</c>.
        /// </remarks>
        private static readonly double _tickFrequency = F64.FromI64Unchecked(TimeSpan.TicksPerSecond) / F64.FromI64Unchecked(Stopwatch.Frequency);

        /// <summary>
        ///     Cumulative elapsed time in performance counter ticks, excluding any currently running interval.
        /// </summary>
        private long _elapsed;

        /// <summary>
        ///     Timestamp obtained from <see cref="GetTimestamp" /> when the stopwatch was last started or restarted.
        /// </summary>
        private long _startTimeStamp;

        /// <summary>
        ///     Whether the timer is currently running.
        /// </summary>
        private bool _isRunning;

        /// <summary>
        ///     Gets a value indicating whether the <see cref="Stopwatch" /> timer is running.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the <see cref="Stopwatch" /> instance is currently running
        ///     and measuring elapsed time for an interval; otherwise, <see langword="false" />.
        /// </value>
        public readonly bool IsRunning => _isRunning;

        /// <summary>
        ///     Gets the total elapsed time measured by the current instance.
        /// </summary>
        /// <value>
        ///     A read-only <see cref="TimeSpan" /> representing the
        ///     total elapsed time measured by the current instance.
        /// </value>
        public readonly TimeSpan Elapsed => new(GetElapsedDateTimeTicks());

        /// <summary>
        ///     Gets the total elapsed time measured by the current instance, in milliseconds.
        /// </summary>
        /// <value>
        ///     A read-only long integer representing the total number of milliseconds measured by the current instance.
        /// </value>
        public readonly long ElapsedMilliseconds => GetElapsedDateTimeTicks() / 10000L;

        /// <summary>
        ///     Gets the total elapsed time measured by the current instance, in timer ticks.
        /// </summary>
        /// <value>
        ///     A read-only long integer representing the total number of timer ticks measured by the current instance.
        /// </value>
        public readonly long ElapsedTicks => GetRawElapsedTicks();

        /// <summary>
        ///     Starts, or resumes, measuring elapsed time for an interval.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start()
        {
            if (_isRunning)
                return;

            _startTimeStamp = GetTimestamp();
            _isRunning = true;
        }

        /// <summary>
        ///     Stops measuring elapsed time for an interval.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            if (!_isRunning)
                return;

            _elapsed = _elapsed.WrappingAdd(GetTimestamp().WrappingSub(_startTimeStamp));
            _isRunning = false;
            if (_elapsed >= 0L)
                return;

            _elapsed = 0L;
        }

        /// <summary>
        ///     Stops time interval measurement and resets the elapsed time to zero.
        /// </summary>
        public void Reset()
        {
            _elapsed = 0L;
            _isRunning = false;
            _startTimeStamp = 0L;
        }

        /// <summary>
        ///     Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Restart()
        {
            _elapsed = 0L;
            _startTimeStamp = GetTimestamp();
            _isRunning = true;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeStopwatch other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeStopwatch other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the <see cref="Stopwatch.Elapsed" /> time as a string.
        /// </summary>
        /// <returns>The elapsed time string in the same format used by <see cref="TimeSpan.ToString()" />.</returns>
        public readonly override string ToString() => Elapsed.ToString();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeStopwatch left, UnsafeStopwatch right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeStopwatch left, UnsafeStopwatch right) => !left.Equals(right);

        /// <summary>
        ///     Gets the current number of ticks in the timer mechanism.
        /// </summary>
        /// <returns>A long integer representing the tick counter value of the underlying timer mechanism.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTimestamp() => Stopwatch.GetTimestamp();

        /// <summary>
        ///     Gets the elapsed time since the <paramref name="startingTimestamp" />
        ///     value retrieved using <see cref="Stopwatch.GetTimestamp" />.
        /// </summary>
        /// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
        /// <returns>
        ///     A <see cref="TimeSpan" /> for the elapsed time between the starting timestamp and the time of this call.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan GetElapsedTime(long startingTimestamp)
        {
#if NET7_0_OR_GREATER
            return Stopwatch.GetElapsedTime(startingTimestamp);
#else
            return GetElapsedTime(startingTimestamp, GetTimestamp());
#endif
        }

        /// <summary>
        ///     Gets the elapsed time between two timestamps retrieved using <see cref="Stopwatch.GetTimestamp" />.
        /// </summary>
        /// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
        /// <param name="endingTimestamp">The timestamp marking the end of the time period.</param>
        /// <returns>
        ///     A <see cref="TimeSpan" /> for the elapsed time between the starting and ending timestamps.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
        {
#if NET7_0_OR_GREATER
            return Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);
#else
            return new TimeSpan(I64.FromF64Unchecked(F64.FromI64Unchecked(endingTimestamp.WrappingSub(startingTimestamp)) * _tickFrequency));
#endif
        }

        /// <summary>
        ///     Gets the total elapsed time in performance counter ticks, including any currently running interval.
        /// </summary>
        /// <returns>The total elapsed time in performance counter ticks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly long GetRawElapsedTicks()
        {
            var elapsed = _elapsed;
            if (_isRunning)
            {
                var num = GetTimestamp().WrappingSub(_startTimeStamp);
                elapsed = elapsed.WrappingAdd(num);
            }

            return elapsed;
        }

        /// <summary>
        ///     Gets the total elapsed time as
        ///     <see cref="TimeSpan.TicksPerSecond" />-based ticks (i.e., DateTime ticks).
        /// </summary>
        /// <returns>The elapsed time in DateTime ticks (100 nanoseconds per tick).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly long GetElapsedDateTimeTicks() => I64.FromF64Unchecked(F64.FromI64Unchecked(GetRawElapsedTicks()) * _tickFrequency);

        /// <summary>
        ///     Initializes a new <see cref="UnsafeStopwatch" /> instance, sets the
        ///     elapsed time property to zero, and starts measuring elapsed time.
        /// </summary>
        /// <returns>A <see cref="UnsafeStopwatch" /> that has just begun measuring elapsed time.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeStopwatch StartNew()
        {
            UnsafeStopwatch stopwatch = default;
            stopwatch.Start();
            return stopwatch;
        }
    }
}