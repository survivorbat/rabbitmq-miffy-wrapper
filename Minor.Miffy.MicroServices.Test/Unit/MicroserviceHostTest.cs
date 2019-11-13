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
            var context = contextMock.Object;
            
            // Act
            var host = new MicroserviceHost(context);
            
            // Assert
            Assert.AreSame(context, host.Context);
        }

        [TestMethod]
        public void DisposeCallsDisposeOnContext()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var context = contextMock.Object;
            var host = new MicroserviceHost(context);

            // Act
            host.Dispose();
            
            // Assert
            contextMock.Verify(e => e.Dispose(), Times.Once);
        }
    }
}