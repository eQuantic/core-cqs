# eQuantic.Core.CQS.Datadog

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Datadog.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Datadog/)

Datadog APM telemetry provider for eQuantic CQS.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.Datadog
```

## Configuration

```csharp
services.AddCqs(options =>
{
    options.UseDatadog("my-service-name");
});
```

## Requirements

- Datadog Agent running with APM enabled
- `DD_AGENT_HOST` environment variable set (or default localhost)

## Features

- Distributed tracing with Datadog APM
- Custom metrics as span metrics
- Exception tracking with stack traces
- Tag propagation
