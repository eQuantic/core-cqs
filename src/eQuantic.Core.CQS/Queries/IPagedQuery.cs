namespace eQuantic.Core.CQS.Queries
{
    public interface IPagedQuery<TResult> : IQuery<TResult>
    {
        int PageIndex { get; }

        int PageSize { get; }
    }
}