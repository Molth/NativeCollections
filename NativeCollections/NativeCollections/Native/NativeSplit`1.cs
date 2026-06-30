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
    ///     Native split
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [IsReferenceOrContainsReferences]
    [IsAssignableTo(typeof(IIsCreated), typeof(IEnumerable<>))]
    public readonly ref struct NativeSplit<T>
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
        public NativeSplit(ReadOnlySpan<T> buffer, [MustBePinned] in T separator)
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
        public NativeSplit(ReadOnlySpan<T> buffer, ReadOnlySpan<T> separator)
        {
            _buffer = buffer;
            _separator = separator;
        }

        /// <summary>
        ///     Equals
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
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("NativeSplit<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !_buffer.IsEmpty;

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeSplit<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        public Enumerator GetEnumerator() => new(_buffer, _separator);

#if NET9_0_OR_GREATER
        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<ReadOnlySpan<T>> IEnumerable<ReadOnlySpan<T>>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
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
            ///     Current
            /// </summary>
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
            ///     Move next
            /// </summary>
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
            ///     Reset
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _current = default;
                _next = 0;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public readonly ReadOnlySpan<T> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _buffer[_current];
            }
        }
    }
}