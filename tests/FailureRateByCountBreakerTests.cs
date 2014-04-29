using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace dervish.Tests
{
    public class FailureRateByCountBreakerTests
    {
        [TestFixture]
        public class When_initially_created
        {
            [Test]
            public void the_breaker_should_be_closed()
            {
                var failureRateByCountBreaker = new FailureRateByCountBreaker(10,10,10,10);
                Assert.That(failureRateByCountBreaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Closed));
            }
        }

        [TestFixture]
        public class When_the_threshold_failure_percentage_has_occurred_within_the_specified_number_of_previous_calls
        {
            private int _callCount = 0;
            [Test]
            public void the_breaker_should_be_opened()  
            {
                var breaker = new FailureRateByCountBreaker(10,10,10,50.0);
                var circuitBreaker = new CircuitBreaker(breaker);

                try
                {
                    circuitBreaker.Execute(SomeCall);
                }
                catch (Exception)
                {
                    //swallow to setup state
                }

                Assert.That(breaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Open));
            }

            public void SomeCall()
            {
                _callCount++;
                throw new NotImplementedException();
            }
        }
    }
}