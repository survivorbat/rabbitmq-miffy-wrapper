using System.Threading;
using Microsoft.Extensions.Logging;
using Minor.Miffy;
using Minor.Miffy.MicroServices;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService
{
    class Program
    {
        static void Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(configure =>
            {
                configure.AddConsole()
                    .SetMinimumLevel(LogLevel.Trace);
            });

            var contextBuilder = new RabbitMqContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithConnectionString("amqp://guest:guest@localhost");  
            
            using IBusContext<IConnection> context = contextBuilder.CreateContext();

            var builder = new MicroserviceHostBuilder()
                    .SetLoggerFactory(loggerFactory)
                    .WithBusContext(context);
            
            using var host = builder.CreateHost();
            host.Start();

            var polisToegevoegdEvent = new PolisToegevoegdEvent
            {
                Polis = new Polis {Klantnaam = "Marco Pill"}
            };
            
            var sender = new EventPublisher(context);
            while (true)
            {
                sender.Publish(polisToegevoegdEvent);
                Thread.Sleep(1000);
            }
        }
    }
}
