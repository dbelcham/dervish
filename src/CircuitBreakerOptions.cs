namespace dervish
{
    public abstract class CircuitBreakerOptions
    {
        public int PauseBetweenCalls { get; private set; }
        public int PauseWhenBreakerOpen { get; private set; }
        public int NumberOfRetries { get; private set; }

        protected CircuitBreakerOptions(int pauseBetweenCalls, int pauseWhenBreakerOpen, int numberOfRetries)
        {
            PauseBetweenCalls = pauseBetweenCalls;
            PauseWhenBreakerOpen = pauseWhenBreakerOpen;
            NumberOfRetries = numberOfRetries;
        }

        public abstract bool IsPartiallyOpen();
        public abstract bool IsOpen();
        public abstract bool IsClosed();
        public abstract bool TryAgain();
        public abstract void Trying();
        public abstract void SetPartiallyOpen();
        public abstract void SetOpen();
        public abstract void SetClosed();
    }
}