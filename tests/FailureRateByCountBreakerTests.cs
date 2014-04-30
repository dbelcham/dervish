using System;
using System.Threading;
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
                throw new NotImplementedException();
            }
        }

        [TestFixture]
        public class When_the_list_of_past_calls_is_all_failures_and_the_current_call_is_a_success
        {
            private int _callCount = 0;

            [Test]
            public void it_should_take_the_the_threshold_number_of_calls_failing_to_open_the_breaker_again()
            {
                const int pauseTime = 2;
                const int requestHistoryBufferCount = 10;

                var breaker = new FailureRateByCountBreaker(10,pauseTime,requestHistoryBufferCount,50);
                var circuitBreaker = new CircuitBreaker(breaker);
                try
                {
                    circuitBreaker.Execute(SomeFailingCall);
                }
                catch (Exception)
                {
                    //swallow to setup open state
                }
                Assert.That(breaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Open));

                Thread.Sleep(pauseTime*2*1000);

                circuitBreaker.Execute(SomeSuccessfulCall);
                Assert.That(breaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Closed));

                _callCount = 0;
                try
                {
                    circuitBreaker.Execute(SomeFailingCall);
                }
                catch (Exception)
                {
                    //swallow
                }
                Assert.That(_callCount, Is.EqualTo(requestHistoryBufferCount-1));
            }

            private void SomeSuccessfulCall()
            {
                //nothing means success
            }

            private void SomeFailingCall()
            {
                _callCount++;
                throw new NotImplementedException();
            }
        }

        [TestFixture]
        public class When_a_list_of_successes_is_followed_by_failures
        {
            private int _counter;

            [Test]
            public void the_breaker_should_not_be_opened_until_enough_failures_have_occured()
            {
                _counter = 0;
                var breaker = new FailureRateByCountBreaker(10,2,10,50);
                var circuitBreaker = new CircuitBreaker(breaker);

                for (var i = 0; i < 10; i++)
                {
                    circuitBreaker.Execute(SomeSuccessfulCall);
                }

                try
                {
                    circuitBreaker.Execute(SomeFailingCall);
                }
                catch (Exception)
                {
                    //swallow
                }

                Assert.That(breaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Open));
                Assert.That(_counter, Is.EqualTo(5));
            }

            private void SomeFailingCall()
            {
                _counter++;
                throw new NotImplementedException();
            }

            private void SomeSuccessfulCall()
            {
                //success is empty
            }
        }
    }
}