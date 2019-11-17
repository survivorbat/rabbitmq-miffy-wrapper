using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Minor.Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqLoggerFactoryTest
    {
        [TestMethod]
        public void LoggerFactoryIsStandardNullFactory()
        {
            // Assert
            Assert.IsInstanceOfType(RabbitMqLoggerFactory.LoggerFactory, typeof(NullLoggerFactory));
        }

        [TestMethod]
        public void CreateInstanceReturnsDesiredInstance()
        {
            // Act
            var logger = RabbitMqLoggerFactory.CreateInstance<string>();
            
            // Assert
            Assert.IsInstanceOfType(logger, typeof(Logger<string>));
        }
    }
}