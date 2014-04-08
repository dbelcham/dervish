using System;
using System.Collections.Generic;
using System.Threading;

namespace dervish
{
    public class CircuitBreaker
    {
        private readonly CircuitBreakerOptions _options;
        private CircuitState _circuitState;
        private DateTime? _circuitOpenTime;

        public event EventHandler<Exception> QuietException; 

        public CircuitBreaker(CircuitBreakerOptions options)
        {
            _options = options;
            _circuitState = CircuitState.Closed;
        }

        public T Execute<T>(Func<T> functionToRun)
        {
            if (_circuitState == CircuitState.Open) throw new CircuitOpenException();

            if (_circuitState == CircuitState.Closed && _circuitOpenTime.HasValue)
            {
                if (DateTime.Now.Subtract(_circuitOpenTime.Value).Seconds < _options.PauseWhenBreakerOpen)
                {
                    _circuitState = CircuitState.PartiallyOpen;
                }
            }

            if (_circuitState == CircuitState.PartiallyOpen)
            {
                try
                {
                    var returnValue = functionToRun.Invoke();
                    _circuitOpenTime = null;
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
                    RaiseQuietException(ex);
                    Thread.Sleep(_options.PauseBetweenCalls);
                }
            }
            _circuitOpenTime = DateTime.Now;
            _circuitState = CircuitState.Open;
            throw new CircuitBreakerAggregateException(exceptions);        
        }

        private void RaiseQuietException(Exception exception)
        {
            if (QuietException != null)
            {
                QuietException(this, exception);
            }
        }

        public void Execute(Action methodToRun)
        {
            if (_circuitState == CircuitState.Open) throw new CircuitOpenException();

            if (_circuitState == CircuitState.Closed && _circuitOpenTime.HasValue)
            {
                if (DateTime.Now.Subtract(_circuitOpenTime.Value).Seconds < _options.PauseWhenBreakerOpen)
                {
                    _circuitState = CircuitState.PartiallyOpen;
                }
            }

            if (_circuitState == CircuitState.PartiallyOpen)
            {
                try
                {
                    methodToRun.Invoke();
                    _circuitOpenTime = null;
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
                    return;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    RaiseQuietException(ex);
                    Thread.Sleep(_options.PauseBetweenCalls);
                }
            }
            _circuitOpenTime = DateTime.Now;
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