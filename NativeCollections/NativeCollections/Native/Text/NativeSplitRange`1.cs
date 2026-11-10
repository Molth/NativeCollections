using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET9_0_OR_GREATER
using System.Collections;
#endif

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Splits the source span using a sequence of elements as the delimiter, but returns
    ///     the positional ranges (<see cref="Range" />) of each segment instead of the
    ///     segment slices themselves. The entire sequence must match for a split to occur.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source span.</typeparam>
    /// <remarks>
    ///     The delimiter is either the single element supplied as the delimiter or the complete
    ///     delimiter sequence; a split occurs only where the whole delimiter matches. An empty
    ///     source produces exactly one empty segment, represented as an empty <see cref="Range" />.
    ///     A delimiter at the start or end of the source, or consecutive delimiters, produce
    ///     empty segments. When an empty delimiter sequence is supplied, no match is found and
    ///     the remaining buffer is returned as a single segment.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [IsReferenceOrContainsReferences]
    [IsAssignableTo(typeof(IIsCreated), typeof(IEnumerable<Range>))]
    public readonly ref struct NativeSplitRange<T>
#if NET9_0_OR_GREATER
        : IIsCreated, IEnumerable<Range>
#endif
        where T : IEquatable<T>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly ReadOnlySpan<T> _buffer;

        /// <summary>
        ///     The separator.
        /// </summary>
        private readonly ReadOnlySpan<T> _separator;

        /// <summary>
        ///     The single element used as the delimiter.
        /// </summary>
        private readonly T _separatorElement;

        /// <summary>
        ///     Indicates whether <see cref="_separatorElement" /> is the delimiter.
        /// </summary>
        private readonly bool _hasSingleSeparator;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">The source to be split.</param>
        /// <param name="bufferLength">The length of the source buffer.</param>
        /// <param name="separator">The separator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeSplitRange([MustBePinned] ref T buffer, int bufferLength, T separator)
        {
            _buffer = MemoryMarshal.CreateReadOnlySpan(ref buffer, bufferLength);
            _separator = default;
            _separatorElement = separator;
            _hasSingleSeparator = true;
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">The source to be split.</param>
        /// <param name="bufferLength">The length of the source buffer.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="separatorLength">The length of the separator buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeSplitRange([MustBePinned] ref T buffer, int bufferLength, [MustBePinned] ref T separator, int separatorLength)
        {
            _buffer = MemoryMarshal.CreateReadOnlySpan(ref buffer, bufferLength);
            _separator = MemoryMarshal.CreateReadOnlySpan(ref separator, separatorLength);
            _separatorElement = default!;
            _hasSingleSeparator = false;
        }

        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        /// <param name="buffer">The source to be split.</param>
        /// <param name="bufferLength">The length of the source buffer.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>A new instance of this class.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static NativeSplitRange<T> From([MustBePinned] ref T buffer, int bufferLength, T separator) => new(ref buffer, bufferLength, separator);

        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        /// <param name="buffer">The source to be split.</param>
        /// <param name="bufferLength">The length of the source buffer.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="separatorLength">The length of the separator buffer.</param>
        /// <returns>A new instance of this class.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static NativeSplitRange<T> From([MustBePinned] ref T buffer, int bufferLength, [MustBePinned] ref T separator, int separatorLength) => new(ref buffer, bufferLength, ref separator, separatorLength);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeSplitRange<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !_buffer.IsEmpty;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public Enumerator GetEnumerator() => Enumerator.From(ref MemoryMarshal.GetReference(_buffer), _buffer.Length, ref MemoryMarshal.GetReference(_separator), _separator.Length, _separatorElement, _hasSingleSeparator);

#if NET9_0_OR_GREATER
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<Range> IEnumerable<Range>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
#endif

        /// <summary>
        ///     Supports a simple iteration over a generic collection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [IsAssignableTo(typeof(IIterator<Range>))]
        public ref struct Enumerator
#if NET9_0_OR_GREATER
            : IIterator<Range>
#endif
        {
            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private Range _current;

            /// <summary>
            ///     The starting index of the remaining portion of the buffer to be split.
            /// </summary>
            private int _next;

            /// <summary>
            ///     Represents a contiguous region of arbitrary memory.
            /// </summary>
            private readonly ReadOnlySpan<T> _buffer;

            /// <summary>
            ///     The separator.
            /// </summary>
            private readonly ReadOnlySpan<T> _separator;

            /// <summary>
            ///     The single element used as the delimiter.
            /// </summary>
            private readonly T _separatorElement;

            /// <summary>
            ///     Indicates whether <see cref="_separatorElement" /> is the delimiter.
            /// </summary>
            private readonly bool _hasSingleSeparator;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="buffer">The source to be split.</param>
            /// <param name="bufferLength">The length of the source buffer.</param>
            /// <param name="separator">The separator.</param>
            /// <param name="separatorLength">The length of the separator buffer.</param>
            /// <param name="separatorElement">The single element used as the delimiter.</param>
            /// <param name="hasSingleSeparator">Whether <paramref name="separatorElement" /> is the delimiter.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator([MustBePinned] ref T buffer, int bufferLength, [MustBePinned] ref T separator, int separatorLength, T separatorElement, bool hasSingleSeparator)
            {
                _current = default;
                _next = 0;
                _buffer = MemoryMarshal.CreateReadOnlySpan(ref buffer, bufferLength);
                _separator = MemoryMarshal.CreateReadOnlySpan(ref separator, separatorLength);
                _separatorElement = separatorElement;
                _hasSingleSeparator = hasSingleSeparator;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="buffer">The source to be split.</param>
            /// <param name="bufferLength">The length of the source buffer.</param>
            /// <param name="separator">The separator.</param>
            /// <param name="separatorLength">The length of the separator buffer.</param>
            /// <param name="separatorElement">The single element used as the delimiter.</param>
            /// <param name="hasSingleSeparator">Whether <paramref name="separatorElement" /> is the delimiter.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From([MustBePinned] ref T buffer, int bufferLength, [MustBePinned] ref T separator, int separatorLength, T separatorElement, bool hasSingleSeparator) => new(ref buffer, bufferLength, ref separator, separatorLength, separatorElement, hasSingleSeparator);

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_next < 0)
                    return false;

                var buffer = _buffer.Slice(_next);
                var separatorLength = _hasSingleSeparator ? 1 : _separator.Length;
                var index = separatorLength == 0 ? -1 : _hasSingleSeparator ? buffer.IndexOf(_separatorElement) : separatorLength == 1 ? buffer.IndexOf(_separator[0]) : buffer.IndexOf(_separator);
                if (index < 0)
                {
                    _current = new Range(_next, _next.WrappingAdd(buffer.Length));
                    _next = -1;
                    return true;
                }

                _current = new Range(_next, _next.WrappingAdd(index));
                _next = _next.WrappingAdd(index.WrappingAdd(separatorLength));
                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _current = default;
                _next = 0;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly Range Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}