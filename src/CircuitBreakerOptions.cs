namespace dervish
{
    public class CircuitBreakerOptions
    {
        public int PauseBetweenCalls { get; private set; }
        public int PauseWhenBreakerOpen { get; private set; }
        public int NumberOfRetries { get; private set; }

        public CircuitBreakerOptions(int pauseBetweenCalls, int pauseWhenBreakerOpen, int numberOfRetries)
        {
            PauseBetweenCalls = pauseBetweenCalls;
            PauseWhenBreakerOpen = pauseWhenBreakerOpen;
            NumberOfRetries = numberOfRetries;
        }
    }
}