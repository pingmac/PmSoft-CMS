using System.Collections;
using System.Collections.Generic;

namespace PmSoft
{
    public interface IPagedList : IEnumerable
    {
        int CurrentPageIndex { get; set; }

        int PageSize { get; set; }

        long TotalItemCount { get; set; }
    }

    public interface IPagedList<T> : IEnumerable<T>, IPagedList { }
}
