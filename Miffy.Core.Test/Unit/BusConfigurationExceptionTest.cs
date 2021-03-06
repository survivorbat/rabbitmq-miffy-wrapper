using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Miffy.Test.Unit
{
    [TestClass]
    public class BusConfigurationExceptionTest
    {

        [TestMethod]
        public void BusConfigurationExceptionCanBeInstantiated()
        {
            // Act
            _ = new BusConfigurationException();

            // Assert
            // No exception \o/
        }

        [TestMethod]
        [DataRow("Something went wrong!")]
        [DataRow("Not again! :/")]
        public void BusConfigurationExceptionCanBeInstantiatedWithMessage(string message)
        {
            // Act
            var exception = new BusConfigurationException(message);

            // Assert
            Assert.AreEqual(message, exception.Message);
        }

        [TestMethod]
        [DataRow("Outer", "Inner")]
        [DataRow("Hello", "World")]
        public void BusConfigurationExceptionCanBeInstantiatedWithInnerException(string message, string innerMessage)
        {
            // Arrange
            var innerException = new BusConfigurationException(innerMessage);

            // Act
            var exception = new BusConfigurationException(message, innerException);

            // Assert
            Assert.AreEqual(exception.Message, message);
            Assert.AreEqual(innerException, exception.InnerException);
        }
    }
}
