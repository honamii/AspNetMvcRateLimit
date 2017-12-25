using System;
using System.Linq;
using System.Web;
using AspNetMvcRateLimit.Core;
using AspNetMvcRateLimit.Logger;
using AspNetMvcRateLimit.Models;
using AspNetMvcRateLimit.Net;
using AspNetMvcRateLimit.Store;

namespace AspNetMvcRateLimit
{
    public static class IpRateLimit
    {
        private static readonly ILogger Logger;
        private static readonly IIpAddressParser IpParser;
        private static readonly IpRateLimitProcessor Processor;
        private static readonly IpRateLimitPolicies Policies;
        private static readonly IpRateLimitOptions Options;
        private static readonly IMemoryCache MemoryCache;
        private static readonly bool Configurationed;
        static IpRateLimit()
        {
            if (Setting.Configuration?.IpRateLimiting == null)
            {
                return;
            }

            Logger = Factory.Logger.Value;

            Options = Setting.Configuration.IpRateLimiting;

            Policies = Setting.Configuration.IpRateLimitPolicies;

            IpParser = new ReversProxyIpParser(Options.RealIpHeader);

            MemoryCache = Factory.MemoryCache.Value;

            var rateLimitCounterStore = Factory.RateLimitCounterStore.Value;

            var ipPolicyStore = new MemoryCacheIpPolicyStore(MemoryCache, Options, Policies);

            Processor = new IpRateLimitProcessor(Options, rateLimitCounterStore, ipPolicyStore, IpParser);

            Configurationed = true;
        }

        public static void RequestProcessing(HttpContext httpContext)
        {
            if (!Configurationed)
                return;

            // check if rate limiting is enabled
            if (Options == null)
            {
                return;
            }

            // compute identity from request
            var identity = SetIdentity(httpContext);

            // check white list
            if (Processor.IsWhitelisted(identity))
            {
                return;
            }

            var rules = Processor.GetMatchingRules(identity);

            foreach (var rule in rules)
            {
                if (rule.Limit > 0)
                {
                    // increment counter
                    var counter = Processor.ProcessRequest(identity, rule);

                    // check if key expired
                    if (counter.Timestamp + rule.PeriodTimespan.Value < DateTime.UtcNow)
                    {
                        continue;
                    }

                    // check if limit is reached
                    if (counter.TotalRequests > rule.Limit)
                    {
                        //compute retry after value
                        var retryAfter = Processor.RetryAfterFrom(counter.Timestamp, rule);

                        // log blocked request
                        LogBlockedRequest(httpContext, identity, counter, rule);

                        // break execution
                        ReturnQuotaExceededResponse(httpContext, rule, retryAfter);

                        return;
                    }
                }
            }

            //set X-Rate-Limit headers for the longest period
            if (rules.Any() && !Options.DisableRateLimitHeaders)
            {
                var rule = rules.OrderByDescending(x => x.PeriodTimespan.Value).First();
                var headers = Processor.GetRateLimitHeaders(identity, rule);

                httpContext.Response.Headers["X-Rate-Limit-Limit"] = headers.Limit;
                httpContext.Response.Headers["X-Rate-Limit-Remaining"] = headers.Remaining;
                httpContext.Response.Headers["X-Rate-Limit-Reset"] = headers.Reset;
            }
        }

        private static ClientRequestIdentity SetIdentity(HttpContext httpContext)
        {
            var clientId = "anon";
            if (httpContext.Request.Headers.Get(Options.ClientIdHeader) != null)
            {
                clientId = httpContext.Request.Headers.GetValues(Options.ClientIdHeader).First();
            }

            var clientIp = string.Empty;
            try
            {
                var ip = IpParser.GetClientIp(httpContext);
                if (ip == null)
                {
                    throw new Exception("IpRateLimitMiddleware can't parse caller IP");
                }

                clientIp = ip.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("IpRateLimitMiddleware can't parse caller IP", ex);
            }

            return new ClientRequestIdentity
            {
                ClientIp = clientIp,
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.HttpMethod.ToLowerInvariant(),
                ClientId = clientId
            };
        }

        private static void ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
        {
            var message = string.IsNullOrEmpty(Options.QuotaExceededMessage) ? $"API calls quota exceeded! maximum admitted {rule.Limit} per {rule.Period}." : Options.QuotaExceededMessage;

            if (!Options.DisableRateLimitHeaders)
            {
                httpContext.Response.Headers["Retry-After"] = retryAfter;
            }

            httpContext.Response.StatusCode = Options.HttpStatusCode;

            httpContext.Response.Write(message);
        }

        private static void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            var log = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}\t{identity.ClientIp}\t{identity.HttpVerb}:{identity.Path}\t{identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.TotalRequests}. Blocked by rule {rule.Endpoint} .";

            Logger.Log(log);
        }

    }
}