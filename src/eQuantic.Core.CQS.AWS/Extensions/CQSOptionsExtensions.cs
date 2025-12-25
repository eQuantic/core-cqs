using Amazon.SQS;
using eQuantic.Core.CQS.Abstractions.Options;
using eQuantic.Core.CQS.AWS.Options;
using eQuantic.Core.CQS.AWS.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.CQS.AWS.Extensions;

/// <summary>
/// CQSOptions extension methods for AWS SQS
/// </summary>
public static class CQSOptionsExtensions
{
    /// <summary>
    /// Configures AWS SQS as the outbox publisher
    /// </summary>
    /// <param name="options">The CQS options</param>
    /// <param name="configure">SQS configuration</param>
    /// <returns>The CQS options for chaining</returns>
    public static CQSOptions UseAwsSqs(
        this CQSOptions options, 
        Action<SqsOptions> configure)
    {
        var sqsOptions = new SqsOptions();
        configure(sqsOptions);

        options.Services.AddSingleton(sqsOptions);
        options.Services.AddSingleton<IAmazonSQS>(_ =>
        {
            if (!string.IsNullOrEmpty(sqsOptions.Region))
            {
                return new AmazonSQSClient(Amazon.RegionEndpoint.GetBySystemName(sqsOptions.Region));
            }
            return new AmazonSQSClient();
        });
        options.Services.AddSingleton<IOutboxPublisher, SqsOutboxPublisher>();

        return options;
    }

    /// <summary>
    /// Configures AWS SQS with an existing IAmazonSQS client
    /// </summary>
    public static CQSOptions UseAwsSqs(
        this CQSOptions options, 
        IAmazonSQS sqsClient,
        Action<SqsOptions> configure)
    {
        var sqsOptions = new SqsOptions();
        configure(sqsOptions);

        options.Services.AddSingleton(sqsOptions);
        options.Services.AddSingleton(sqsClient);
        options.Services.AddSingleton<IOutboxPublisher, SqsOutboxPublisher>();

        return options;
    }
}
