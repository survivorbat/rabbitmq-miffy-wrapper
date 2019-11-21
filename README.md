# RabbitMQ Miffy Wrapper

This is a work-in-progress wrapper library for the RabbitMQ Client in dotnetcore.
These packages allow you to easily set up event listeners and command listeners using
RabbitMQ.

### Example configuration

First, you need to open a connection using an implementation of the
IBusContext<IConnection> class like so:


## Packages

**MaartenH.Minor.Miffy.Abstractions**:  
Contains all the interfaces and base classes of the framework.
This package also contains a testbus for in-memory queueing.

**MaartenH.Minor.Miffy.MicroServices**:  
The package containing the classes used to set up a microservice host.

**MaartenH.Minor.Miffy.RabbitMQ**  
Implementation classes to use RabbitMQ with the framework

## Notes
- A commandlistener or eventlistener must have only one handle command with a unique queue name
