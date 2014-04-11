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
        private Action _dependency;
        private const int MaxRetries = 5;

        [Given(@"that the dependency returns void")]
        public void GivenThatTheDependencyReturnsVoid()
        {
            _dependency = CallDependency;
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
            catch(Exception){}
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
    }
}
