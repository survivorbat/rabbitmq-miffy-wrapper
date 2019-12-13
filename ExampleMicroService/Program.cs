using System;
using System.Linq;
using System.Threading;
using ExampleMicroService.Commands;
using ExampleMicroService.DAL;
using ExampleMicroService.Events;
using ExampleMicroService.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minor.Miffy;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Host;
using Minor.Miffy.RabbitMQBus;
using RabbitMQ.Client;

namespace ExampleMicroService
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
                configure.AddConsole().SetMinimumLevel(LogLevel.Error);
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
             * Create a dummy database context for testing with an in-memory database
             */
            PolisContext databaseContext = new PolisContext();

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
            using MicroserviceHostBuilder builder = new MicroserviceHostBuilder()
                .SetLoggerFactory(loggerFactory)
                .RegisterDependencies(services =>
                {
                    services.AddSingleton(databaseContext);
                })
                .WithBusContext(context)
                .UseConventions();

            /**
             * Lastly, instantiate a host and ensure it starts
             */
            using IMicroserviceHost host = builder.CreateHost();
            host.Start();

            /**
             * Now let's pretend this service is running somewhere in a cluster
             * and is receiving events, let's fire some events at it
             */
            string[] names = { "Jack", "Jake", "Penny", "Robin", "Rick", "Vinny", "Spencer" };
            IEventPublisher publisher = new EventPublisher(context, loggerFactory);

            foreach (string name in names)
            {
                PolisToegevoegdEvent toegevoegdEvent = new PolisToegevoegdEvent
                {
                    Polis = new Polis { Klantnaam = name }
                };
                publisher.Publish(toegevoegdEvent);
            }

            /**
             * Now let's wait 1 second for all the events to arrive and be processed
             */
            Thread.Sleep(1000);

            /**
             * Now let's fire a command and retrieve a list of polissen
             */
            ICommandPublisher commandPublisher = new CommandPublisher(context, loggerFactory);
            HaalPolissenOpCommand command = new HaalPolissenOpCommand();
            HaalPolissenOpCommandResult commandResult = commandPublisher.PublishAsync<HaalPolissenOpCommandResult>(command).Result;

            /**
             * Now, print the result!
             */
            foreach (Polis polis in commandResult.Polissen)
            {
                Console.WriteLine($"Found polis for {polis.Klantnaam} with ID {polis.Id}");
            }

            /**
             * Lastly, let's see how the queue deals with exceptions on the other side
             */
            ExceptionCommand exceptionCommand = new ExceptionCommand();

            try
            {
                /**
                 * This will throw an exception in the receiver
                 */
                _ = commandPublisher.PublishAsync<ExceptionCommand>(exceptionCommand).Result;
            }
            catch (AggregateException expectedException)
            {
                DestinationQueueException destinationQueueException = expectedException.InnerExceptions.First() as DestinationQueueException;

                /**
                 * Now the expectedException.Innerexception will reveal all the info we need
                 */
                Console.WriteLine($"{destinationQueueException?.InnerException?.GetType().Name} {destinationQueueException?.InnerException?.Message}");
                Console.WriteLine($"Destination queue: {destinationQueueException?.DestinationQueueName}, " +
                                  $"Reply queue: {destinationQueueException?.ReplyQueueName}, " +
                                  $"Id: {destinationQueueException?.CorrelationId}");
            }
        }
    }
}
