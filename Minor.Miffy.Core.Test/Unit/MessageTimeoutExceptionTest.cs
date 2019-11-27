using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Minor.Miffy.Test.Unit
{
    [TestClass]
    public class MessageTimeoutExceptionTest
    {
        [TestMethod]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(600000)]
        public void TimeoutIsProperlySet(int timeOut)
        {
            // Act
            MessageTimeoutException exception = new MessageTimeoutException("message", timeOut);
            
            // Assert
            Assert.AreEqual(timeOut, exception.TimeOut);
        }

        [TestMethod]
        [DataRow("Hello")]
        [DataRow("World")]
        public void MessageIsProperlySet(string message)
        {
            // Act
            MessageTimeoutException exception = new MessageTimeoutException(message, 10);
            
            // Assert
            Assert.AreEqual(message, exception.Message);
        }
    }
}