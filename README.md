# RabbitMQ Miffy Wrapper

![GitHub](https://img.shields.io/github/license/survivorbat/rabbitmq-miffy-wrapper)

This is a wrapper library for the RabbitMQ Client in dotnetcore.
These packages allow you to easily set up event listeners and command listeners using RabbitMQ.

<style>.bmc-button img{width: 35px !important;margin-bottom: 1px !important;box-shadow: none !important;border: none !important;vertical-align: middle !important;}.bmc-button{padding: 7px 10px 7px 10px !important;line-height: 35px !important;height:51px !important;min-width:217px !important;text-decoration: none !important;display:inline-flex !important;color:#ffffff !important;background-color:#5F7FFF !important;border-radius: 5px !important;border: 1px solid transparent !important;padding: 7px 10px 7px 10px !important;font-size: 22px !important;letter-spacing: 0.6px !important;box-shadow: 0px 1px 2px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 1px 2px 2px rgba(190, 190, 190, 0.5) !important;margin: 0 auto !important;font-family:'Cookie', cursive !important;-webkit-box-sizing: border-box !important;box-sizing: border-box !important;-o-transition: 0.3s all linear !important;-webkit-transition: 0.3s all linear !important;-moz-transition: 0.3s all linear !important;-ms-transition: 0.3s all linear !important;transition: 0.3s all linear !important;}.bmc-button:hover, .bmc-button:active, .bmc-button:focus {-webkit-box-shadow: 0px 1px 2px 2px rgba(190, 190, 190, 0.5) !important;text-decoration: none !important;box-shadow: 0px 1px 2px 2px rgba(190, 190, 190, 0.5) !important;opacity: 0.85 !important;color:#ffffff !important;}</style><link href="https://fonts.googleapis.com/css?family=Cookie" rel="stylesheet"><a class="bmc-button" target="_blank" href="https://www.buymeacoffee.com/MaartenH"><img src="https://cdn.buymeacoffee.com/buttons/bmc-new-btn-logo.svg" alt="Buy me a coffee :)"><span style="margin-left:15px;font-size:28px !important;">Buy me a coffee :)</span></a>

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

## Exceptions

### DestinationQueueException

This exception indicates that an exception occured at the CommandListener on
the receiving end and that the CommandPublisher is unable to complete its command.

### BusConfigurationException

This exception indicates that something is wrong with the configuration of
your listener of sender.

### MessageTimeoutException

This exception indicates that no response was received within the (currently hard-coded) limit
within a CommandPublisher.

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
public class ExampleEventListener 
{
    [EventListener("UniqueQueueName")]
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

using var builder = new MicroserviceHostBuilder()
                .WithBusContext(context)
                .AddEventListener<ExampleEventListener>();

using var host = builder.CreateHost();
host.Start();

// More code
```

And voila! Incoming events matching the topic will now be handled by the Handles method in the ExampleEventListener.

You can also allow reflection to take care of registering listeners, for example:
```c#
using var builder = new MicroserviceHostBuilder()
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

var publisher = new EventPublisher(context);
publisher.Publish(exampleEvent);
```

Publishers can be injected using the IEventPublisher interface and accept any event that inherits from DomainEvent.

#### Listening for raw json data

In case you don't want to listen to specific events or want to
serialize your own data. You can simply create an event listener with an input
type of _string_ like so:

```c#
public class JsonEventListener
{
    [EventListener("UniqueQueueName")]
    [Topic("ExampleTopic")]
    public void Handles(string rawJson) 
    {
        DoSomethingWithJson(rawJsoon);
    }
}
```


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

using var builder = new MicroserviceHostBuilder()
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

ICommandPublisher publisher = new CommandPublisher(context)
ExampleCommandResult result = publisher.PublishAsync<ExampleCommandResult>(exampleCommand);

Assert.AreEqual("Hello world!", result.ExampleData);
```

And that's about it! Have fun rabbiting :)

## Notes
- Queue names **MUST** be unique
- Exceptions thrown in Command callbacks **MUST** implement Serializable or have a _[Serializable]_ attribute
- Events, exceptions and commands need to have the same classname in all involved services in order to be properly (de)serialized
