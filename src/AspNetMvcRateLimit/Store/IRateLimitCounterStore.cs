using System;
using AspNetMvcRateLimit.Models;

namespace AspNetMvcRateLimit.Store
{
    public interface IRateLimitCounterStore
    {
        bool Exists(string id);
        RateLimitCounter? Get(string id);
        void Remove(string id);
        void Set(string id, RateLimitCounter counter, TimeSpan expirationTime);
    }
}