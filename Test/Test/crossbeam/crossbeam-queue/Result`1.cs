namespace crossbeam
{
    public readonly struct Result<T>
    {
        public readonly bool Ok;
        public readonly T Value;

        public Result(bool ok, T value)
        {
            Ok = ok;
            Value = value;
        }
    }
}