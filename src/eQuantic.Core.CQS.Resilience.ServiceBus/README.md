# eQuantic.Core.CQS.Resilience.ServiceBus

[![NuGet](https://img.shields.io/nuget/v/eQuantic.Core.CQS.Resilience.ServiceBus.svg)](https://www.nuget.org/packages/eQuantic.Core.CQS.Resilience.ServiceBus/)

Azure Service Bus dead letter handler for eQuantic CQS resilience.

## Installation

```bash
dotnet add package eQuantic.Core.CQS.Resilience.ServiceBus
```

## Configuration

```csharp
services.AddCqs(options =>
{
    options.UseResilience<MySaga>();
    options.UseServiceBusDeadLetter(sb =>
    {
        sb.ConnectionString = "Endpoint=sb://...";
        sb.QueueName = "saga-deadletter";
        sb.TimeToLive = TimeSpan.FromDays(30);
    });
});
```

### With existing client

```csharp
var client = new ServiceBusClient("...");

services.AddCqs(options =>
{
    options.UseServiceBusDeadLetter(client, sb =>
    {
        sb.QueueName = "saga-deadletter";
    });
});
```

## Features

- Send failed sagas to Service Bus queue
- Message properties with saga metadata
- Configurable TTL
- Integration with Azure dead letter queues
