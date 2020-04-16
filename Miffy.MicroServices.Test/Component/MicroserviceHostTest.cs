using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;
using Miffy.MicroServices.Host;
using Miffy.MicroServices.Host.HostEventArgs;
using Miffy.MicroServices.Test.Component.EventListeners;
using Miffy.TestBus;
using Newtonsoft.Json;

namespace Miffy.MicroServices.Test.Component
{
    [TestClass]
    public class MicroserviceHostTest
    {
        private const int WaitTime = 1000;

        [TestMethod]
        public void AddingListenerRegistersProperReceiver()
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            hostBuilder.AddEventListener<EventListenerDummy>();
            hostBuilder.WithQueueName("test.queue");

            hostBuilder.CreateHost().Start();

            // Assert
            var message = new DummyEvent("TestTopic");
            new EventPublisher(testContext).Publish(message);

            Thread.Sleep(WaitTime);

            Assert.AreEqual(message, EventListenerDummy.HandlesResult);
        }

        [TestMethod]
        public void EventMessageReceivedIsFiredOnIncomingMessageWithProperValues()
        {
            // Arrange
            const string queueName = "test.queue";
            const string topicExpression = "TestTopic";

            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithBusContext(testContext)
                .AddEventListener<EventListenerDummy>()
                .WithQueueName(queueName);

            using var host = hostBuilder.CreateHost();

            host.Start();

            EventMessage receivedEventMessage = null;
            EventMessageReceivedEventArgs receivedEventArgs = null;

            host.EventMessageReceived += (eventMessage, args) =>
            {
                receivedEventMessage = eventMessage;
                receivedEventArgs = args;
            };

            // Act
            var message = new DummyEvent(topicExpression);
            new EventPublisher(testContext).Publish(message);

            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreEqual(message.Type, receivedEventMessage.EventType);
            Assert.AreEqual(message.Id, receivedEventMessage.CorrelationId);
            Assert.AreEqual(message.Timestamp, receivedEventMessage.Timestamp);

            Assert.AreEqual(queueName, receivedEventArgs.QueueName);
            Assert.AreEqual(1, receivedEventArgs.TopicExpressions.Count());
            Assert.AreEqual(topicExpression, receivedEventArgs.TopicExpressions.Single());
        }

        [TestMethod]
        public void EventMessageHandledIsFiredOnIncomingMessageWithProperValues()
        {
            // Arrange
            const string queueName = "test.queue";
            const string topicExpression = "TestTopic";

            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithBusContext(testContext)
                .AddEventListener<EventListenerDummy>()
                .WithQueueName(queueName);

            using var host = hostBuilder.CreateHost();

            host.Start();

            EventMessage receivedEventMessage = null;
            EventMessageHandledEventArgs receivedEventArgs = null;

            host.EventMessageHandled += (eventMessage, args) =>
            {
                receivedEventMessage = eventMessage;
                receivedEventArgs = args;
            };

            // Act
            var message = new DummyEvent(topicExpression);
            new EventPublisher(testContext).Publish(message);

            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreEqual(message.Type, receivedEventMessage.EventType);
            Assert.AreEqual(message.Id, receivedEventMessage.CorrelationId);
            Assert.AreEqual(message.Timestamp, receivedEventMessage.Timestamp);

            Assert.AreEqual(queueName, receivedEventArgs.QueueName);
            Assert.AreEqual(1, receivedEventArgs.TopicExpressions.Count());
            Assert.AreEqual(topicExpression, receivedEventArgs.TopicExpressions.Single());
        }

        [TestMethod]
        public void AddingListenerOnlyAddsRelevantMethod()
        {
            // Arrange
            var testContext = new TestBusContext();
            var builder = new MicroserviceHostBuilder().WithBusContext(testContext)
                .WithQueueName("test.queue")
                .AddEventListener<MethodEventListener>();

            // Act
            using var host = builder.CreateHost();
            var result = host.Listeners.ToList();

            // Assert
            Assert.AreEqual(1, result.Count);

            var firstItem = result.FirstOrDefault();
            Assert.AreEqual("testPattern", firstItem?.TopicExpressions.FirstOrDefault());
        }

