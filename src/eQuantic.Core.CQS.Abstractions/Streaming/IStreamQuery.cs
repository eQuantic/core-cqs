namespace eQuantic.Core.CQS.Abstractions.Streaming;

/// <summary>
/// Marker interface for stream queries that return data as IAsyncEnumerable
/// </summary>
/// <typeparam name="TResult">The result item type</typeparam>
public interface IStreamQuery<out TResult>
{
}
