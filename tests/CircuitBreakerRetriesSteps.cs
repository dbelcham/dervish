using System;
using System.Collections.Generic;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace dervish.Tests
{
    [Binding]
    public class CircuitBreakerRetriesSteps
    {
        private CircuitBreaker _circuitBreaker;
        private int _count = 0;
        private CircuitBreakerAggregateException _aggregatedException;
        private IList<Exception> _quietExceptions;
        private const int MaxRetries = 5;

        [Given(@"that the dependency call hits the retry threshold")]
        public void GivenThatTheDependencyCallHitsTheRetryThreshold()
        {
            _quietExceptions = new List<Exception>();
            _circuitBreaker = new CircuitBreaker(new CircuitBreakerOptions(5,5,MaxRetries));
            _circuitBreaker.QuietException += HandleQuietException;
        }

        private void HandleQuietException(object sender, Exception e)
        {
            _quietExceptions.Add(e);
        }

        [When(@"I attempt to call the dependency")]
        public void WhenIAttemptToCallTheDependency()
        {
            try
            {
                _circuitBreaker.Execute(CallDependency);
            }
            catch(CircuitBreakerAggregateException ex)
            {
                _aggregatedException = ex;
            }
        }

        private void CallDependency()
        {
            _count++;
            throw new Exception("dependency error");
        }

        [Then(@"the circuit breaker should retry the call the maximum permitted number of retries")]
        public void ThenTheCircuitBreakerShouldRetryTheCallTheMaximumPermittedNumberOfRetries()
        {
            Assert.That(_count, Is.EqualTo(MaxRetries));
        }

        [Then(@"the number of aggregated exceptions will equal the maximum permitted number of retries")]
        public void ThenTheNumberOfAggregatedExceptionsWillEqualTheMaximumPermittedNumberOfRetries()
        {
            Assert.That(_aggregatedException.InnerExceptions.Count, Is.EqualTo(MaxRetries));
        }

        [Given(@"that the dependency will fail before succeeding")]
        public void GivenThatTheDependencyWillFailBeforeSucceeding()
        {
            _quietExceptions = new List<Exception>();
            _circuitBreaker = new CircuitBreaker(new CircuitBreakerOptions(5, 5, MaxRetries));
            _circuitBreaker.QuietException += HandleQuietException;
        }

        [Then(@"the exceptions from the failures should be surfaced in full detail")]
        public void ThenTheExceptionsFromTheFailuresShouldBeSurfacedInFullDetail()
        {
            Assert.That(_quietExceptions.Count, Is.EqualTo(MaxRetries-1));
        }

        [When(@"I successfully call the dependency")]
        public void WhenISuccessfullyCallTheDependency()
        {
            _circuitBreaker.Execute(CallDependencyFailingBeforeSucceeding);
        }

        private void CallDependencyFailingBeforeSucceeding()
        {
            _count++;
            if (_count < MaxRetries)
            {
                throw new DivideByZeroException("dependency error");
            }
        }
    }
}
