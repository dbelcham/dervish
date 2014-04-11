using System;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace dervish.Tests
{
    [Binding]
    public class CircuitBreakerRetriesSteps
    {
        private const int MaxRetries = 5;
        private CircuitBreaker _circuitBreaker;
        private int _count;
        private Action _actionDependency;
        private bool _errorOccurred;
        private int _quietExceptionCount;
        private Func<bool> _funcDependency ;

        [Given(@"that the dependency returns void"), Scope(Feature = "CircuitBreakerActionRetries")]
        public void GivenThatTheDependencyReturnsVoid()
        {
        }

        [Given(@"that the dependency is broken"), Scope(Feature="CircuitBreakerActionRetries")]
        public void GivenThatTheDependencyIsBroken()
        {
            _actionDependency = CallBrokenAction;
        }

        [Given(@"that the dependency is intermittently broken"), Scope(Feature = "CircuitBreakerActionRetries")]
        public void GivenThatTheDependencyIsIntermittentlyBroken()
        {
            _actionDependency = CallIntermittentlyBrokenAction;
        }

        [Given(@"that the dependency is intermittently broken"), Scope(Feature = "CircuitBreakerFuncRetries")]
        public void GivenThatTheFuncDependencyIsIntermittentlyBroken()
        {
            _funcDependency = CallIntermittentlyBrokenFunc;
        }
        
        [Given(@"that the dependency call hits the retry threshold")]
        public void GivenThatTheDependencyCallHitsTheRetryThreshold()
        {
            _circuitBreaker = new CircuitBreaker(new CircuitBreakerOptions(5, 5, MaxRetries));
        }

        [When(@"I attempt to call the dependency"), Scope(Feature = "CircuitBreakerActionRetries")]
        public void WhenIAttemptToCallTheDependency()
        {
            try
            {
                _circuitBreaker.Execute(_actionDependency);
            }
            catch (Exception)
            {
                _errorOccurred = true;
            }
        }

        private void CallBrokenAction()
        {
            _count++;
            throw new Exception("dependency error");
        }

        private void CallIntermittentlyBrokenAction()
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

        [Then(@"the number of silent errors should be less than the maximum number of retries")]
        public void ThenTheNumberOfSilentErrorsShouldBeLessThanTheMaximumNumberOfRetries()
        {
            Assert.That(_quietExceptionCount, Is.LessThan(MaxRetries));
        }

        [Given(@"that the dependency is a func"), Scope(Feature = "CircuitBreakerFuncRetries")]
        public void GivenThatTheDependencyIsAFunc()
        {
        }

        [Given(@"that the dependency is broken"), Scope(Feature = "CircuitBreakerFuncRetries")]
        public void GivenThatTheFuncDependencyIsBroken()
        {
            _funcDependency = CallBrokenFunc;
        }

        [When(@"I attempt to call the dependency"), Scope(Feature="CircuitBreakerFuncRetries")]
        public void WhenIAttemptToCallTheFuncDependency()
        {
            try
            {
                var result = _circuitBreaker.Execute(_funcDependency);
            }
            catch (Exception)
            {
                _errorOccurred = true;
            }
        }
        
        public bool CallBrokenFunc()
        {
            _count++;
            throw new Exception("broken dependency");
        }


        private bool CallIntermittentlyBrokenFunc()
        {
            _count++;
            if (_count < MaxRetries)
            {
                throw new Exception("broken dependency");
            }
            return true;
        }

    }
}