using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

        [TestMethod]
        public void SettingLoggerWorks()
        {
            // Arrange
            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();

            // Act
            RabbitMqLoggerFactory.LoggerFactory = loggerFactoryMock.Object;

            // Assert
            Assert.AreSame(RabbitMqLoggerFactory.LoggerFactory, loggerFactoryMock.Object);
        }

        [TestMethod]
        public void SettingLoggerFactoryTwiceThrowsException()
        {
            // Arrange
            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();

            RabbitMqLoggerFactory.LoggerFactory = loggerFactoryMock.Object;

            // Act
            void Act() => RabbitMqLoggerFactory.LoggerFactory = loggerFactoryMock.Object;

            // Assert
            InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(Act);
            Assert.AreEqual("Loggerfactory has already been set", exception.Message);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var prop = typeof(RabbitMqLoggerFactory).GetField("_loggerFactory", BindingFlags.Static | BindingFlags.NonPublic);
            prop.SetValue(null, new NullLoggerFactory());
        }
    }
}
