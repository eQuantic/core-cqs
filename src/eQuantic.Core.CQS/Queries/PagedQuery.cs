using System;

namespace eQuantic.Core.CQS.Queries
{
    public class PagedQuery<TQuery, TResult> : IPagedQuery<TResult> where TQuery : PagedQuery<TQuery, TResult>
    {
        public int PageIndex { get; private set; } = 1;

        public int PageSize { get; private set; } = 100;

        public TQuery SkipAndTake(uint skip, uint take)
        {
            if (take < 1) throw new ArgumentOutOfRangeException(nameof(take));

            PageIndex = Convert.ToInt32(Math.Floor((skip / (double)take) + 1));
            PageSize = (int)take;

            return (TQuery)this;
        }
    }
}