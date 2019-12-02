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
            var hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

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
        
        [TestMethod]
        public void AddingListenerRegistersProperCommandReceiver()
        {
            // Arrange
            TestBusContext testContext = new TestBusContext();
            MicroserviceHostBuilder hostBuilder = new MicroserviceHostBuilder().WithBusContext(testContext);

            // Act
            hostBuilder.AddEventListener<CommandListenerDummy>();
            
            List<MicroserviceCommandListener> result = hostBuilder.CreateHost().CommandListeners.ToList();
            
            // Assert
            Assert.AreEqual(1, result.Count);
            
            var firstItem = result.FirstOrDefault();
            Assert.AreEqual("command.queue", firstItem?.Queue);
        }
    }
}