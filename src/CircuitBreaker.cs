using System;
using System.Collections.Generic;
using System.Threading;

namespace dervish
{
    public class CircuitBreaker
    {
        private readonly CircuitBreakerOptions _options;
        private CircuitState _circuitState;
        private DateTime? _circuitCloseTime;
        
        public CircuitBreaker(CircuitBreakerOptions options)
        {
            _options = options;
            _circuitState = CircuitState.Closed;
        }

        public T Execute<T>(Func<T> functionToRun)
        {
            if (_circuitState == CircuitState.Open) throw new CircuitOpenException();

            if (_circuitState == CircuitState.Closed && _circuitCloseTime.HasValue)
            {
                if (DateTime.Now.Subtract(_circuitCloseTime.Value).Seconds < _options.PauseWhenBreakerOpen)
                {
                    _circuitState = CircuitState.PartiallyOpen;
                }
            }

            if (_circuitState == CircuitState.PartiallyOpen)
            {
                try
                {
                    var returnValue = functionToRun.Invoke();
                    _circuitCloseTime = null;
                    _circuitState = CircuitState.Closed;

                    return returnValue;
                }
                catch (Exception ex)
                {
                    throw new CircuitPartiallyOpenException();
                }
            }

            var exceptions = new List<Exception>();

            for (var i = 0; i < _options.NumberOfRetries; i++)
            {
                try
                {
                    return functionToRun.Invoke();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(_options.PauseBetweenCalls);
                }
            }
            _circuitCloseTime = DateTime.Now;
            _circuitState = CircuitState.Open;
            throw new CircuitBreakerAggregateException(exceptions);        
        }

        public void Execute(Action methodToRun)
        {
            if (_circuitState == CircuitState.Open) throw new CircuitOpenException();

            if (_circuitState == CircuitState.Closed && _circuitCloseTime.HasValue)
            {
                if (DateTime.Now.Subtract(_circuitCloseTime.Value).Seconds < _options.PauseWhenBreakerOpen)
                {
                    _circuitState = CircuitState.PartiallyOpen;
                }
            }

            if (_circuitState == CircuitState.PartiallyOpen)
            {
                try
                {
                    methodToRun.Invoke();
                    _circuitCloseTime = null;
                    _circuitState = CircuitState.Closed;
                    return;
                }
                catch (Exception ex)
                {
                    throw new CircuitPartiallyOpenException();
                }
            }

            var exceptions = new List<Exception>();

            for (var i = 0; i < _options.NumberOfRetries; i++)
            {
                try
                {
                    methodToRun.Invoke();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(_options.PauseBetweenCalls);
                }
            }
            _circuitCloseTime = DateTime.Now;
            _circuitState = CircuitState.Open;
            throw new CircuitBreakerAggregateException(exceptions);        
        }

        private enum CircuitState
        {
            Open,
            Closed,
            PartiallyOpen
        }
    }
}