using System.Threading;
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
            var contextBuilder = new RabbitMqContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithConnectionString("amqp://guest:guest@localhost");  
            
            using IBusContext<IConnection> context = contextBuilder.CreateContext();

            var builder = new MicroserviceHostBuilder()
                    .WithBusContext(context);
            
            using var host = builder.CreateHost();
            host.Start();

            var polisToegevoegdEvent = new PolisToegevoegdEvent
            {
                Polis = new Polis {Klantnaam = "Marco Pill"}
            };
            
            var sender = new EventPublisher(context);
            
            sender.Publish(polisToegevoegdEvent);
        }
    }
}
