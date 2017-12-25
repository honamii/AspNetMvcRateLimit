using System.Collections.Generic;

namespace AspNetMvcRateLimit.Models
{
    public class IpRateLimitPolicies
    {
        public List<IpRateLimitPolicy> IpRules { get; set; }
    }
}
