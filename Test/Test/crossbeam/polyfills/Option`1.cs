namespace crossbeam
{
    public readonly struct Option<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;

        public Option(bool hasValue, T value)
        {
            _hasValue = hasValue;
            _value = value;
        }

        public bool is_some() => _hasValue;

        public bool is_none() => !is_some();

        public T unwrap_unchecked() => _value;
    }
}