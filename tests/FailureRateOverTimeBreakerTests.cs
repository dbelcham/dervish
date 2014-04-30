using System;
using NUnit.Framework;

namespace dervish.Tests
{
    public class FailureRateOverTimeBreakerTests
    {
        [TestFixture]
        public class When_the_failure_percentage_surpasses_the_threshold_in_the_time_window
        {
            [Test]
            public void the_breaker_should_be_opened()
            {
                var breaker = new FailureRateOverTimeBreaker(10,2,new TimeSpan(0,0,3),50,20);
                var circuitBreaker = new CircuitBreaker(breaker);
                try
                {
                    circuitBreaker.Execute(SomeFailingCall);
                }
                catch (Exception)
                {
                    //failure tastes good
                }
                Assert.That(breaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Open));
            }

            private void SomeFailingCall()
            {
                throw new NotImplementedException();
            }
        }
    }
}