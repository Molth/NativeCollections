using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native split any
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe ref struct NativeSplitAny<T> where T : IEquatable<T>
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
        public NativeSplitAny(ReadOnlySpan<T> buffer, in T separator)
        {
            if (Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)) == null || buffer.Length == 0)
                throw new ArgumentNullException(nameof(buffer));
            if (!typeof(T).IsValueType && separator == null)
                throw new ArgumentNullException(nameof(separator));
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
            if (Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)) == null || buffer.Length == 0)
                throw new ArgumentNullException(nameof(buffer));
            if (Unsafe.AsPointer(ref MemoryMarshal.GetReference(separator)) == null || separator.Length == 0)
                throw new ArgumentNullException(nameof(separator));
            _buffer = buffer;
            _separator = separator;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        public Enumerator GetEnumerator() => new(_buffer, _separator);

        /// <summary>
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public ref struct Enumerator
        {
            /// <summary>
            ///     Current
            /// </summary>
            private ReadOnlySpan<T> _current;

            /// <summary>
            ///     Buffer
            /// </summary>
            private ReadOnlySpan<T> _buffer;

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
                _buffer = buffer;
                _separator = separator;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var index = _separator.Length == 1 ? _buffer.IndexOf(_separator[0]) : _buffer.IndexOfAny(_separator);
                if (index < 0)
                {
                    if (_buffer.Length > 0)
                    {
                        _current = _buffer;
                        _buffer = default;
                        return true;
                    }

                    return false;
                }

                _current = _buffer.Slice(0, index);
                _buffer = _buffer.Slice(index + 1);
                return true;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public ReadOnlySpan<T> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}