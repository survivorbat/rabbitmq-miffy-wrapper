using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;

namespace Minor.Miffy.MicroServices.Test.Component
{
    [TestClass]
    public class MicroserviceHostTest
    {
        [TestMethod]
        public void AddingListenerRegistersProperReceiver()
        {
            // Arrange
            var testContext = new TestBusContext();
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            hostBuilder.AddEventListener<EventListenerDummy>();
            
            hostBuilder.CreateHost().Start();

            // Assert
            var message = new DummyEvent("TestTopic");
            new EventPublisher(testContext).Publish(message);
            
            Thread.Sleep(500);
            
            Assert.AreEqual(message, EventListenerDummy.HandlesResult);
        }
    }
}