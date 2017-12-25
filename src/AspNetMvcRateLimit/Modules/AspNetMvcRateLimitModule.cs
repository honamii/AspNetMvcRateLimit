using System;
using System.Web;

namespace AspNetMvcRateLimit.Modules
{
    public class AspNetMvcRateLimitModule : IHttpModule
    {


        public void Dispose()
        {

        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += new EventHandler(this.Application_BeginRequest);
        }

        private void Application_BeginRequest(Object sender, EventArgs e)
        {
            HttpContext httpContext = ((HttpApplication)sender).Context;
            
            IpRateLimit.RequestProcessing(httpContext);

            ClientRateLimit.RequestProcessing(httpContext);
        }
    }
}
