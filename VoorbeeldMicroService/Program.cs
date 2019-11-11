using Microsoft.Extensions.Hosting;
using Minor.Miffy.MicroServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoorbeeldMicroService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureMicroServiceHostDefaults(hostBuilder =>
                {
                    //hostBuilder.UseStartup<Startup>();
                });
    }
}
