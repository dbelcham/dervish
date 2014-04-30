using System;

namespace dervish
{
    public class ConsecutiveFailureBreaker : CircuitBreakerOptions
    {
        private int _currentAttemptCount;

        public ConsecutiveFailureBreaker(int pauseBetweenCalls, int pauseWhenBreakerOpens, int numberOfRetries)
            :base(pauseBetweenCalls, pauseWhenBreakerOpens, numberOfRetries)
        {
            _currentAttemptCount = 0;
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

        public override void SetOpen()
        {
            CircuitOpenTime = DateTime.Now;
            CircuitState = CircuitBreaker.CircuitState.Open;
        }

        public override void SetClosed()
        {
            CircuitOpenTime = null;
            CircuitState = CircuitBreaker.CircuitState.Closed;
        }
    }
}