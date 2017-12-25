using AspNetMvcRateLimit.Models;

namespace AspNetMvcRateLimit
{
    public class Configuration
    {
        public IpRateLimitOptions IpRateLimiting { get; set; }

        public IpRateLimitPolicies IpRateLimitPolicies { get; set; }

        public ClientRateLimitOptions ClientRateLimiting { get; set; }

        public ClientRateLimitPolicies ClientRateLimitPolicies { get; set; }
    }
}