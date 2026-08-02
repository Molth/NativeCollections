using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static crossbeam.Option;

// ReSharper disable All

namespace crossbeam
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Result<T> where T : unmanaged
    {
        private readonly bool _ok;
        private readonly T _value;

        public Result(bool ok, T value)
        {
            _ok = ok;
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool is_ok() => _ok;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool is_err() => !is_ok();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T unwrap_unchecked() => _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> err()
        {
            if (is_ok())
            {
                return None<T>();
            }
            else
            {
                return Some(_value);
            }
        }
    }
}