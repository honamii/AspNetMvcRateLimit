using System.Linq;
using System.Net;
using System.Web;

namespace AspNetMvcRateLimit.Net
{
    public class ReversProxyIpParser : RemoteIpParser
    {
        private readonly string _realIpHeader;

        public ReversProxyIpParser(string realIpHeader)
        {
            _realIpHeader = realIpHeader;
        }

        public override IPAddress GetClientIp(HttpContext context)
        {
            if (context.Request.Headers[_realIpHeader] != null)
            {
                return ParseIp(context.Request.Headers.GetValues(_realIpHeader).Last());
            }

            return base.GetClientIp(context);
        }
    }
}
