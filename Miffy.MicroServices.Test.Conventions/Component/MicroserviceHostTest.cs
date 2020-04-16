using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;
using Miffy.MicroServices.Host;
using Miffy.MicroServices.Test.Conventions.Component.Commands;
using Miffy.MicroServices.Test.Conventions.Component.Event;
using Miffy.MicroServices.Test.Conventions.Component.EventListeners;
using Miffy.MicroServices.Test.Conventions.Component.Models;
using Miffy.TestBus;

namespace Miffy.MicroServices.Test.Conventions.Component
{
    [TestClass]
    public class MicroserviceHostTest
    {
        private const int WaitTime = 1500;

        [TestCleanup]
        public void TestCleanup()
        {
            EventListenerDummy.HandlesResult = null;
            EventListenerDummy2.HandlesResult = null;
            EventListenerDummy3.HandlesResult = null;
        }

        [TestMethod]
        [DataRow("Dummy text")]
        [DataRow("Example text")]
        [DataRow("Does This Compute?")]
        public void UseConventionsRegistersProperReceivers(string messageText)
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithBusContext(testContext);

            // Act
            hostBuilder.UseConventions();

            hostBuilder.CreateHost().Start();

            // Assert
            var message = new DummyEvent("TestTopic") { DummyText = messageText };

            new EventPublisher(testContext).Publish(message);

            Thread.Sleep(WaitTime);

            Assert.AreEqual(message, EventListenerDummy.HandlesResult);
            Assert.AreEqual(message, EventListenerDummy2.HandlesResult);
        }

        [TestMethod]
        [DataRow("Dummy text")]
        [DataRow("Example text")]
        [DataRow("Does This Compute?")]
        public void UseConventionsRegistersProperTopics(string messageText)
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            hostBuilder.UseConventions();

            hostBuilder.CreateHost().Start();

            // Assert
            var message = new DummyEvent("IrrelevantTopic") { DummyText = messageText };

            new EventPublisher(testContext).Publish(message);

            Thread.Sleep(WaitTime);

            Assert.IsNull(EventListenerDummy.HandlesResult);
            Assert.IsNull(EventListenerDummy2.HandlesResult);
            Assert.AreEqual(message, EventListenerDummy3.HandlesResult);
        }

        [TestMethod]
        public void UseConventionsRegistersProperCommandListeners()
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder()
                .WithBusContext(testContext);

            // Act
            hostBuilder.UseConventions();

            hostBuilder.CreateHost().Start();

            // Assert
            var message = new GetAnimalsCommand();

            ICommandPublisher publisher = new CommandPublisher(testContext);

            Animal[] animals = publisher.PublishAsync<IEnumerable<Animal>>(message).Result.ToArray();
            Assert.AreEqual(2, animals.Length);
        }
    }
}
