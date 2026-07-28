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
    ///     Splits the source span using any of the specified single elements as delimiters.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [IsReferenceOrContainsReferences]
    [IsAssignableTo(typeof(IIsCreated), typeof(IEnumerable<>))]
    public readonly ref struct NativeSplitAny<T>
#if NET9_0_OR_GREATER
        : IIsCreated, IEnumerable<ReadOnlySpan<T>>
#endif
        where T : IEquatable<T>
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly ReadOnlySpan<T> _buffer;

        /// <summary>
        ///     Separator
        /// </summary>
        private readonly ReadOnlySpan<T> _separator;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="separator">Separator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(separator))]
        public NativeSplitAny(ReadOnlySpan<T> buffer, [MustBePinned] in T separator)
        {
            _buffer = buffer;
            _separator = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in separator), 1);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="separator">Separator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeSplitAny(ReadOnlySpan<T> buffer, ReadOnlySpan<T> separator)
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
        ///     Get hashCode
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
        public override string ToString() => SR.Format("NativeSplitAny<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !_buffer.IsEmpty;

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeSplitAny<T> Empty => default;

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
        IEnumerator<ReadOnlySpan<T>> IEnumerable<ReadOnlySpan<T>>.GetEnumerator()
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
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [IsAssignableTo(typeof(IIterator<>))]
        public ref struct Enumerator
#if NET9_0_OR_GREATER
            : IIterator<ReadOnlySpan<T>>
#endif
        {
            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private Range _current;

            /// <summary>
            ///     Next
            /// </summary>
            private int _next;

            /// <summary>
            ///     Buffer
            /// </summary>
            private readonly ReadOnlySpan<T> _buffer;

            /// <summary>
            ///     Separator
            /// </summary>
            private readonly ReadOnlySpan<T> _separator;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="separator">Separator</param>
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
                var index = _separator.Length == 1 ? buffer.IndexOf(_separator[0]) : buffer.IndexOfAny(_separator);
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
                _next += index + 1;
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
            public readonly ReadOnlySpan<T> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer[_current];
            }
        }
    }
}