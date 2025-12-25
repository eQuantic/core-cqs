namespace eQuantic.Core.CQS.Abstractions.Queries;

/// <summary>
/// Marker interface for queries that return a result
/// </summary>
/// <typeparam name="TResult">The result type</typeparam>
public interface IQuery<TResult> where TResult : class
{
}