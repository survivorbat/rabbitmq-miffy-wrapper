using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Host;
using Minor.Miffy.MicroServices.Test.Component.EventListeners;
using Minor.Miffy.TestBus;

namespace Minor.Miffy.MicroServices.Test.Component
{
    [TestClass]
    public class MicroserviceHostTest
    {
        private const int WaitTime = 1500;

        [TestMethod]
        public void AddingListenerRegistersProperReceiver()
        {
            // Arrange
            var testContext = new TestBusContext();
            using var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            hostBuilder.AddEventListener<EventListenerDummy>();

            hostBuilder.CreateHost().Start();

            // Assert
            var message = new DummyEvent("TestTopic");
            new EventPublisher(testContext).Publish(message);

            Thread.Sleep(WaitTime);

            Assert.AreEqual(message, EventListenerDummy.HandlesResult);
        }

        [TestMethod]
        public void AddingListenerOnlyAddsRelevantMethod()
        {
            // Arrange
            var testContext = new TestBusContext();
            using var builder = new MicroserviceHostBuilder().WithBusContext(testContext)
                .AddEventListener<MethodEventListener>();

            // Act
            using var host = builder.CreateHost();
            var result = host.Listeners.ToList();

            // Assert
            Assert.AreEqual(1, result.Count);

            var firstItem = result.FirstOrDefault();
            Assert.AreEqual("PersonApp.Cats.Test", firstItem?.Queue);
            Assert.AreEqual("testPattern", firstItem?.TopicExpressions.FirstOrDefault());
        }

        [TestMethod]
        public void AddingListenerRegistersProperCommandReceiver()
        {
            // Arrange
            TestBusContext testContext = new TestBusContext();
            using MicroserviceHostBuilder hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

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
            using var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            void Act() => hostBuilder.AddEventListener<WrongParameterEventListener>();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Method Handle does not have a proper " +
                            "commandlistener signature in type WrongParameterEventListener", exception.Message);

        }

        [TestMethod]
        public void ListenerWithWrongReturnTypeThrowsException()
        {
            // Arrange
            var testContext = new TestBusContext();
            using var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            void Act() => hostBuilder.AddEventListener<WrongReturnEventListener>();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Method Handle does not have a proper " +
                            "commandlistener signature in type WrongReturnEventListener", exception.Message);
        }

        [TestMethod]
        public void ListenerWithWrongParameterCountThrowsException()
        {
            // Arrange
            var testContext = new TestBusContext();
            using var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            void Act() => hostBuilder.AddEventListener<WrongParameterAmountEventListener>();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Method Handle does not have a proper " +
                            "commandlistener signature in type WrongParameterAmountEventListener", exception.Message);
        }

        [TestMethod]
        public void EventListenerWithReturnTypeThrowsException()
        {
            // Arrange
            var testContext = new TestBusContext();
            using var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            void Act() => hostBuilder.AddEventListener<EventWithReturnTypeEventListener>();

            // Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(Act);
            Assert.AreEqual("Method Handle does not have a proper " +
                            "commandlistener signature in type EventWithReturnTypeEventListener", exception.Message);
        }
    }
}
