using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;

namespace Minor.Miffy.Test.Unit.TestBus
{
    [TestClass]
    public class TestBusQueueWrapperTest
    {
        [TestMethod]
        public void QueueIsInitialized()
        {
            // Act
            TestBusQueueWrapper<EventMessage> queue = new TestBusQueueWrapper<EventMessage>();

            // Assert
            Assert.IsNotNull(queue.Queue);
        }
        
        [TestMethod]
        public void ResetEventIsInitialized()
        {
            // Act
            TestBusQueueWrapper<EventMessage> queue = new TestBusQueueWrapper<EventMessage>();

            // Assert
            Assert.IsNotNull(queue.AutoResetEvent);
        }
        
        [TestMethod]
        public void ResetEventInitialStateIsFalse()
        {
            // Act
            TestBusQueueWrapper<EventMessage> queue = new TestBusQueueWrapper<EventMessage>();

            // Assert
            Assert.IsFalse(queue.AutoResetEvent.WaitOne(0));
        }
    }
}