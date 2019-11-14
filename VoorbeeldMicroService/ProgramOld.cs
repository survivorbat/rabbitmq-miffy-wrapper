using Minor.Miffy;
using Minor.Miffy.MicroServices;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VoorbeeldMicroService.DAL;

namespace VoorbeeldMicroService
{
    class ProgramOld
    {
        static void Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(configure =>
            {
                configure.AddConsole().SetMinimumLevel(LogLevel.Trace);
            });

            MiffyLoggerFactory.LoggerFactory = loggerFactory;
            RabbitMqLoggerFactory.LoggerFactory = loggerFactory;

            var contextBuilder = new RabbitMqContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithConnectionString("amqp://guest:guest@localhost");  
            
            using IBusContext<IConnection> context = contextBuilder.CreateContext();

            var builder = new MicroserviceHostBuilder()
                .SetLoggerFactory(loggerFactory)
                .RegisterDependencies(services =>
                {
                    services.AddDbContext<PolisContext>(e =>
                    {
                        e.UseSqlite(":memory:");
                    });
                })
                .WithBusContext(context)
                .UseConventions();
            
            using var host = builder.CreateHost();
            host.Start();
            
            Console.WriteLine("ServiceHost is listening to incoming events...");
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }
    }
}
