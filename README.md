# RabbitMQ Miffy Wrapper

![GitHub](https://img.shields.io/github/license/survivorbat/rabbitmq-miffy-wrapper)

This is a wrapper library for the RabbitMQ Client in dotnetcore.
These packages allow you to easily set up event listeners and command listeners using RabbitMQ.

**MaartenH.Minor.Miffy.Abstractions**:  
Contains all the interfaces and base classes of the framework.
This package also contains a testbus for in-memory queueing.

![Nuget](https://img.shields.io/nuget/v/MaartenH.Minor.Miffy.Abstractions)
![Nuget](https://img.shields.io/nuget/dt/MaartenH.Minor.Miffy.Abstractions)

**MaartenH.Minor.Miffy.MicroServices**:  
The package containing the classes used to set up a microservice host.

![Nuget](https://img.shields.io/nuget/v/MaartenH.Minor.Miffy.Microservices)
![Nuget](https://img.shields.io/nuget/dt/MaartenH.Minor.Miffy.MicroServices)

**MaartenH.Minor.Miffy.RabbitMQBus**  
Implementation classes to use RabbitMQ with the framework

![Nuget](https://img.shields.io/nuget/v/MaartenH.Minor.Miffy.RabbitMQBus)
![Nuget](https://img.shields.io/nuget/dt/MaartenH.Minor.Miffy.RabbitMQBus)

These packages can be found on nuget.org and in my private azure cloud.

## Example configuration

To allow you for a quick start, here are a few examples on how to use this library.

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
This tutorial will only include the former option. First you'll need a DomainEvent class, like so:

```c#
public class ExampleEvent : DomainEvent 
{
    public ExampleEvent() : base("ExampleTopic") {}
    public string ExampleData { get; set; }
}
```

Then, you're going to need a callback function that handles such an event.

```c#
[EventListener("UniqueQueueName")]
public class ExampleEventListener 
{
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
                .WithBusContext(context)
                .UseConventions();
```

Just make sure an event listener has a EventListener attribute with an unique queuename and one method with a topic
and event parameter type.

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

var publisher = new EventPublisher();
publisher.Publish(exampleEvent);
```

Publishers can be injected using the IEventPublisher interface and accept any event that inherits from DomainEvent.

### Commands

Sending commands over the bus is also possible, first create a bus context from the **Events** section.

#### Listening for commands

```c#
public class ExampleCommand : DomainCommand 
{
    public ExampleCommand() : base("command.queue.somewhere") {}
    public string ExampleData { get; set; }
}
```

```c#
// Note: The name of this queue corresponds to the DestinationQueue of the ExampleCommand
[CommandListener("command.queue.somewhere")]
public class ExampleEventListener 
{
    public ExampleCommand Handles(ExampleCommand command) 
    {
        command.ExampleData = "Hello World!";
        DoSomethingwithCommand(command);
        return command;
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

var publisher = new CommandPublisher(context)
var result = publisher.PublishAsync<ExampleCommand>(exampleCommand);

Assert.AreEqual("Hello world!", result.Result.ExampleData);
```

And that's about it! Have fun rabbiting :)

## Notes
- A commandlistener or eventlistener must have only one handle command with a unique queue name
