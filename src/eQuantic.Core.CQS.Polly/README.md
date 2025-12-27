# eQuantic.Core.CQS.Polly

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Polly.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Polly/)

Polly resilience provider for eQuantic CQS. Uses Polly v8+ for modern resilience patterns.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.Polly
```

## Configuration

### Retry behavior

```csharp
services.AddCqs(options =>
{
    options.UsePolly(polly =>
    {
        polly.MaxRetryAttempts = 3;
        polly.InitialDelay = TimeSpan.FromMilliseconds(100);
        polly.UseExponentialBackoff = true;
        polly.UseJitter = true;
        polly.ShouldRetry = ex => ex is HttpRequestException;
    });
});
```

### Saga timeout

```csharp
services.AddCqs(options =>
{
    options.UsePollyTimeout(saga =>
    {
        saga.DefaultTimeout = TimeSpan.FromMinutes(30);
        saga.EnableDeadLetter = true;
    });
});
```

## Features

- Retry with exponential backoff and jitter
- Circuit breaker integration
- Timeout policies for sagas
- Custom resilience pipelines
