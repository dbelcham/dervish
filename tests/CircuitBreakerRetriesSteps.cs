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
        private CircuitBreakerAggregateException _aggregateException;

        [Given(@"that the dependency returns void"), Scope(Feature = "CircuitBreakerActionRetries"), Scope(Feature="CircuitBreakerActionExceptionThrowing")]
        public void GivenThatTheDependencyReturnsVoid()
        {
        }

        [Given(@"that the dependency is broken"), Scope(Feature="CircuitBreakerActionRetries"), Scope(Feature="CircuitBreakerActionExceptionThrowing")]
        public void GivenThatTheDependencyIsBroken()
        {
            _actionDependency = CallBrokenAction;
        }

        [Given(@"that the dependency is intermittently broken"), Scope(Feature = "CircuitBreakerActionRetries"), Scope(Feature = "CircuitBreakerActionExceptionThrowing")]
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

        [When(@"I attempt to call the dependency"), Scope(Feature = "CircuitBreakerActionRetries"), Scope(Feature = "CircuitBreakerActionExceptionThrowing")]
        public void WhenIAttemptToCallTheDependency()
        {
            try
            {
                _circuitBreaker.Execute(_actionDependency);
            }
            catch (CircuitBreakerAggregateException ex)
            {
                _errorOccurred = true;
                _aggregateException = ex;
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

        [Then(@"an aggregate exception should be raised")]
        public void ThenAnAggregateExceptionShouldBeRaised()
        {
            Assert.That(_aggregateException, Is.Not.Null);
        }

        [Then(@"the aggregate exception should contain the same number of exceptions as retries")]
        public void ThenTheAggregateExceptionShouldContainTheSameNumberOfExceptionsAsRetries()
        {
            Assert.That(_aggregateException.InnerExceptions.Count, Is.EqualTo(MaxRetries));
        }

        [Then(@"the dependency call should succeed")]
        public void ThenTheDependencyCallShouldSucceed()
        {
            Assert.That(_aggregateException, Is.Null);
        }

        [Then(@"there should be quiet exceptions equalling the number of failures before success")]
        public void ThenThereShouldBeQuietExceptionsEquallingTheNumberOfFailuresBeforeSuccess()
        {
            Assert.That(_quietExceptionCount, Is.EqualTo(MaxRetries-1));
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