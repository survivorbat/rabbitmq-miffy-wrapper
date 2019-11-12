using Microsoft.Extensions.Hosting;
using Minor.Miffy.MicroServices;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Minor.Miffy;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;
using VoorbeeldMicroService.DAL;

namespace VoorbeeldMicroService
{
    public class Program
    {
//        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureMicroServiceHostDefaults(hostBuilder =>
                {
                    var contextBuilder = new RabbitMqContextBuilder()
                        .WithExchange("MVM.EventExchange")
                        .WithConnectionString("amqp://guest:guest@localhost");  
            
                    using IBusContext<IConnection> context = contextBuilder.CreateContext();

                    var builder = new MicroserviceHostBuilder()
                        .WithBusContext(context)
                        .RegisterDependencies(services =>
                        {
                            services.AddDbContext<PolisContext>(e => e.UseSqlite(":memory:"));
                        })
                        .UseConventions();

                    builder.CreateHost();
                });
    }
}
