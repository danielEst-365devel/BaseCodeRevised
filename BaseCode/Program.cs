using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BaseCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<Startup>();
        //        });
        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            //    var config = new ConfigurationBuilder()
            //        .SetBasePath(Directory.GetCurrentDirectory())
            //        .AddJsonFile("appsettings.json", optional: false)
            //        .Build();

            var HOST = Environment.GetEnvironmentVariable("HOST");
            var PORT = Environment.GetEnvironmentVariable("PORT");

            var webHost = WebHost.CreateDefaultBuilder(args)
            .UseUrls($"http://{HOST}:{PORT}")
            .UseKestrel()
            .UseStartup<Startup>();

            return webHost;
        }

    }
}
