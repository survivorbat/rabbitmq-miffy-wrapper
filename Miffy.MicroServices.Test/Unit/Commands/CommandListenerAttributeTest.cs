using Microsoft.VisualStudio.TestTools.UnitTesting;
using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;

namespace Miffy.MicroServices.Test.Unit.Commands
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