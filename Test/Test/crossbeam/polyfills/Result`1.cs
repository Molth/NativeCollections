namespace crossbeam
{
    public readonly struct Result<T>
    {
        private readonly bool _ok;
        private readonly T _value;

        public Result(bool ok, T value)
        {
            _ok = ok;
            _value = value;
        }

        public bool is_ok() => _ok;

        public bool is_err() => !is_ok();

        public T unwrap_unchecked() => _value;
    }
}