using System;
using System.Threading;
using NUnit.Framework;

namespace dervish.Tests
{
    public class BasicLoopBreakerTests
    {
        [TestFixture]
        public class When_initially_created
        {
            [Test]
            public void the_breaker_should_be_closed()
            {
                var basicLoopBreaker = new BasicLoopBreaker(10,10,10);
                Assert.That(basicLoopBreaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Closed));
            }
        }

        [TestFixture]
        public class When_partially_opening_a_closed_breaker
        {
            [Test]
            public void the_breaker_should_remain_closed()
            {
                var basicLoopBreaker = new BasicLoopBreaker(10,10,10);
                basicLoopBreaker.SetPartiallyOpen();
                Assert.That(basicLoopBreaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Closed));
            }
        }

        [TestFixture]
        public class When_partially_opening_an_open_breaker_that_has_not_waited_the_full_pause_period
        {
            [Test]
            public void the_breaker_should_remain_open()
            {
                var basicLoopBreaker = new BasicLoopBreaker(10,1000,3);
                var circuitBreaker = new CircuitBreaker(basicLoopBreaker);
                try
                {
                    circuitBreaker.Execute(SomeAction);
                }
                catch (Exception)
                {
                    //swallow to setup open state
                }
                Assert.That(basicLoopBreaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Open));

                basicLoopBreaker.SetPartiallyOpen();

                Assert.That(basicLoopBreaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Open));

            }

            private void SomeAction()
            {
                throw new System.NotImplementedException();
            }
        }

        [TestFixture]
        public class When_partially_opening_an_open_breaker_that_has_waited_the_full_pause_periodd
        {
            [Test]
            public void the_breaker_should_be_put_in_the_partially_open_state()
            {
                const int waitTime = 3;
                var basicLoopBreaker = new BasicLoopBreaker(10,waitTime,3);
                var circuitBreaker = new CircuitBreaker(basicLoopBreaker);
                try
                {
                    circuitBreaker.Execute(SomeCall);
                }
                catch (Exception)
                {
                    //swallow to setup open state
                }

                Assert.That(basicLoopBreaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.Open));

                Thread.Sleep(waitTime*2*1000);

                basicLoopBreaker.SetPartiallyOpen();
                Assert.That(basicLoopBreaker.CircuitState, Is.EqualTo(CircuitBreaker.CircuitState.PartiallyOpen));
            }

            public void SomeCall()
            {
                throw new NotImplementedException();
            }
        }


    }
}