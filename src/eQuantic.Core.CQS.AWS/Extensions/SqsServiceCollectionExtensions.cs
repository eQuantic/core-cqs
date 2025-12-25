using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.AWS;

/// <summary>
/// Extension methods for registering AWS SQS services
/// </summary>
public static class SqsServiceCollectionExtensions
{
    /// <summary>
    /// Adds AWS SQS Outbox Publisher to the service collection
    /// </summary>
    public static IServiceCollection AddCQSAwsSqs(
        this IServiceCollection services,
        Action<SqsOptions> configure)
    {
        var options = new SqsOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<IAmazonSQS>(_ =>
        {
            if (!string.IsNullOrEmpty(options.Region))
            {
                return new AmazonSQSClient(Amazon.RegionEndpoint.GetBySystemName(options.Region));
            }
            return new AmazonSQSClient();
        });
        services.AddSingleton<IOutboxPublisher, SqsOutboxPublisher>();

        return services;
    }

    /// <summary>
    /// Adds AWS SQS Outbox Publisher with existing IAmazonSQS client
    /// </summary>
    public static IServiceCollection AddCQSAwsSqs(
        this IServiceCollection services,
        IAmazonSQS sqsClient,
        Action<SqsOptions> configure)
    {
        var options = new SqsOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton(sqsClient);
        services.AddSingleton<IOutboxPublisher, SqsOutboxPublisher>();

        return services;
    }
}