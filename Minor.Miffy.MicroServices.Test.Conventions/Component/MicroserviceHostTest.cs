using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;

namespace Minor.Miffy.MicroServices.Test.Conventions.Component
{
    [TestClass]
    public class MicroserviceHostTest
    {
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
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            hostBuilder.UseConventions();
            
            hostBuilder.CreateHost().Start();

            // Assert
            var message = new DummyEvent("TestTopic") { DummyText = messageText };
            
            new EventPublisher(testContext).Publish(message);
            
            Thread.Sleep(500);
            
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
            
            Thread.Sleep(500);
            
            Assert.IsNull(EventListenerDummy.HandlesResult);
            Assert.IsNull(EventListenerDummy2.HandlesResult);
            Assert.AreEqual(message, EventListenerDummy3.HandlesResult);
        }

        [TestMethod]
        [DataRow("Dummy text")]
        [DataRow("Example text")]
        [DataRow("Does This Compute?")]
        public void UseConventionsRegistersConcurrentQueuesProperly(string messageText)
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            hostBuilder.UseConventions();
            
            hostBuilder.CreateHost().Start();

            // Assert
            var message = new DummyEvent("ConcurrentTopic") { DummyText = messageText };
            
            new EventPublisher(testContext).Publish(message);
            
            Thread.Sleep(500);
            
            Assert.AreEqual(message, ConcurrentEventListenerDummy.HandlesResult);
            Assert.AreEqual(message, ConcurrentEventListenerDummy2.HandlesResult);
        }
    }
}