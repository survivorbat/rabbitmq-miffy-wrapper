using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Test.Unit
{
    [TestClass]
    public class MicroserviceHostTest
    {
        private Mock<ILoggerFactory> _loggerFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            var logger = new Mock<ILogger<MicroserviceHost>>();
            
            _loggerFactory = new Mock<ILoggerFactory>();
            _loggerFactory.Setup(e => e.CreateLogger<MicroserviceHost>())
                .Returns(logger.Object);
        }
        
        [TestMethod]
        public void ContextIsProperlySet()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var context = contextMock.Object;
            
            // Act
            var host = new MicroserviceHost(context, null, _loggerFactory.Object);
            
            // Assert
            Assert.AreSame(context, host.Context);
        }

        [TestMethod]
        public void DisposeCallsDisposeOnContext()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var context = contextMock.Object;
            var host = new MicroserviceHost(context, null, _loggerFactory.Object);

            // Act
            host.Dispose();
            
            // Assert
            contextMock.Verify(e => e.Dispose(), Times.Once);
        }
    }
}