using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minor.Miffy;
using Minor.Miffy.MicroServices;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Host;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;
using VoorbeeldMicroService.Commands;
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
                    .SetMinimumLevel(LogLevel.Information);
            });

            MiffyLoggerFactory.LoggerFactory = loggerFactory;
            RabbitMqLoggerFactory.LoggerFactory = loggerFactory;

            var contextBuilder = new RabbitMqContextBuilder()
                .WithExchange("MVM.EventExchange")
                .WithConnectionString("amqp://guest:guest@localhost");

            using IBusContext<IConnection> context = contextBuilder.CreateContext();

            var builder = new MicroserviceHostBuilder()
                .SetLoggerFactory(loggerFactory)
                .WithBusContext(context);

            using var host = builder.CreateHost();
            host.Start();

            var newEvent = new PolisToegevoegdEvent
            {
                Polis = new Polis {Klantnaam = "Piet Jan"}
            };

            Task.Run(() =>
            {
                while (true)
                {
                    var messageSender = new EventPublisher(context);
                    messageSender.Publish(newEvent);
                    
                    Thread.Sleep(5000);
                }
            });
            
            while (true)
            {
                var polisCommand = new HaalPolissenOpCommand();
                var sender = new CommandPublisher(context);
                var result = sender.PublishAsync<HaalPolissenOpCommand>(polisCommand);

                foreach (var resultPolis in result.Result.Polisses)
                {
                    Console.WriteLine(resultPolis.Klantnaam);
                }
                
                Thread.Sleep(9000);
            }
        }
    }
}
