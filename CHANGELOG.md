# Changelog

## [3.0.0](https://github.com/eQuantic/core-cqs/compare/v2.5.0...v3.0.0) (2026-07-25)

### ⚠ BREAKING CHANGES

* **outbox:** `IOutboxRepository.MarkFailed` takes `maxAttempts` and
`backoff`; `IOutbox.Enqueue` takes an optional `context` ahead of the
cancellation token; `IOutboxMessage` gains `NextAttemptAt` and `Context`.
Custom implementations of either interface must be updated. Existing data
survives: the PostgreSQL table picks up its two columns through `ADD COLUMN
IF NOT EXISTS`, while EF Core deployments need a migration for the new
`NextAttemptAt`/`Context` properties and the widened outbox index.

### Features

* **outbox:** carry context, retry with backoff and claim under lock ([319abb9](https://github.com/eQuantic/core-cqs/commit/319abb920e7f31355e0be4547884f6a0ba04de8a))

## [2.5.0](https://github.com/eQuantic/core-cqs/compare/v2.4.0...v2.5.0) (2026-07-24)

### Features

* **data:** native eQuantic.Core.Data outbox adapter ([ccec351](https://github.com/eQuantic/core-cqs/commit/ccec3517ce2994db148fb1440bcde422e37685f5))

### Bug Fixes

* **tests:** guard the remaining fixture entry points ([cc9c798](https://github.com/eQuantic/core-cqs/commit/cc9c7984fb4d6c458266680adc682c0fe859cdae))
* **tests:** let container fixtures stand down when Docker is absent ([b62aa30](https://github.com/eQuantic/core-cqs/commit/b62aa30e2bf6b0c688274375e47eabb3137b701e))
* **tests:** require a Linux-container daemon before starting fixtures ([ca08d71](https://github.com/eQuantic/core-cqs/commit/ca08d71990cec932e3af6b117172db2b3610526e))
* **tests:** stop the Docker probe from outlasting its own timeout ([7878e32](https://github.com/eQuantic/core-cqs/commit/7878e32699b38dcb17f2f52c92214e3a59e7d844))

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### New Packages

- **`eQuantic.Core.CQS.OpenTelemetry`** - OpenTelemetry integration for distributed tracing:

  - `ICqsTelemetry` abstraction with `NullCqsTelemetry` null object pattern
  - Tracing decorators for commands, queries, sagas, and outbox
  - Fluent configuration via `UseOpenTelemetry()`

- **`eQuantic.Core.CQS.Resilience`** - Saga timeout and compensation handling:

  - `ISagaTimeoutPolicy` / `DefaultSagaTimeoutPolicy`
  - `ICompensationHandler<T>` with delegate-based and class-based options
  - `SagaTimeoutBackgroundService` for monitoring
  - Fluent configuration via `UseResilience()`, `WithCompensation<>()`

- **`eQuantic.Core.CQS.ApplicationInsights`** - Azure Application Insights telemetry provider:

  - `ApplicationInsightsTelemetryAdapter` implementing `ICqsTelemetry`
  - Distributed tracing and metrics integration
  - Fluent configuration via `UseApplicationInsights()`

- **`eQuantic.Core.CQS.Datadog`** - Datadog APM telemetry provider:

  - `DatadogTelemetryAdapter` implementing `ICqsTelemetry`
  - Distributed tracing with Datadog Trace SDK
  - Fluent configuration via `UseDatadog()`

- **`eQuantic.Core.CQS.Polly`** - Polly resilience integration:

  - `PollyRetryBehavior` for retry with exponential backoff
  - `PollySagaTimeoutPolicy` for saga timeout management
  - Fluent configuration via `UsePolly()`, `UsePollyTimeout()`

- **`eQuantic.Core.CQS.Resilience.Redis`** - Redis dead letter handler:

  - `RedisDeadLetterHandler` stores failed sagas in Redis list
  - Configurable key prefix and expiry
  - Fluent configuration via `UseRedisDeadLetter()`

- **`eQuantic.Core.CQS.Resilience.ServiceBus`** - Azure Service Bus dead letter handler:
  - `ServiceBusDeadLetterHandler` sends failed sagas to queue
  - Message properties with saga metadata
  - Fluent configuration via `UseServiceBusDeadLetter()`

#### Core.Eventing Integration

- **`INotification` now extends `IEvent`** from `eQuantic.Core.Eventing`
- Added `NotificationBase` class for easy notification implementation
- Updated dependency to `eQuantic.Core.Eventing` 1.8.1
- Enables ecosystem-wide event handling and interoperability

#### Abstractions

- Added `ICqsTelemetry` interface for telemetry abstraction
- Added `IResilientSagaData` interface for sagas with timeout/retry support
- Added `ISagaTimeoutPolicy`, `ICompensationHandler<T>`, `IDeadLetterHandler` interfaces
- Added `IOutboxPublisher` interface (consolidated from Azure/AWS)

#### Documentation

- Added individual `README.md` for each NuGet package with installation instructions, configuration examples, and usage documentation:
  - `eQuantic.Core.CQS.Abstractions`
  - `eQuantic.Core.CQS.Redis`
  - `eQuantic.Core.CQS.MongoDb`
  - `eQuantic.Core.CQS.PostgreSql`
  - `eQuantic.Core.CQS.EntityFramework`
  - `eQuantic.Core.CQS.Azure`
  - `eQuantic.Core.CQS.AWS`
  - `eQuantic.Core.CQS.Generators`
  - `eQuantic.Core.CQS.OpenTelemetry`
  - `eQuantic.Core.CQS.Resilience`

#### Test Coverage

- Added comprehensive test projects for all providers:
  - `eQuantic.Core.CQS.Tests.Commons` - Shared test utilities
  - `eQuantic.Core.CQS.Redis.Tests` - 11 integration tests
  - `eQuantic.Core.CQS.MongoDb.Tests` - 6 integration tests
  - `eQuantic.Core.CQS.PostgreSql.Tests` - 6 integration tests
  - `eQuantic.Core.CQS.EntityFramework.Tests` - 6 tests
  - `eQuantic.Core.CQS.Azure.Tests` - 4 unit tests
  - `eQuantic.Core.CQS.AWS.Tests` - 6 unit tests

### Fixed

- **MongoDbSagaRepository**: Added try-catch for `CreateIndex` to handle `[BsonId]` mapped SagaId

### Changed

- Consolidated `IOutboxPublisher` interface - moved from Azure/AWS to Abstractions
- Updated all `.csproj` files to include `PackageReadmeFile` property

## [1.0.0] - Initial Release

### Added

- Core CQS abstractions and interfaces
- Redis provider for Saga and Outbox patterns
- MongoDB provider for Saga and Outbox patterns
- PostgreSQL provider using Dapper
- Entity Framework Core provider
- Azure Service Bus outbox publisher
- AWS SQS outbox publisher
- Source generators for command/query handlers
