using System;

namespace dervish
{
    public abstract class CircuitBreakerOptions
    {
        public int PauseBetweenCalls { get; private set; }
        public int PauseWhenBreakerOpen { get; private set; }
        public int NumberOfRetries { get; private set; }
        public CircuitBreaker.CircuitState CircuitState { get; protected set; }
        public DateTime? CircuitOpenTime { get; protected set; }

        protected CircuitBreakerOptions(int pauseBetweenCalls, int pauseWhenBreakerOpen, int numberOfRetries)
        {
            PauseBetweenCalls = pauseBetweenCalls;
            PauseWhenBreakerOpen = pauseWhenBreakerOpen;
            NumberOfRetries = numberOfRetries;
            CircuitState = CircuitBreaker.CircuitState.Closed;
            CircuitOpenTime = null;
        }

        public abstract bool TryAgain();
        public abstract void Trying();
        public abstract void SetOpen();
        public abstract void SetClosed();

        public virtual bool IsPartiallyOpen()
        {
            return CircuitState == CircuitBreaker.CircuitState.PartiallyOpen;
        }

        public virtual bool IsOpen()
        {
            return CircuitState == CircuitBreaker.CircuitState.Open;
        }

        public virtual bool IsClosed()
        {
            return CircuitState == CircuitBreaker.CircuitState.Closed;
        }

        public abstract void FailureOccurred();

        public void SetPartiallyOpen()
        {
            if (CircuitState == CircuitBreaker.CircuitState.Open && CircuitOpenTime.HasValue)
            {
                if (DateTime.Now.Subtract(CircuitOpenTime.Value).Seconds >= PauseWhenBreakerOpen)
                {
                    CircuitState = CircuitBreaker.CircuitState.PartiallyOpen;
                }
            }
        }
    }
}