using System;
using AspNetMvcRateLimit.Models;

namespace AspNetMvcRateLimit.Store
{
    public class MemoryCacheIpPolicyStore : IIpPolicyStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheIpPolicyStore(IMemoryCache memoryCache, 
            IpRateLimitOptions options = null, 
            IpRateLimitPolicies policies = null)
        {
            _memoryCache = memoryCache;

            //save ip rules defined in appsettings in cache on startup
            if (options != null &&  policies != null && policies.IpRules != null)
            {
                Set($"{options.IpPolicyPrefix}", policies);
            }
        }

        public void Set(string id, IpRateLimitPolicies policy)
        {
            _memoryCache.Set<IpRateLimitPolicies>(id, policy,TimeSpan.MaxValue);
        }

        public bool Exists(string id)
        {
            IpRateLimitPolicies stored;
            return _memoryCache.TryGetValue(id, out stored);
        }

        public IpRateLimitPolicies Get(string id)
        {
            IpRateLimitPolicies stored;
            if (_memoryCache.TryGetValue(id, out stored))
            {
                return stored;
            }

            return null;
        }

        public void Remove(string id)
        {
            _memoryCache.Remove(id);
        }
    }
}
