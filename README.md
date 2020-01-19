# RabbitMQ .NET Core Miffy Wrapper

![GitHub](https://img.shields.io/github/license/survivorbat/rabbitmq-miffy-wrapper)

*🎉 Version 2.0.0 has just been released! 🎉*

This is a wrapper library for the RabbitMQ Client in dotnetcore.
These packages allow you to easily set up event listeners and command listeners using RabbitMQ.

**MaartenH.Minor.Miffy.Abstractions**
Contains all the interfaces and base classes of the framework.
This package also contains a testbus for in-memory queueing.

![Nuget](https://img.shields.io/nuget/v/MaartenH.Minor.Miffy.Abstractions)
![Nuget](https://img.shields.io/nuget/dt/MaartenH.Minor.Miffy.Abstractions)

**MaartenH.Minor.Miffy.MicroServices**
The package containing the classes used to set up a microservice host.

![Nuget](https://img.shields.io/nuget/v/MaartenH.Minor.Miffy.Microservices)
![Nuget](https://img.shields.io/nuget/dt/MaartenH.Minor.Miffy.MicroServices)

**MaartenH.Minor.Miffy.RabbitMQBus**
Implementation classes to use RabbitMQ with the framework

![Nuget](https://img.shields.io/nuget/v/MaartenH.Minor.Miffy.RabbitMQBus)
![Nuget](https://img.shields.io/nuget/dt/MaartenH.Minor.Miffy.RabbitMQBus)

These packages can be found on nuget.org.

## Example configuration

To allow you a quick start, here are a few examples on how to use this library.

### Events

#### Bus Context
First, you need to open a connection using an implementation of the IBusContext<IConnection> class like so:
```c#
var contextBuilder = new RabbitMqContextBuilder()
                    .WithExchange("ExampleExchange")
                    .WithConnectionString("amqp://guest:guest@localhost");

using IBusContext<IConnection> context = contextBuilder.CreateContext();
```

Or, if you wish, an in-memory context:
```c#
var context = new TestBusContext();
```

#### Listening for events

To start listening for events you can either use a microservice host or implement your own listener.
This tutorial will only include the former option. You're going to need a DomainEvent class.

```c#
public class ExampleEvent : DomainEvent
{
    public ExampleEvent() : base("ExampleTopic") {}
    public string ExampleData { get; set; }
}
```

Then, you're going to need a callback function that handles such an event.

```c#
public class ExampleEventListener
{
    [EventListener]
    [Topic("ExampleTopic")]
    public void Handles(ExampleEvent exampleEvent)
    {
        DoSomethingWithData(exampleEvent.ExampleData);
    }
}
```

Now that we have that setup, we can register the event listener in our hostbuilder.

```c#
// Context builder code

var builder = new MicroserviceHostBuilder()
				.WithQueueName("MyService.Queue")
                .WithBusContext(context)
                .AddEventListener<ExampleEventListener>();

using var host = builder.CreateHost();
host.Start();

// More code
```

And voila! Incoming events matching the topic will now be handled by the Handles method in the ExampleEventListener.

You can also allow reflection to take care of registering listeners, for example:
```c#
var builder = new MicroserviceHostBuilder()
				.WithQueueName("MyService.Queue")
                .WithBusContext(context)
                .UseConventions();
```

Just make sure an event listener has a EventListener attribute with one or more Topic attributes.

#### Publishing events

Now that we have a listener listening for events, we need something to publish events with. Luckily we have that too.

```c#
// Context builder code

public class ExampleEvent : DomainEvent
{
    public ExampleEvent() : base("ExampleTopic") {}
    public string ExampleData { get; set; }
}

var exampleEvent = new ExampleEvent() { ExampleData = "Hello World" };

var publisher = new EventPublisher(context);
publisher.PublishAsync(exampleEvent);
```

Publishers can be injected using the IEventPublisher interface and accept any event that inherits from DomainEvent.

Please note that DomainEvents can also contain a Process Id since v1.5.0 to follow a certain process through a system.
This property can be set using an overloaded constructor like so:

```c#
public class ExampleEvent : DomainEvent
{
    public ExampleEvent(Guid processId) : base("Exampletopic", processId) {}
    public string ExampleData { get; set; }
}

Guid guid = /* Create a guid */;
ExampleEvent exampleEvent = new ExampleEvent(guid);
```

#### Listening for raw json data

In case you don't want to listen to specific events or want to
serialize your own data. You can simply create an event listener with an input
type of _string_ like so:

```c#
public class JsonEventListener
{
    [EventListener]
    [Topic("ExampleTopic")]
    public void Handles(string rawJson)
    {
        DoSomethingWithJson(rawJson);
    }
}
```

#### Publishing raw json data

In case you want to publish raw json over the bus, you can use the overloaded variant of the Publish method like so:

```c#
var publisher = new EventPublisher(context);
string body = "{\"hello\": \"World\"}";
publisher.PublishAsync(timestamp: 500000, topic: "TestTopic", correlationId: Guid.NewGuid(), eventType: "TestEvent", body: body);
```

**Use this method with caution**, since deserializing raw or faulty data might throw errors and cause havoc.

### Commands

Sending commands over the bus is also possible, first create a bus context from the **Events** section.

#### Listening for commands

```c#
class ExampleCommand : DomainCommand
{
    public ExampleCommand() : base("command.queue.somewhere") {}
    public string ExampleData { get; set; }
}

class ExampleCommandResult {
    public string ExampleData { get; set; }
}
```

```c#
// Note: The name of this queue corresponds to the DestinationQueue of the ExampleCommand
public class ExampleEventListener
{
    [CommandListener("command.queue.somewhere")]
    public ExampleCommandResult Handles(ExampleCommand command)
    {
        DoSomethingwithCommand(command);
        return new ExampleCommandResult { ExampleData = "Hello World" };
    }
}
```

Now that we have that setup, we can register the command listener in our hostbuilder, the same way we register an event listener.

```c#
// Context builder code

var builder = new MicroserviceHostBuilder()
                .WithBusContext(context)
                .AddEventListener<ExampleCommandListener>();

using var host = builder.CreateHost();
host.Start();

// More code
```

And voila, now it's listening for incoming ExampleCommands and sends back data to the reply queue.
Please note that the Start() command is non-blocking, so in case you wish to
keep it running you'll need something to occupy the main thread.

Since v1.6.0 it's also possible to .Pause() and .Resume() this host instance.

#### Sending commands

The last piece of the puzzle is publishing commands. This can be done like so:

```c#
// Context builder code

public class ExampleCommand : DomainCommand
{
    public ExampleCommand() : base("command.queue.somewhere") {}
    public string ExampleData { get; set; }
}

var exampleCommand = new ExampleCommand { ExampleData = "Hello?" };

ICommandPublisher publisher = new CommandPublisher(context)
ExampleCommandResult result = publisher.PublishAsync<ExampleCommandResult>(exampleCommand).Result;

Assert.AreEqual("Hello world!", result.ExampleData);
```

Please note that, just like events,  DomainCommands can also contain a Process Id since v1.5.0 to follow a certain process through a system.
This property can be set using an overloaded constructor like so:

```c#
public class ExampleCommand : DomainCommand
{
    public ExampleCommand(Guid processId) : base("command.queue.somewhere", processId) {}
    public string ExampleData { get; set; }
}

Guid guid = /* Create a guid */;
ExampleCommand exampleCommand = new ExampleCommand(guid);
```

And that's about it! Have fun rabbiting :)

## Invalid events

Any incoming events or commands that can not be properly deserialized or other cause issues will be logged as critical.
In case you encounter such an error, please consider changing your listener's parameter to _string_ as described in the
**Listening for raw json data** section of this README.

## Library events

This library contains a few C# events (not to be confused with RabbitMQ events) that you can subscribe to.
These events are:

### IMicroserviceHost
- EventMessageReceived
- EventMessageHandled
- HostStarted
- HostPaused
- HostResumed

## Notes
- We encourage you to utilize a loggerfactory for logging, since bugs can easily be found by reading the logs.
- Exceptions thrown in Command receivers **MUST** implement Serializable or have a _[Serializable]_ attribute
- Events, exceptions and commands need to have the same classname in all involved services in order to be properly (de)serialized

<a href="https://www.buymeacoffee.com/MaartenH" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-red.png" height="50" widt="216" alt="Buy Me A Coffee" ></a>
