using System;
using System.IO;
using System.Text;
using System.Web;

namespace AspNetMvcRateLimit.Logger
{
    public class FileLogger : ILogger
    {
        private readonly string _basePath;
        private const string DirectoryName = "AspNetMvcRateLimit/log";
        private static readonly object LockObject = new object();

        public FileLogger()
        {
            _basePath = HttpContext.Current.ApplicationInstance.Server.MapPath("~/App_Data");

            _basePath = Path.Combine(_basePath, DirectoryName);

            if (!Directory.Exists(DirectoryName))
                Directory.CreateDirectory(_basePath);
        }

        public void Log(string log)
        {
            lock (LockObject)
            {
                File.AppendAllText(GetCurrentPath(),  log + "\r\n", Encoding.UTF8);
            }
        }

        private string GetCurrentPath()
        {
            return Path.Combine(_basePath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
        }
    }
}
