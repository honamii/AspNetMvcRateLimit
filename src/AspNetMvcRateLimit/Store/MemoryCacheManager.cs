using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace AspNetMvcRateLimit.Store
{
    public class MemoryCacheManager : IMemoryCache
    {
        private static readonly object LockObject = new object();

        private static MemoryCache Cache => MemoryCache.Default;

        public IEnumerable<string> GetAllCachesKey()
        {
            lock (LockObject)
            {
                return Cache.Select(cache => cache.Key).ToList();
            }
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            T value;

            lock (LockObject)
            {
                value = (T)Cache.Get(key);
            }

            if (value == null)
            {
                return default(T);
            }

            return value;
        }

        public void Set<TValue>(string key, TValue value, TimeSpan cacheTime)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            //  If value is null then, no need to save anything
            if (value == null)
            {
                throw new Exception($"No value is set for cache key: {key}");
            }

            if (cacheTime.TotalMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cacheTime), "Cache time must be a positive number");
            }

            lock (LockObject)
            {
                if (IsSet(key))
                {
                    throw new Exception($"The cache with the key: {key} is already exists.");
                }

                var cacheItemPolicy = new CacheItemPolicy
                {
                    Priority = CacheItemPriority.Default,
                };

                if (cacheTime != TimeSpan.MaxValue)
                {
                    cacheItemPolicy.AbsoluteExpiration =
                        DateTimeOffset.Now.AddMilliseconds(cacheTime.TotalMilliseconds);
                }

                Cache.Add(key, value, cacheItemPolicy);
            }

        }

        public void SetOrReset<TValue>(string key, TValue value, TimeSpan cacheTime)
        {
            lock (LockObject)
            {
                if (IsSet(key))
                {
                    Remove(key);
                }

                Set(key, value, cacheTime);
            }
        }

        public bool IsSet(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (LockObject)
            {
                return Cache.Contains(key);
            }
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (LockObject)
            {
                Cache.Remove(key);
            }
        }

        public void Clear()
        {
            lock (LockObject)
            {
                foreach (var item in Cache)
                {
                    Remove(item.Key);
                }
            }
        }

        public long GetMemoryLimitSize()
        {
            return Cache.CacheMemoryLimit;
        }

        public bool TryGetValue<TItem>(string key, out TItem value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (LockObject)
            {
                if (Cache.Contains(key))
                {
                    value = (TItem)Cache.Get(key);
                    return true;
                }
            }

            value = default(TItem);

            return false;
        }
    }
}