# eQuantic.Core.CQS.ApplicationInsights

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.ApplicationInsights.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.ApplicationInsights/)

Azure Application Insights telemetry provider for eQuantic CQS.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.ApplicationInsights
```

## Configuration

### With existing TelemetryClient

```csharp
// First, register Application Insights
services.AddApplicationInsightsTelemetry();

// Then configure CQS
services.AddCqs(options =>
{
    options.UseApplicationInsights();
});
```

### With connection string

```csharp
services.AddCqs(options =>
{
    options.UseApplicationInsights("InstrumentationKey=...");
});
```

## Features

- Distributed tracing for commands, queries, sagas, and outbox
- Custom metrics recording
- Exception tracking with correlation
- Property tags on telemetry
