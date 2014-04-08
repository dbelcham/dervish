Feature: CircuitBreakerRetries
	In order to retry on failure
	As a developer
	I want the circuit breaker to loop over the dependency call


Scenario: When calling a dependency through the circuit breaker
	Given that the dependency call hits the retry threshold
	When I attempt to call the dependency
	Then the circuit breaker should retry the call the maximum permitted number of retries

Scenario: When collecting exceptions on a circuit breaker that trips
	Given that the dependency call hits the retry threshold
	When I attempt to call the dependency
	Then the number of aggregated exceptions will equal the maximum permitted number of retries

Scenario: When a successfully calling a dependency
	Given that the dependency will fail before succeeding
	When I successfully call the dependency
	Then the exceptions from the failures should be surfaced in full detail