using System;

namespace dervish.Tests
{
    public class BasicLoopBreakerOptions : CircuitBreakerOptions
    {
        private int _currentAttemptCount;
        private CircuitBreaker.CircuitState _circuitState;
        private DateTime? _circuitOpenTime;

        public BasicLoopBreakerOptions(int pauseBetweenCalls, int pauseWhenBreakerOpens, int numberOfRetries)
            :base(pauseBetweenCalls, pauseWhenBreakerOpens, numberOfRetries)
        {
            _currentAttemptCount = 0;
            _circuitOpenTime = null;
            _circuitState = CircuitBreaker.CircuitState.Closed;
        }

        public override bool IsPartiallyOpen()
        {
            return _circuitState == CircuitBreaker.CircuitState.PartiallyOpen;
        }

        public override bool IsOpen()
        {
            return _circuitState == CircuitBreaker.CircuitState.Open;
        }

        public override bool IsClosed()
        {
            return _circuitState == CircuitBreaker.CircuitState.Closed;
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
            if (_circuitState == CircuitBreaker.CircuitState.Open && _circuitOpenTime.HasValue)
            {
                if (DateTime.Now.Subtract(_circuitOpenTime.Value).Seconds < PauseWhenBreakerOpen)
                {
                    _circuitState = CircuitBreaker.CircuitState.PartiallyOpen;
                }
            }
        }

        public override void SetOpen()
        {
            _circuitOpenTime = DateTime.Now;
            _circuitState = CircuitBreaker.CircuitState.Open;
        }

        public override void SetClosed()
        {
            _circuitOpenTime = null;
            _circuitState = CircuitBreaker.CircuitState.Closed;
        }
    }
}