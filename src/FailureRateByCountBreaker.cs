using System;
using System.Collections.Generic;
using System.Linq;

namespace dervish
{
    public class FailureRateByCountBreaker : CircuitBreakerOptions
    {
        private readonly int _lastXRequests;
        private readonly double _thresholdPercentage;
        private IList<Call> _previousCalls;

        public FailureRateByCountBreaker(int pauseBetweenCalls, int pauseWhenBreakerOpen, 
                                         int lastXRequests, double thresholdPercentage)
            : base(pauseBetweenCalls, pauseWhenBreakerOpen, 0)
        {
            _lastXRequests = lastXRequests;
            _thresholdPercentage = thresholdPercentage;
            _previousCalls = new List<Call>();
        }

        public override bool TryAgain()
        {
            return GetFailurePercentage() < _thresholdPercentage;
        }

        public override void Trying()
        {
            //do nothing
        }

        private double GetFailurePercentage()
        {
            if (_previousCalls.Count() < _lastXRequests) return 0;
            return (double)_previousCalls.Count(x => x == Call.Failure)/_lastXRequests * 100;
        }

        public override void SetOpen()
        {
            _previousCalls = new List<Call>();
            CircuitState = CircuitBreaker.CircuitState.Open;
            CircuitOpenTime = DateTime.Now;
        }

        public override void SetClosed()
        {
            AddCall(Call.Success);
            CircuitState = CircuitBreaker.CircuitState.Closed;
            CircuitOpenTime = null;
        }

        public override void FailureOccurred()
        {
            AddCall(Call.Failure);
        }

        private void AddCall(Call call)
        {
            var excessCalls = _previousCalls.Count - _lastXRequests;

            if (excessCalls > 0)
            {
                for (var i = 0; i < excessCalls; i++)
                {
                    _previousCalls.RemoveAt(0);
                }
            }

            _previousCalls.Add(call);
        }

        private enum Call
        {
            Success,
            Failure
        }
    }
}