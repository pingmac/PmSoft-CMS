using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PmSoft
{
    /// <summary>
    /// Linq分页相关
    /// </summary>
    public static class PageLinqExtensions
    {

        public static PagedList<T> ToPagedList<T>(
                this IQueryable<T> allItems,
                int pageIndex,
                int pageSize
            )
        {
            if (pageIndex < 1)
                pageIndex = 1;
            var itemIndex = (pageIndex - 1) * pageSize;
            var totalItemCount = allItems.Count();
            while (totalItemCount <= itemIndex && pageIndex > 1)
            {
                itemIndex = (--pageIndex - 1) * pageSize;
            }
            var pageOfItems = allItems.Skip(itemIndex).Take(pageSize);
            return new PagedList<T>(pageOfItems, pageIndex, pageSize, totalItemCount);
        }

        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> allItems, int pageIndex, int pageSize)
        {
            return allItems.AsQueryable().ToPagedList(pageIndex, pageSize);
        }

        public static async Task<PagedList<T>> ToPagedListAsync<T>
        (
            this IQueryable<T> allItems,
            int pageIndex,
            int pageSize
        )
        {
            if (pageIndex < 1)
                pageIndex = 1;
            var itemIndex = (pageIndex - 1) * pageSize;
            var totalItemCount = allItems.Count();
            while (totalItemCount <= itemIndex && pageIndex > 1)
            {
                itemIndex = (--pageIndex - 1) * pageSize;
            }
            var query = allItems.Skip(itemIndex).Take(pageSize);
            var pageOfItems = await Task.Factory.StartNew(query.ToList);
            return new PagedList<T>(pageOfItems, pageIndex, pageSize, totalItemCount);
        }

        public static Task<PagedList<T>> ToPagedListAsync<T>(this IEnumerable<T> allItems, int pageIndex, int pageSize)
        {
            return allItems.AsQueryable().ToPagedListAsync(pageIndex, pageSize);
        }
    }
}