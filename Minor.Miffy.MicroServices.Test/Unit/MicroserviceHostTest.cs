using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Test.Unit
{
    [TestClass]
    public class MicroserviceHostTest
    {
        [TestMethod]
        public void ContextIsProperlySet()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var loggerFactory = new Mock<ILogger<MicroserviceHost>>();
            var context = contextMock.Object;
            
            // Act
            var host = new MicroserviceHost(context, null, loggerFactory.Object);
            
            // Assert
            Assert.AreSame(context, host.Context);
        }

        [TestMethod]
        public void DisposeCallsDisposeOnContext()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var loggerFactory = new Mock<ILogger<MicroserviceHost>>();
            var context = contextMock.Object;
            var host = new MicroserviceHost(context, null, loggerFactory.Object);

            // Act
            host.Dispose();
            
            // Assert
            contextMock.Verify(e => e.Dispose(), Times.Once);
        }
    }
}