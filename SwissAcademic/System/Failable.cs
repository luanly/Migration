namespace System
{
    public class Failable<T>
    {
        public Failable(Func<T> func)
        {
            try
            {
                Value = func();
                HasValue = true;
            }
            catch (Exception exception)
            {
                Exception = exception;
            }
        }

        public Exception Exception { get; }

        public bool HasValue { get; }
        public T Value { get; }
    }
}
