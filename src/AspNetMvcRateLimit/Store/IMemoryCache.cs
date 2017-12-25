using System;
using System.Collections.Generic;

namespace AspNetMvcRateLimit.Store
{
    public interface IMemoryCache
    {
        IEnumerable<string> GetAllCachesKey();

        /// <summary>
        /// Get cached value.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Cache key</param>
        T Get<T>(string key);

        /// <summary>
        /// Set a value to the cache.
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="cacheTime">Time to cache.</param>
        void Set<TValue>(string key, TValue value, TimeSpan cacheTime);

        /// <summary>
        /// Set a value to the cache. if cache contains specified key then reset it
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Cache value</param>
        /// <param name="cacheTime">Time to cache.</param>
        void SetOrReset<TValue>(string key, TValue value, TimeSpan cacheTime);

        /// <summary>
        /// Check if a cache is set or not.
        /// </summary>
        /// <param name="key">Cache key</param>
        bool IsSet(string key);

        /// <summary>
        /// Remove a cached item.
        /// </summary>
        /// <param name="key">Cache key</param>
        void Remove(string key);

        /// <summary>
        /// Clear all items from the cache.
        /// </summary>
        void Clear();

        long GetMemoryLimitSize();

        bool TryGetValue<TItem>(string key, out TItem value);
    }
}
