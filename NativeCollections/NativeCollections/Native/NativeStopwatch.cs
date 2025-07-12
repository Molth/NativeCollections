using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native stopwatch
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public struct NativeStopwatch
    {
        /// <summary>Gets the frequency of the timer as the number of ticks per second. This field is read-only.</summary>
        public static long Frequency => Stopwatch.Frequency;

        /// <summary>Indicates whether the timer is based on a high-resolution performance counter. This field is read-only.</summary>
        public static bool IsHighResolution => Stopwatch.IsHighResolution;

        /// <summary>
        ///     Tick frequency
        /// </summary>
        private static readonly double _tickFrequency = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        /// <summary>
        ///     Elapsed
        /// </summary>
        private long _elapsed;

        /// <summary>
        ///     Start timeStamp
        /// </summary>
        private long _startTimeStamp;

        /// <summary>
        ///     Is running
        /// </summary>
        private bool _isRunning;

        /// <summary>Gets a value indicating whether the <see cref="T:System.Diagnostics.Stopwatch" /> timer is running.</summary>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="T:System.Diagnostics.Stopwatch" /> instance is currently running and
        ///     measuring elapsed time for an interval; otherwise, false.
        /// </returns>
        public readonly bool IsRunning => _isRunning;

        /// <summary>Gets the total elapsed time measured by the current instance.</summary>
        /// <returns>
        ///     A read-only <see cref="T:System.TimeSpan" /> representing the total elapsed time measured by the current
        ///     instance.
        /// </returns>
        public readonly TimeSpan Elapsed => new(GetElapsedDateTimeTicks());

        /// <summary>Gets the total elapsed time measured by the current instance, in milliseconds.</summary>
        /// <returns>A read-only long integer representing the total number of milliseconds measured by the current instance.</returns>
        public readonly long ElapsedMilliseconds => GetElapsedDateTimeTicks() / 10000L;

        /// <summary>Gets the total elapsed time measured by the current instance, in timer ticks.</summary>
        /// <returns>A read-only long integer representing the total number of timer ticks measured by the current instance.</returns>
        public readonly long ElapsedTicks => GetRawElapsedTicks();

        /// <summary>Starts, or resumes, measuring elapsed time for an interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start()
        {
            if (_isRunning)
                return;
            _startTimeStamp = GetTimestamp();
            _isRunning = true;
        }

        /// <summary>Stops measuring elapsed time for an interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            if (!_isRunning)
                return;
            _elapsed += GetTimestamp() - _startTimeStamp;
            _isRunning = false;
            if (_elapsed >= 0L)
                return;
            _elapsed = 0L;
        }

        /// <summary>Stops time interval measurement and resets the elapsed time to zero.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _elapsed = 0L;
            _isRunning = false;
            _startTimeStamp = 0L;
        }

        /// <summary>Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Restart()
        {
            _elapsed = 0L;
            _startTimeStamp = GetTimestamp();
            _isRunning = true;
        }

        /// <summary>Returns the <see cref="P:System.Diagnostics.Stopwatch.Elapsed" /> time as a string.</summary>
        /// <returns>The elapsed time string in the same format used by <see cref="M:System.TimeSpan.ToString" />.</returns>
        public readonly override string ToString() => Elapsed.ToString();

        /// <summary>Gets the current number of ticks in the timer mechanism.</summary>
        /// <returns>A long integer representing the tick counter value of the underlying timer mechanism.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTimestamp() => Stopwatch.GetTimestamp();

        /// <summary>
        ///     Gets the elapsed time since the <paramref name="startingTimestamp" /> value retrieved using
        ///     <see cref="M:System.Diagnostics.Stopwatch.GetTimestamp" />.
        /// </summary>
        /// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
        /// <returns>
        ///     A <see cref="T:System.TimeSpan" /> for the elapsed time between the starting timestamp and the time of this call.
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
        ///     Gets the elapsed time between two timestamps retrieved using
        ///     <see cref="M:System.Diagnostics.Stopwatch.GetTimestamp" />.
        /// </summary>
        /// <param name="startingTimestamp">The timestamp marking the beginning of the time period.</param>
        /// <param name="endingTimestamp">The timestamp marking the end of the time period.</param>
        /// <returns>A <see cref="T:System.TimeSpan" /> for the elapsed time between the starting and ending timestamps.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
        {
#if NET7_0_OR_GREATER
            return Stopwatch.GetElapsedTime(startingTimestamp, endingTimestamp);
#else
            return new TimeSpan((long)((endingTimestamp - startingTimestamp) * _tickFrequency));
#endif
        }

        /// <summary>
        ///     Get raw elapsed ticks
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly long GetRawElapsedTicks()
        {
            var elapsed = _elapsed;
            if (_isRunning)
            {
                var num = GetTimestamp() - _startTimeStamp;
                elapsed += num;
            }

            return elapsed;
        }

        /// <summary>
        ///     Get elapsed dateTime ticks
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly long GetElapsedDateTimeTicks() => (long)(GetRawElapsedTicks() * _tickFrequency);

        /// <summary>
        ///     Initializes a new <see cref="T:System.Diagnostics.Stopwatch" /> instance, sets the elapsed time property to
        ///     zero, and starts measuring elapsed time.
        /// </summary>
        /// <returns>A <see cref="T:System.Diagnostics.Stopwatch" /> that has just begun measuring elapsed time.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeStopwatch StartNew()
        {
            var stopwatch = new NativeStopwatch();
            stopwatch.Start();
            return stopwatch;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeStopwatch Empty => new();
    }
}