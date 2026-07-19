namespace crossbeam
{
    public static class Option
    {
        public static Option<T> Some<T>(T value) => new(true, value);

        public static Option<T> None<T>(T value) => new(false, value);
    }
}