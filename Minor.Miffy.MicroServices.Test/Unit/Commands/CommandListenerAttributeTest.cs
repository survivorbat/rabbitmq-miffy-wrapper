using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Unit.Commands
{
    [TestClass]
    public class CommandListenerAttributeTest
    {
        [TestMethod]
        [DataRow("TestQueue")]
        [DataRow("queue.test")]
        public void QueueNameIsProperlySet(string queueName)
        {
            // Act
            CommandListenerAttribute attribute = new CommandListenerAttribute(queueName);
            
            // Assert
            Assert.AreEqual(queueName, attribute.QueueName);
        }
    }
}