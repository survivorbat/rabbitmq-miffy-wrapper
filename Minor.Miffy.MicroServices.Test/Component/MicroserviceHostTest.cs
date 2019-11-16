using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.Microservices.Test.Integration.Integration.EventListeners;
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

        [TestMethod]
        public void AddingListenerOnlyAddsRelevantMethods()
        {
            // Arrange
            var testContext = new TestBusContext();
            var builder = new MicroserviceHostBuilder().WithBusContext(testContext)
                .AddEventListener<MethodEventListener>();

            // Act
            var result = builder.CreateHost().Listeners.ToList();
            
            // Assert
            Assert.AreEqual(1, result.Count);

            var firstItem = result.FirstOrDefault();
            Assert.AreEqual("PersonApp.Cats.Test", firstItem?.Queue);
            Assert.AreEqual("testPattern", firstItem?.TopicExpressions.FirstOrDefault());
        }
    }
}