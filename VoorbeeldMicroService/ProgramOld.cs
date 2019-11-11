using Minor.Miffy;
using Minor.Miffy.MicroServices;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;
using System;

namespace VoorbeeldMicroService
{
    class ProgramOld
    {
        /*static*/ void Main(string[] args)
        {
            var contextBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest")
                    .ReadFromEnvironmentVariables();    // beetje dubbel-op, misschien
            using IBusContext<IConnection> context = contextBuilder.CreateContext();

            var builder = new MicroserviceHostBuilder()
                    .WithBusContext(context)
                    .RegisterDependencies((services) =>
                    {
                        //services.AddDbContext<PolisContext>(...);
                    })
                    .UseConventions();
            using var host = builder.CreateHost();
            host.Start();

            Console.WriteLine("ServiceHost is listening to incoming events...");
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
        }
    }
}
