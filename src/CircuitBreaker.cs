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
            if (_options.IsOpen()) throw new CircuitOpenException();

            _options.SetPartiallyOpen();
            
            var exceptions = new List<Exception>();

            do
            {
                _options.Trying();
                try
                {
                    var result = functionToRun.Invoke();
                    _options.SetClosed();
                    return result;
                }
                catch (Exception ex)
                {
                    RaiseQuietException(ex);
                    if (_options.IsPartiallyOpen())
                    {
                        throw new CircuitOpenException();
                    }
                    _options.FailureOccurred();
                    exceptions.Add(ex);
                    Thread.Sleep(_options.PauseBetweenCalls);
                }
            } while (_options.TryAgain());

            _options.SetOpen();

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
            if (_options.IsOpen()) throw new CircuitOpenException();

            _options.SetPartiallyOpen();

            var exceptions = new List<Exception>();

            do
            {
                _options.Trying();
                try
                {
                    methodToRun.Invoke();
                    _options.SetClosed();
                }
                catch (Exception ex)
                {
                    _options.FailureOccurred();
                    RaiseQuietException(ex);
                    if (_options.IsPartiallyOpen())
                    {
                        throw new CircuitOpenException();
                    }

                    exceptions.Add(ex);
                    Thread.Sleep(_options.PauseBetweenCalls);
                }
            } while (_options.TryAgain());

            _options.SetOpen();

            throw new CircuitBreakerAggregateException(exceptions);
        }

        public enum CircuitState
        {
            Open,
            Closed,
            PartiallyOpen
        }
    }
}