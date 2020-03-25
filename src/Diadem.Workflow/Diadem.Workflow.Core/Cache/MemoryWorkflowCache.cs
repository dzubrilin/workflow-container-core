using System;
using Microsoft.Extensions.Caching.Memory;

namespace Diadem.Workflow.Core.Cache
{
    public class InMemoryWorkflowCache : IWorkflowCache
    {
        private readonly IMemoryCache _memoryCache;

        public InMemoryWorkflowCache()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        public void Dispose()
        {
            _memoryCache?.Dispose();
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            if (!_memoryCache.TryGetValue(key, out var cacheValue))
            {
                value = default;
                return false;
            }

            value = (TValue) cacheValue;
            return true;
        }

        public void Put(string key, object value)
        {
            using (var cacheEntry = _memoryCache.CreateEntry(key))
            {
                cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(1D));
                cacheEntry.SetValue(value);
            }
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }
    }
}