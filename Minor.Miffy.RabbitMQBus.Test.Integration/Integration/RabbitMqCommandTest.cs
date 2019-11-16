using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Minor.Miffy.RabbitMQBus.Test.Integration.Integration
{
    [TestClass]
    public class RabbitMqCommandReceiverTest
    {
        [TestMethod]
        [DataRow("test,queue", "Jan", "Hallo Jan")]
        [DataRow("add.hello", "Bart", "Hallo Bart")]
        [DataRow("random.queue.name", "Piet", "Hallo Piet")]
        [DataRow("testQueue", "Vind", "Hallo Vind")]
        public void CommandIsReceivedByReceiverWhenSent(string destQueue, string message, string expectedMessage)
        {
            // arrange
            const string prependMessage = "Hallo ";
            byte[] prependBytes = Encoding.Unicode.GetBytes(prependMessage);
            
            var context = new RabbitMqContextBuilder()
                .WithConnectionString("amqp://guest:guest@localhost")
                .WithExchange("HelloExchange")
                .CreateContext();

            var receiver = context.CreateCommandReceiver(destQueue);
            var sender = context.CreateCommandSender();

            receiver.DeclareCommandQueue();

            var command = new CommandMessage
            {
                DestinationQueue = destQueue,
                Timestamp = 294859,
                CorrelationId = Guid.NewGuid(),
                Body = Encoding.Unicode.GetBytes(message)
            };
            
            // Act
            receiver.StartReceivingCommands(commandMessage =>
                new CommandMessage
                {
                    Body = prependBytes.Concat(commandMessage.Body).ToArray()
                });

            var result = sender.SendCommandAsync(command);

            // Assert
            var stringResult = Encoding.Unicode.GetString(result.Result.Body);
            Assert.AreEqual(expectedMessage, stringResult);
        }
    }
}