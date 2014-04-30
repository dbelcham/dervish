using System;
using System.Collections.Generic;
using System.Linq;

namespace dervish
{
    public class FailureRateOverTimeBreaker : CircuitBreakerOptions
    {
        private readonly TimeSpan _bufferDuration;
        private readonly int _minimumSampleSize;
        private readonly double _thresholdPercentage;
        private IList<CallLog> _callBuffer;

        public FailureRateOverTimeBreaker(int pauseBetweenCalls, int pauseWhenBreakerOpen,
                                            TimeSpan bufferDuration, double thresholdPercentage,
                                            int minimumSampleSize)
            : base(pauseBetweenCalls, pauseWhenBreakerOpen, 0)
        {
            _bufferDuration = bufferDuration;
            _thresholdPercentage = thresholdPercentage;
            _minimumSampleSize = minimumSampleSize;
            CircuitState = CircuitBreaker.CircuitState.Closed;
            _callBuffer = new List<CallLog>();
        }

        public override bool TryAgain()
        {
            return GetFailurePercentage() < _thresholdPercentage;
        }

        private double GetFailurePercentage()
        {
            if (_callBuffer.Count() < _minimumSampleSize) return 0;
            return _callBuffer.Count(x => x.Call == Call.Failure)/_callBuffer.Count()*100;
        }

        public override void Trying()
        {
            //do nothing
        }

        public override void SetOpen()
        {
            _callBuffer = new List<CallLog>();
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
            List<CallLog> outOfScopeCalls =
                _callBuffer.Where(x => x.CallTime.Subtract(DateTime.Now) > _bufferDuration).ToList();
            foreach (CallLog outOfScopeCall in outOfScopeCalls)
            {
                _callBuffer.Remove(outOfScopeCall);
            }
            _callBuffer.Add(new CallLog(call, DateTime.Now));
        }

        private enum Call
        {
            Failure,
            Success
        }

        private class CallLog
        {
            public CallLog(Call call, DateTime callTime)
            {
                Call = call;
                CallTime = callTime;
            }

            public DateTime CallTime { get; private set; }
            public Call Call { get; private set; }
        }
    }
}