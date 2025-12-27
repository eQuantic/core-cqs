# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### New Packages

- **`eQuantic.Core.CQS.OpenTelemetry`** - OpenTelemetry integration for distributed tracing:

  - `ICqsTelemetry` abstraction with `NullCqsTelemetry` null object pattern
  - `TracingCommandHandlerDecorator` - traces command execution
  - `TracingQueryHandlerDecorator` - traces query execution
  - `TracingSagaRepositoryDecorator` - traces saga operations
  - `TracingOutboxPublisherDecorator` - traces outbox publishing
  - Fluent configuration via `UseOpenTelemetry()`

- **`eQuantic.Core.CQS.Resilience`** - Saga timeout and compensation handling:
  - `ISagaTimeoutPolicy` / `DefaultSagaTimeoutPolicy`
  - `ICompensationHandler<T>` with delegate-based and class-based options
  - `IDeadLetterHandler` / `LoggingDeadLetterHandler`
  - `SagaTimeoutBackgroundService` - monitors and handles saga timeouts
  - Fluent configuration via `UseResilience()`, `WithCompensation<>()`, `WithDeadLetterHandler<>()`

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
