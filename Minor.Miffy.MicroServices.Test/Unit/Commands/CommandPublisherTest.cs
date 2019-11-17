using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Commands;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Test.Unit.Commands
{
    [TestClass]
    public class CommandPublisherTest
    {
        [TestMethod]
        [DataRow("Begin.School", "Test data")]
        [DataRow("MVM.Blackjack.AddUser", "Nijntje, lief klein konijntje")]
        [DataRow("MVM.Blackjack.Begin", "Nieuwe speler toegevoegd")]
        [DataRow("EDA.Start.Machine", "The machine has started!")]
        public void CreateCommandSenderCallsPublishOnSender(string destQueue, string body)
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var senderMock = new Mock<ICommandSender>();

            contextMock.Setup(e => e.CreateCommandSender())
                .Returns(senderMock.Object);
            
            CommandMessage message = new CommandMessage();

            senderMock.Setup(e => e.SendCommandAsync(It.IsAny<CommandMessage>()))
                .Callback<CommandMessage>(e => message = e);

            var sender = new CommandPublisher(contextMock.Object);

            var command = new TestCommand(destQueue);
            
            var jsonBody = JsonConvert.SerializeObject(command);
            
            // Act
            sender.PublishAsync<TestCommand>(command);
            
            // Assert
            Assert.AreEqual(destQueue, message.DestinationQueue);
            Assert.AreEqual(command.Id, message.CorrelationId);
            Assert.AreEqual(command.Timestamp, message.Timestamp);
            Assert.AreEqual(jsonBody, Encoding.Unicode.GetString(message.Body));
        }
    }
}