using System;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Core.Infrastructure
{
    public interface ICacheProvider
    {
        T GetFromCache<T>(string key) where T : class;
        void SetCache<T>(string key, T value, int ttl) where T : class;
        void RemoveCache(string key);
    }

    public class CacheProvider : ICacheProvider
    {
        private readonly IMemoryCache _memoryCache;

        public CacheProvider(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public T GetFromCache<T>(string key) where T : class => (T) _memoryCache.Get(key);


        public void SetCache<T>(string key, T value, int ttl) where T : class =>
            _memoryCache.Set(key, value, DateTimeOffset.Now.AddSeconds(ttl));

        public void RemoveCache(string key) =>
            _memoryCache.Remove(key);
    }
}