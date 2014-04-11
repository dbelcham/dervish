using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace dervish.Tests
{
    [Binding]
    public class CircuitBreakerRetriesSteps
    {
        private CircuitBreaker _circuitBreaker;
        private int _count = 0;
        private Action _dependency;
        private bool _errorOccurred;
        private int _quietExceptionCount = 0;
        private const int MaxRetries = 5;

        [Given(@"that the dependency returns void")]
        public void GivenThatTheDependencyReturnsVoid()
        {
        }

        [Given(@"that the dependency is broken")]
        public void GivenThatTheDependencyIsBroken()
        {
            _dependency = CallBrokenDependency;
        }

        [Given(@"that the dependency is intermittently broken")]
        public void GivenThatTheDependencyIsIntermittentlyBroken()
        {
            _dependency = CallIntermittentlyBrokenDependency;
        }

        [Given(@"that the dependency call hits the retry threshold")]
        public void GivenThatTheDependencyCallHitsTheRetryThreshold()
        {
            _circuitBreaker = new CircuitBreaker(new CircuitBreakerOptions(5,5,MaxRetries));
        }

        [When(@"I attempt to call the dependency")]
        public void WhenIAttemptToCallTheDependency()
        {
            try
            {
                _circuitBreaker.Execute(_dependency);
            }
            catch (Exception)
            {
                _errorOccurred = true;
            }
        }

        private void CallBrokenDependency()
        {
            _count++;
            throw new Exception("dependency error");
        }

        private void CallIntermittentlyBrokenDependency()
        {
            _count++;
            if (_count != MaxRetries)
            {
                throw new Exception("dependency error");
            }
        }

        [Then(@"the circuit breaker should retry the call the maximum permitted number of retries")]
        public void ThenTheCircuitBreakerShouldRetryTheCallTheMaximumPermittedNumberOfRetries()
        {
            Assert.That(_count, Is.EqualTo(MaxRetries));
        }

        [Given(@"that the dependency call succeeds after some failures")]
        public void GivenThatTheDependencyCallSucceedsAfterSomeFailures()
        {
            _circuitBreaker = new CircuitBreaker(new CircuitBreakerOptions(5, 5, MaxRetries));
            _circuitBreaker.QuietException += QuietExceptionHandler;
        }

        private void QuietExceptionHandler(object sender, Exception e)
        {
            _quietExceptionCount++;
        }

        [Then(@"the circuit breaker should retry the call until it gets a successful interaction")]
        public void ThenTheCircuitBreakerShouldRetryTheCallUntilItGetsASuccessfulInteraction()
        {
            Assert.That(_errorOccurred, Is.False);
        }

        [Then(@"the number of silent errors should be greater than zero")]
        public void ThenTheNumberOfSilentErrorsShouldBeGreaterThanZero()
        {
            Assert.That(_quietExceptionCount, Is.GreaterThan(0));
        }

        [Then(@"the number of silent errors should not equal the maximum number of retries")]
        public void ThenTheNumberOfSilentErrorsShouldNotEqualTheMaximumNumberOfRetries()
        {
            Assert.That(_quietExceptionCount, Is.LessThan(MaxRetries));
        }

    }
}
