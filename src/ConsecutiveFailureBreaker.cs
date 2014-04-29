using System;

namespace dervish
{
    public class ConsecutiveFailureBreaker : CircuitBreakerOptions
    {
        private int _currentAttemptCount;
        private DateTime? _circuitOpenTime;
        public CircuitBreaker.CircuitState CircuitState { get; private set; }

        public ConsecutiveFailureBreaker(int pauseBetweenCalls, int pauseWhenBreakerOpens, int numberOfRetries)
            :base(pauseBetweenCalls, pauseWhenBreakerOpens, numberOfRetries)
        {
            _currentAttemptCount = 0;
            _circuitOpenTime = null;
            CircuitState = CircuitBreaker.CircuitState.Closed;
        }

        public override bool IsPartiallyOpen()
        {
            return CircuitState == CircuitBreaker.CircuitState.PartiallyOpen;
        }

        public override bool IsOpen()
        {
            return CircuitState == CircuitBreaker.CircuitState.Open;
        }

        public override bool IsClosed()
        {
            return CircuitState == CircuitBreaker.CircuitState.Closed;
        }

        public override void FailureOccurred()
        {
            
        }

        public override bool TryAgain()
        {
            return _currentAttemptCount <= NumberOfRetries;
        }

        public override void Trying()
        {
            _currentAttemptCount++;
        }

        public override void SetPartiallyOpen()
        {
            if (CircuitState == CircuitBreaker.CircuitState.Open && _circuitOpenTime.HasValue)
            {
                if (DateTime.Now.Subtract(_circuitOpenTime.Value).Seconds < PauseWhenBreakerOpen)
                {
                    CircuitState = CircuitBreaker.CircuitState.PartiallyOpen;
                }
            }
        }

        public override void SetOpen()
        {
            _circuitOpenTime = DateTime.Now;
            CircuitState = CircuitBreaker.CircuitState.Open;
        }

        public override void SetClosed()
        {
            _circuitOpenTime = null;
            CircuitState = CircuitBreaker.CircuitState.Closed;
        }
    }
}