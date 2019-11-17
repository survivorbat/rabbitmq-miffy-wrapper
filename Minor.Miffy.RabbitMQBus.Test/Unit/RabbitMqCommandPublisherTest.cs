using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus.Test.Unit
{
    [TestClass]
    public class RabbitMqCommandPublisherTest
    {
        [TestMethod]
        public void SendCommandCreatesChannel()
        {
            // Arrange
            var connectionMock = new Mock<IConnection>();
            var contextMock = new Mock<IBusContext<IConnection>>();

            contextMock.SetupGet(e => e.Connection)
                .Returns(connectionMock.Object);
            
            var sender = new RabbitMqCommandSender(contextMock.Object);
            
            var command = new CommandMessage();
            
            // Act
            sender.SendCommandAsync(command);

            // Assert
            connectionMock.Verify(e => e.CreateModel(), Times.Once);
        }
    }
}