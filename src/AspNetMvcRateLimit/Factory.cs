using System;
using AspNetMvcRateLimit.Logger;
using AspNetMvcRateLimit.Store;

namespace AspNetMvcRateLimit
{
    public static class Factory
    {
        public static Lazy<IMemoryCache> MemoryCache = new Lazy<IMemoryCache>(() => new MemoryCacheManager());

        public static Lazy<ILogger> Logger = new Lazy<ILogger>(() => new FileLogger());

        public static Lazy<IRateLimitCounterStore> RateLimitCounterStore = new Lazy<IRateLimitCounterStore>(() => new MemoryCacheRateLimitCounterStore(MemoryCache.Value));
    }
}