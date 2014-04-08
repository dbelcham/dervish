using System;
using System.Collections.Generic;

namespace dervish
{
    public class CircuitBreakerAggregateException : AggregateException
    {
        public CircuitBreakerAggregateException(IEnumerable<Exception> exceptions):base(exceptions)
        {
        }
    }
}