        [TestMethod]
        public void AddingListenerRegistersProperCommandReceiver()
        {
            // Arrange
            TestBusContext testContext = new TestBusContext();
            MicroserviceHostBuilder hostBuilder = new MicroserviceHostBuilder()
                .WithQueueName("test.queue")
                .WithBusContext(testContext);

            // Act
            hostBuilder.AddEventListener<CommandListenerDummy>();

            List<MicroserviceCommandListener> result = hostBuilder.CreateHost().CommandListeners.ToList();

            // Assert
            Assert.AreEqual(1, result.Count);

            var firstItem = result.FirstOrDefault();
            Assert.AreEqual("command.queue", firstItem?.Queue);
        }

        [TestMethod]
        public void ListenerWithWrongParameterThrowsException()
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            void Act() => hostBuilder.AddEventListener<WrongParameterEventListener>();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Method Handle does not have a proper " +
                            "eventlistener signature in type WrongParameterEventListener", exception.Message);

        }

        [TestMethod]
        public void ListenerWithWrongParameterCountThrowsException()
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            void Act() => hostBuilder.AddEventListener<WrongParameterAmountEventListener>();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Method Handle does not have a proper " +
                            "eventlistener signature in type WrongParameterAmountEventListener", exception.Message);
        }

        [TestMethod]
        public void EventListenerWithReturnTypeThrowsException()
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            void Act() => hostBuilder.AddEventListener<EventWithReturnTypeEventListener>();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Method Handle does not have a proper " +
                            "eventlistener signature in type EventWithReturnTypeEventListener", exception.Message);
        }

        [TestMethod]
        public void EventListenerWithReturnsInvalidOperationException()
        {
            //Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            using var host = hostBuilder
                .WithQueueName("test.queue")
                .AddEventListener<MissingDependencyEventListener>()
                .CreateHost();

            var eventListener = host.Listeners.First();

            //Act
            void Act() => eventListener.Callback.Invoke(new EventMessage());

            //Assert
            var exception = Assert.ThrowsException<InvalidOperationException>(Act);
            bool containsErrorMessage = exception.Message.Contains(
                "Unable to resolve service for type 'Miffy.MicroServices.Test.Component.EventListeners.MethodEventListener");
            Assert.IsTrue(containsErrorMessage);
        }

        [TestMethod]
        [DataRow("TestMessage")]
        [DataRow("Very Important Data")]
        [DataRow("{ secret }")]
        public void StringEventListenerReceivesProperString(string text)
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            using var host = hostBuilder
                .WithQueueName("test.queue")
                .AddEventListener<StringEventListenerDummy>()
                .CreateHost();

            host.Start();

            //Act
            var message = new DummyEvent("test.topic") { DummyText = text };
            new EventPublisher(testContext).Publish(message);

            // Assert
            Thread.Sleep(WaitTime);

            var expectedResult = JsonConvert.SerializeObject(message);

            Assert.AreEqual(expectedResult, StringEventListenerDummy.ReceivedData);
        }

        [TestMethod]
        [DataRow("TestMessage")]
        [DataRow("Very Important Data")]
        [DataRow("{ secret }")]
        public void CommandListenerWithNullReturnsNullProperly(string text)
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            using var host = hostBuilder
                .WithQueueName("test.queue")
                .AddEventListener<NullCommandListener>()
                .CreateHost();

            host.Start();

            var publisher = new CommandPublisher(testContext);
            var message = new DummyCommand {Text = text};

            //Act
            var result = publisher.PublishAsync<DummyCommand>(message);

            // Assert
            Assert.IsNull(result.Result);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            StringEventListenerDummy.ReceivedData = "";
        }
    }
}
