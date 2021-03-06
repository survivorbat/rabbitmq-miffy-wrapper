# Changelog Miffy Framework

## 2.4.0

- Changed Minor.Miffy to just Miffy

## 2.3.1

- The **Topic** property on DomainEvents is now public

## 2.3.0

- Added EventMessageHandled event to host, fired whenever a message is handled

## 2.2.1

- Remove Context dispose from host

## 2.2.1

- Remove exceptions from testbus

## 2.2.0

- Remove Dispose method and IDisposable from host builder
- Remove loggerfactory dispose in host builder

## 2.1.0

- Add events to microservicehost to subscribe to:
- Added EventMessageReceived event to host, fired whenever a event message comes in
- Added HostStarted event to host, fired when you call Start()
- Added HostPaused event to host, fired when you call Pause()
- Added HostResumed event to host, fired when you call Resume()

## 2.0.1

- Add more log statements to microservice host

## 2.0.0(-alpha) 🎉🎉🎉

- Finally remove the unique-queue constraint and the fact that the framework created multiple queues
- Add .WithQueueName(string queueName) to hostbuilder
- Add default queuename to hostbuilder
- Remove queue name from [EventListener] attribute
- Move CommandListenerAttribute to Command namespace

## 1.10.0

- Remove the overwriting mechanism of RegisterDependencies and replace it with appending dependencies

## 1.9.0

- Add overloaded RegisterDependencies method with tests

## 1.8.2

- Fix two log statements in the CommandPublisher

## 1.8.1

- Update eventlistener exception message and add example service info

## 1.8.0

- Add extra error handlers to eventhandlers prevent silent errors on invalid json

## 1.7.0

- Add PublishAsync method to EventPublisher and IEventPublisher

## 1.6.1

- Add overloaded Publish command to ICommandPublisher interface

## 1.6.0

- Turn most protected fields into properties
- Add ability to pause command receivers
- Add Pause() and Resume() methods to microservice host
- Add interface for MicroserviceHost called IMicroserviceHost

## 1.5.0

- Add ProcessId property to DomainEvents and DomainCommands

## 1.4.1

- Add logging to pausing and resuming

## 1.4.0

- Add 'Pause' and 'Resume' methods to MessageReceivers

## 1.3.1

- Make a bunch of context methods virtual

## 1.3.0

- Change accessibility in host and hostbuilder to increase extensibility

## 1.2.0

- Add event publish method that allows user to compose a message

## 1.1.0

- Add attribute usage to attributes

## 1.0.1

- Loosen the accessibility of the fields in the host and the host builder

## 1.0.0

- Make sure only the name of the event type is transmitted and not the full name

## 0.10.0

- Remove exchange name from command handlers
- Add event type to DomainEvents to determine event types

## 0.9.1

- Optimize imports

## 0.9.0

- Rename several senders/receivers to make naming consistent
- Add comments notifying the user that a component is low-level
- Allow a custom command return type to be used in command listeners

## 0.8.4-0.8.5

- Add more tests to bring code coverage above 98%

## 0.8.3

- Add several tests

## 0.8.2

- Bring back single pipeline

## 0.8.1

- Ensure that loggers can only be set once

## 0.8.0

- Add better listener method evaluation
- Separate pipeline into 3 pieces
- Allow strings to be input types for event listeners
- Migrate changelog to individual packages ;-)

## 0.7.1

- Removed test dependency of microservices project.

## 0.7.0

- Command and Event listener attributes now belong on top of the method instead of the class
- Add a CHANGELOG.md file

## 0.6.4

- Make sure queues are not automatically removed and add test cleanup.

## 0.6.3

- Add exception message to critical log statement.
- Add README information concerning serializable exceptions.

## 0.6.2

- Added critical log statement to log unresolved dependencies.

## 0.6.0-0.6.1

- Replace CommandError's ExceptionMessage with a full-fledged error.

## <0.6.0

Changes were not properly recorded before this version.
