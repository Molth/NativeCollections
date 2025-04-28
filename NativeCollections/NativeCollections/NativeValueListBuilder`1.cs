using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native value list builder
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public unsafe ref struct NativeValueListBuilder<T> where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private Span<T> _buffer;

        /// <summary>
        ///     Array
        /// </summary>
        private T[]? _array;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length
        {
            get => _length;
            set => _length = value;
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[int index] => ref _buffer[index];

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => Unsafe.AsPointer(ref MemoryMarshal.GetReference(_buffer)) != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeValueListBuilder(Span<T> buffer)
        {
            _buffer = buffer;
            _array = null;
            _length = 0;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var array = _array;
            if (array == null)
                return;
            _array = null;
            ArrayPool<T>.Shared.Return(array);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("CannotCallEquals");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => throw new NotSupportedException("CannotCallGetHashCode");

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeValueListBuilder<{typeof(T).Name}>[{_length}]";

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in T item)
        {
            var length = _length;
            var buffer = _buffer;
            if ((uint)length < (uint)buffer.Length)
            {
                buffer[length] = item;
                _length = length + 1;
            }
            else
            {
                Grow();
                _buffer[length] = item;
                _length = length + 1;
            }
        }

        /// <summary>
        ///     Append
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ReadOnlySpan<T> source)
        {
            var length = _length;
            var buffer = _buffer;
            if (source.Length == 1 && (uint)length < (uint)buffer.Length)
            {
                buffer[length] = source[0];
                _length = length + 1;
            }
            else
            {
                if ((uint)(_length + source.Length) > (uint)_buffer.Length)
                    Grow(_buffer.Length - _length + source.Length);
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), _length)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(source)), (uint)(source.Length * sizeof(T)));
                _length += source.Length;
            }
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => _buffer.Slice(0, _length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => _buffer.Slice(0, _length);

        /// <summary>
        ///     Cast
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<TTo> Cast<TTo>() where TTo : unmanaged => MemoryMarshal.Cast<T, TTo>(AsSpan());

        /// <summary>
        ///     Grow
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int additionalCapacityRequired = 1)
        {
            var minimumLength = Math.Max(_buffer.Length != 0 ? _buffer.Length * 2 : 4, _buffer.Length + additionalCapacityRequired);
            if ((uint)minimumLength > 2147483591U)
                minimumLength = Math.Max(Math.Max(_buffer.Length + 1, 2147483591), _buffer.Length);
            var destination = ArrayPool<T>.Shared.Rent(minimumLength);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference((Span<T>)destination)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(_buffer)), (uint)(_length * sizeof(T)));
            var array = _array;
            _buffer = (Span<T>)(_array = destination);
            if (array == null)
                return;
            ArrayPool<T>.Shared.Return(array);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeValueListBuilder<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        public Span<T>.Enumerator GetEnumerator() => _buffer.GetEnumerator();
    }
}