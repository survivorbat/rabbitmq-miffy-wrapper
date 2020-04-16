using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Miffy.Test.Unit
{
    [TestClass]
    public class DestinationQueueExceptionTest
    {
        [TestMethod]
        [DataRow("testMessage")]
        [DataRow("test.message")]
        public void MessageIsProperlySet(string message)
        {
            // Act
            DestinationQueueException exception = new DestinationQueueException(message);
            
            // Assert
            Assert.AreEqual(message, exception.Message);
        }

        [TestMethod]
        [DataRow("hello", "its", "me", "9A19103F-16F7-4668-BE54-9A1E7A4F7557")]
        [DataRow("message", "test", "testTest", "9B19103F-16F7-4668-BE54-9A1E7A4F7557")]
        public void PropertiesAreProperlySet(string message, string replyQueue, string destQueue, string guid)
        {
            // Arrange
            Guid guidObject = Guid.Parse(guid);
            
            // Act
            DestinationQueueException exception = new DestinationQueueException(message, new Exception(), replyQueue, destQueue, guidObject);
            
            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(replyQueue, exception.ReplyQueueName);
            Assert.AreEqual(destQueue, exception.DestinationQueueName);
            Assert.AreEqual(guidObject, exception.CorrelationId);
        }
    }
}