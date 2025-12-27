# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

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

#### Test Coverage

- Added comprehensive test projects for all providers:
  - `eQuantic.Core.CQS.Tests.Commons` - Shared test utilities including `DockerAvailableFactAttribute`, `TestSagaData`, and `TestOutboxMessage`
  - `eQuantic.Core.CQS.Redis.Tests` - 11 integration tests using Testcontainers
  - `eQuantic.Core.CQS.MongoDb.Tests` - 6 integration tests using Testcontainers
  - `eQuantic.Core.CQS.PostgreSql.Tests` - 6 integration tests using Testcontainers
  - `eQuantic.Core.CQS.EntityFramework.Tests` - 6 tests using SQLite in-memory
  - `eQuantic.Core.CQS.Azure.Tests` - 4 unit tests with NSubstitute mocks
  - `eQuantic.Core.CQS.AWS.Tests` - 6 unit tests with NSubstitute mocks

### Fixed

- **MongoDbSagaRepository**: Added try-catch block for `CreateIndex` to handle cases where `SagaId` is mapped as `_id` via `[BsonId]` attribute (prevents `Command createIndexes failed` error)

### Changed

- Updated all `.csproj` files to include `PackageReadmeFile` property for NuGet package documentation
- Configured conditional package versions for `Microsoft.Extensions.DependencyInjection.Abstractions` across different target frameworks

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
