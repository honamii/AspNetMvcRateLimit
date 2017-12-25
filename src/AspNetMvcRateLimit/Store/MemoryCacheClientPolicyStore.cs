using System;
using AspNetMvcRateLimit.Models;

namespace AspNetMvcRateLimit.Store
{
    public class MemoryCacheClientPolicyStore: IClientPolicyStore
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheClientPolicyStore(IMemoryCache memoryCache, 
            ClientRateLimitOptions options, 
            ClientRateLimitPolicies policies)
        {
            _memoryCache = memoryCache;

            //save client rules defined in appsettings in cache on startup
            if(options != null  && policies != null && policies != null && policies.ClientRules != null)
            {
                foreach (var rule in policies.ClientRules)
                {
                    Set($"{options.ClientPolicyPrefix}_{rule.ClientId}", new ClientRateLimitPolicy { ClientId = rule.ClientId, Rules = rule.Rules });
                }
            }
        }

        public void Set(string id, ClientRateLimitPolicy policy)
        {
            _memoryCache.Set(id, policy,TimeSpan.MaxValue);
        }

        public bool Exists(string id)
        {
            ClientRateLimitPolicy stored;
            return _memoryCache.TryGetValue(id, out stored);
        }

        public ClientRateLimitPolicy Get(string id)
        {
            ClientRateLimitPolicy stored;
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
