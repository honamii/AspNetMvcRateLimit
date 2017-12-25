using System.Collections.Generic;

namespace AspNetMvcRateLimit.Models
{
    public class ClientRateLimitPolicy
    {
        public string ClientId { get; set; }
        public List<RateLimitRule> Rules { get; set; }
    }
}
