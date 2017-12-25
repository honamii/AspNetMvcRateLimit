using System;
using System.IO;
using System.Web;
using AspNetMvcRateLimit.Logger;

namespace AspNetMvcRateLimit
{
    public static class Setting
    {
        private static Configuration _configuration;

        public static Configuration Configuration => GetCounfiguration();

        private static Configuration GetCounfiguration()
        {
            if (_configuration != null)
                return _configuration;

            try
            {
                var _basePath = HttpContext.Current.ApplicationInstance.Server.MapPath("~/App_Data");

                var path = Path.Combine(_basePath, "AspNetMvcRateLimit", "config.json");

                var rawConfig = File.ReadAllText(path);

                _configuration = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration>(rawConfig);

            }
            catch (Exception exception)
            {
                ILogger logger = new FileLogger();

                logger.Log("config load failed! \r\n" + exception.Message);

                _configuration = new Configuration();
            }

            return _configuration;
        }
    }
}
