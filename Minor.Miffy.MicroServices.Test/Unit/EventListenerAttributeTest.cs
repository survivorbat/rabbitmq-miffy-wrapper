using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Minor.Miffy.MicroServices.Test.Unit
{
    [TestClass]
    public class EventListenerAttributeTest
    {
        [TestMethod]
        [DataRow("TestQueue")]
        [DataRow("queue.test")]
        public void QueueNameIsProperlySet(string queueName)
        {
            // Act
            EventListenerAttribute attribute = new EventListenerAttribute(queueName);
            
            // Assert
            Assert.AreEqual(queueName, attribute.QueueName);
        }
    }
}