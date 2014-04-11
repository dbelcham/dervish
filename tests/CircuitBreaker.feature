Feature: CircuitBreakerRetries
	In order to retry on failure
	As a developer
	I want the circuit breaker to loop over the dependency call


Scenario: When calling a dependency through the circuit breaker
	Given that the dependency call hits the retry threshold
	And that the dependency returns void
	When I attempt to call the dependency
	Then the circuit breaker should retry the call the maximum permitted number of retries