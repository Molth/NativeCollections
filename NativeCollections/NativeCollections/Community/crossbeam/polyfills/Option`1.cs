using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

// ReSharper disable All

namespace crossbeam
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Option<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;

        public Option(bool hasValue, T value)
        {
            _hasValue = hasValue;
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool is_some() => _hasValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool is_none() => !is_some();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T unwrap_unchecked() => _value;
    }
}