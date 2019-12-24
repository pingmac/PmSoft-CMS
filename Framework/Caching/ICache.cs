using System;
using System.Threading.Tasks;

namespace PmSoft.Caching
{
    public interface ICache
    {
        void Add(string key, object value, TimeSpan timeSpan);
        Task AddAsync(string key, object value, TimeSpan timeSpan);

        void Clear();
        Task ClearAsync();

        object Get(string cacheKey);
        Task<object> GetAsync(string cacheKey);

        void Remove(string cacheKey);
        Task RemoveAsync(string cacheKey);

        void Set(string key, object value, TimeSpan timeSpan);
        Task SetAsync(string key, object value, TimeSpan timeSpan);
    }
}
