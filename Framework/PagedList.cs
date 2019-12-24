using System;
using System.Collections.Generic;
using System.Linq;

namespace PmSoft
{

    public class PagedList<T> : List<T>, IPagedList<T>
    {

        public PagedList(IEnumerable<T> allItems, int pageIndex, int pageSize)
        {
            PageSize = pageSize;
            var items = allItems as IList<T> ?? allItems.ToList();
            TotalItemCount = items.Count();
            CurrentPageIndex = pageIndex;
            AddRange(items.Skip(StartItemIndex - 1).Take(pageSize));
        }

        public PagedList(IEnumerable<T> currentPageItems, int pageIndex, int pageSize, long totalItemCount)
        {
            AddRange(currentPageItems);
            TotalItemCount = totalItemCount;
            CurrentPageIndex = pageIndex;
            PageSize = pageSize;
        }

        public PagedList(IQueryable<T> allItems, int pageIndex, int pageSize)
        {
            int startIndex = (pageIndex - 1) * pageSize;
            AddRange(allItems.Skip(startIndex).Take(pageSize));
            TotalItemCount = allItems.Count();
            CurrentPageIndex = pageIndex;
            PageSize = pageSize;
        }

        public PagedList(IQueryable<T> currentPageItems, int pageIndex, int pageSize, int totalItemCount)
        {
            AddRange(currentPageItems);
            TotalItemCount = totalItemCount;
            CurrentPageIndex = pageIndex;
            PageSize = pageSize;
        }

        public int CurrentPageIndex { get; set; }

        public int PageSize { get; set; }

        public long TotalItemCount { get; set; }

        public int TotalPageCount { get { return (int)Math.Ceiling(TotalItemCount / (double)PageSize); } }

        public int StartItemIndex { get { return (CurrentPageIndex - 1) * PageSize + 1; } }

        public int EndItemIndex { get { return (int)(TotalItemCount > CurrentPageIndex * PageSize ? CurrentPageIndex * PageSize : TotalItemCount); } }


        public double Duration { get; set; }
    }
}
