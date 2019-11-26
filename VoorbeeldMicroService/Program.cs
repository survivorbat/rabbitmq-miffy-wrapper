using Minor.Miffy;
using Minor.Miffy.MicroServices;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;
using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minor.Miffy.MicroServices.Host;
using VoorbeeldMicroService.DAL;

namespace VoorbeeldMicroService
{
    /**
     * An example MicroserviceHost
     */
    class Program
    {
        /**
         * An example implementation on how to set up a
         * MicroserviceHost with RabbitMQ in the Main method
         *
         * Setting up a functioning RabbitMQ instance with docker is as easy as running:
         * docker run -d -p 15672:15672 -p 5672:5672 rabbitmq:3-management  
         */
        static void Main(string[] args)
        {
            /*
             * Logging is quite important, for this reason we first create
             * a loggerfactory that will be used in the rest of the application
             *
             * This one outputs to the console
             */
            using ILoggerFactory loggerFactory = LoggerFactory.Create(configure =>
            {
                configure.AddConsole().SetMinimumLevel(LogLevel.Information);
            });

            /*
             * To reach the library classes, a static class has been put in place
             * to register a logger
             */
            MiffyLoggerFactory.LoggerFactory = loggerFactory;
            RabbitMqLoggerFactory.LoggerFactory = loggerFactory;

            /*
             * Now that the logger is done, let's create a RabbitMq context with
             * an exchange called MVM.EventExchange and a connection string
             * that connects to a local RabbitMQ instance
             */
            RabbitMqContextBuilder contextBuilder = new RabbitMqContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithConnectionString("amqp://guest:guest@localhost");  
            
            /*
             * Now instantiate the context and ensure that it's disposed of by using a
             * 'using' statement.
             */
            using IBusContext<IConnection> context = contextBuilder.CreateContext();

            /**
             * Now create a builder that will build our microservice host.
             *
             * First, register our logger by setting the logger factory,
             * next we register any dependencies we might need, like a DBContext
             * that is injected into our CommandListeners and EventListeners
             *
             * Then, throw our context into the builder and lastly, ensure that
             * all our event/command listeners are registered by calling UseConventions().
             *
             * UseConventions could be replaced by multiple AddEventListener calls.
             */
            MicroserviceHostBuilder builder = new MicroserviceHostBuilder()
                .SetLoggerFactory(loggerFactory)
                .RegisterDependencies(services =>
                {
                    services.AddDbContext<PolisContext>(e =>
                    {
                        e.UseNpgsql("Host=localhost;Database=test;Username=root;Password=root");
                        e.UseLoggerFactory(loggerFactory);
                    });
                })
                .WithBusContext(context)
                .UseConventions();

            /**
             * Lastly, instantiate a host and ensure it starts
             */
            using var host = builder.CreateHost();
            host.Start();

            /**
             * Since the host runs in the background, we ensure
             * it keeps running by waiting for a never-ending manual
             * reset event.
             */
            new ManualResetEvent(false).WaitOne();
        }
    }
}
