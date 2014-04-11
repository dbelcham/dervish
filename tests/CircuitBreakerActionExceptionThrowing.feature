Feature: CircuitBreakerActionExceptionThrowing
	In order to convey errors
	As a developer
	I want to have exceptions raised

Scenario: When calling a broken dependency through the circuit breaker
	Given that the dependency call hits the retry threshold
		And that the dependency returns void
		And that the dependency is broken
	When I attempt to call the dependency
	Then an aggregate exception should be raised
		And the aggregate exception should contain the same number of exceptions as retries

Scenario: When calling an intermittently working dependency through the circuit breaker
	Given that the dependency call succeeds after some failures
		And that the dependency returns void
		And that the dependency is intermittently broken
	When I attempt to call the dependency
	Then the dependency call should succeed
		And there should be quiet exceptions equalling the number of failures before success
	