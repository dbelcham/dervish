using System;
using System.Threading;
using NUnit.Framework;

namespace dervish.Tests
{
    public class LoopingTests
    {
        [TestFixture]
        public class When_looping_over_a_method_call
        {
            private int _callCount;

            [SetUp]
            public void TestSetup()
            {
                _callCount = 0;
            }

            [Test][ExpectedException(typeof(CircuitBreakerAggregateException))]
            public void a_failing_dependency_should_loop_the_maximum_number_of_times()
            {
                const int maxTries = 5;
                var circuitBreaker = new CircuitBreaker(new ConsecutiveFailureBreaker(10,10,maxTries));

                circuitBreaker.Execute(TestAction);

                Assert.That(_callCount, Is.EqualTo(maxTries));
            }

            [Test][ExpectedException(typeof(CircuitOpenException))]
            public void an_open_circuit_should_not_attempt_to_call_the_dependency()
            {
                const int maxTries = 5;
                var circuitBreaker = new CircuitBreaker(new ConsecutiveFailureBreaker(10, 10, maxTries));

                try
                {
                    circuitBreaker.Execute(TestAction);
                }
                catch (Exception)
                {
                    //swallow exception to setup a open state
                    //reset call count
                    _callCount = 0;
                }

                circuitBreaker.Execute(TestAction);

                Assert.That(_callCount, Is.EqualTo(0));
            }

            [Test][ExpectedException(typeof(CircuitOpenException))]
            public void an_open_circuit_should_attempt_one_call_after_the_pause_period_has_passed()
            {
                const int maxTries = 5;
                const int pausePeriod = 100;
                var circuitBreaker = new CircuitBreaker(new ConsecutiveFailureBreaker(10, pausePeriod, maxTries));

                try
                {
                    circuitBreaker.Execute(TestAction);
                }
                catch (Exception)
                {
                    //swallow exception to setup a open state
                    //reset call count
                    _callCount = 0;
                }
                Thread.Sleep(pausePeriod*2);
                circuitBreaker.Execute(TestAction);

                Assert.That(_callCount, Is.EqualTo(1));
            }

            private void TestAction()
            {
                _callCount++;
                throw new Exception("oops");
            }
        }

        [TestFixture]
        public class When_looping_over_a_function_call
        {
            private int _callCount;

            [SetUp]
            public void TestSetup()
            {
                _callCount = 0;
            }

            [Test]
            [ExpectedException(typeof(CircuitBreakerAggregateException))]
            public void a_failing_dependency_should_loop_the_maximum_number_of_times()
            {
                const int maxTries = 5;
                var circuitBreaker = new CircuitBreaker(new ConsecutiveFailureBreaker(10, 1, maxTries));

                circuitBreaker.Execute<bool>(TestAction);

                Assert.That(_callCount, Is.EqualTo(maxTries));
            }

            [Test]
            [ExpectedException(typeof(CircuitOpenException))]
            public void an_open_circuit_should_not_attempt_to_call_the_dependency()
            {
                const int maxTries = 5;
                var circuitBreaker = new CircuitBreaker(new ConsecutiveFailureBreaker(10, 10, maxTries));

                try
                {
                    circuitBreaker.Execute<bool>(TestAction);
                }
                catch (Exception)
                {
                    //swallow exception to setup a open state
                    //reset call count
                    _callCount = 0;
                }

                circuitBreaker.Execute<bool>(TestAction);

                Assert.That(_callCount, Is.EqualTo(0));
            }

            [Test]
            [ExpectedException(typeof(CircuitOpenException))]
            public void an_open_circuit_should_attempt_one_call_after_the_pause_period_has_passed()
            {
                const int maxTries = 5;
                const int pausePeriod = 1;
                var circuitBreaker = new CircuitBreaker(new ConsecutiveFailureBreaker(10, pausePeriod, maxTries));

                try
                {
                    circuitBreaker.Execute<bool>(TestAction);
                }
                catch (Exception)
                {
                    //swallow exception to setup a open state
                    //reset call count
                    _callCount = 0;
                }
                Thread.Sleep(pausePeriod * 2 * 1000);
                circuitBreaker.Execute<bool>(TestAction);

                Assert.That(_callCount, Is.EqualTo(1));
            }

            private bool TestAction()
            {
                _callCount++;
                throw new Exception("oops");
            }
        }

    }
}