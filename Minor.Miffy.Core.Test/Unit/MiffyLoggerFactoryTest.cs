using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Minor.Miffy.Test.Unit
{
    [TestClass]
    public class MiffyLoggerFactoryTest
    {
        [TestMethod]
        public void LoggerFactoryIsStandardNullFactory()
        {
            // Assert
            Assert.IsInstanceOfType(MiffyLoggerFactory.LoggerFactory, typeof(NullLoggerFactory));
        }

        [TestMethod]
        public void CreateInstanceReturnsDesiredInstance()
        {
            // Act
            var logger = MiffyLoggerFactory.CreateInstance<string>();

            // Assert
            Assert.IsInstanceOfType(logger, typeof(Logger<string>));
        }

        [TestMethod]
        public void SettingLoggerWorks()
        {
            // Arrange
            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();

            // Act
            MiffyLoggerFactory.LoggerFactory = loggerFactoryMock.Object;

            // Assert
            Assert.AreSame(MiffyLoggerFactory.LoggerFactory, loggerFactoryMock.Object);
        }

        [TestMethod]
        public void SettingLoggerFactoryTwiceThrowsException()
        {
            // Arrange
            Mock<ILoggerFactory> loggerFactoryMock = new Mock<ILoggerFactory>();

            MiffyLoggerFactory.LoggerFactory = loggerFactoryMock.Object;

            // Act
            void Act() => MiffyLoggerFactory.LoggerFactory = loggerFactoryMock.Object;

            // Assert
            InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(Act);
            Assert.AreEqual("Loggerfactory has already been set", exception.Message);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var prop = typeof(MiffyLoggerFactory).GetField("_loggerFactory", BindingFlags.Static | BindingFlags.NonPublic);
            prop?.SetValue(null, new NullLoggerFactory());
        }
    }
}
