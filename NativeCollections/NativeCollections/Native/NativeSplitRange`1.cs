using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET9_0_OR_GREATER
using System.Collections;
#endif

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Splits the source span using a sequence of elements as the delimiter, but returns the positional ranges
    ///     (<see cref="Range" />) of each segment instead of the segment slices themselves.
    ///     The entire sequence must match for a split to occur.
    /// </summary>
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
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">The source to be split.</param>
        /// <param name="separator">The separator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(separator))]
        public NativeSplitRange(ReadOnlySpan<T> buffer, [MustBePinned] in T separator)
        {
            _buffer = buffer;
            _separator = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in separator), 1);
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">The source to be split.</param>
        /// <param name="separator">The separator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSplitRange(ReadOnlySpan<T> buffer, ReadOnlySpan<T> separator)
        {
            _buffer = buffer;
            _separator = separator;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeSplitRange<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !_buffer.IsEmpty;

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeSplitRange<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public Enumerator GetEnumerator() => new(_buffer, _separator);

#if NET9_0_OR_GREATER
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
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
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="buffer">The source to be split.</param>
            /// <param name="separator">The separator.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ReadOnlySpan<T> buffer, ReadOnlySpan<T> separator)
            {
                _current = default;
                _next = 0;
                _buffer = buffer;
                _separator = separator;
            }

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
                var buffer = _buffer.Slice(_next);
                var index = _separator.Length == 1 ? buffer.IndexOf(_separator[0]) : buffer.IndexOf(_separator);
                if (index < 0)
                {
                    if (buffer.Length > 0)
                    {
                        _current = new Range(_next, _next + buffer.Length);
                        _next = _buffer.Length;
                        return true;
                    }

                    return false;
                }

                _current = new Range(_next, _next + index);
                _next += index + _separator.Length;
                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _current = default;
                _next = 0;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly Range Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}