Feature: CircuitBreakerRetries
	In order to retry on failure
	As a developer
	I want the circuit breaker to loop over the dependency call


Scenario: When calling a broken dependency through the circuit breaker
	Given that the dependency call hits the retry threshold
		And that the dependency returns void
		And that the dependency is broken
	When I attempt to call the dependency
	Then the circuit breaker should retry the call the maximum permitted number of retries

Scenario: When calling an itermittently working dependency through the circuit breaker
	Given that the dependency call succeeds after some failures
		And that the dependency returns void
		And that the dependency is intermittently broken
	When I attempt to call the dependency
	Then the circuit breaker should retry the call until it gets a successful interaction
		And the number of silent errors should be greater than zero
		But the number of silent errors should not equal the maximum number of retries