namespace crossbeam
{
    public static class Result
    {
        public static Result<T> Ok<T>(T value) => new(true, value);

        public static Result<T> Err<T>(T value) => new(false, value);
    }
}