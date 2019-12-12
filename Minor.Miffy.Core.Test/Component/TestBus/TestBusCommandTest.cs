using System;
using System.Diagnostics.Tracing;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;

namespace Minor.Miffy.Test.Component.TestBus
{
    [TestClass]
    public class TestBusCommandTest
    {
        private const int WaitTime = 1500;

        [TestMethod]
        [DataRow("test.queue", "World")]
        [DataRow("command.queue", "Test")]
        [DataRow("command.queue", "Hello")]
        public void SentMessageIsProperlyReceivedAndReturned(string destQueue,string expected)
        {
            // Arrange
            var context = new TestBusContext();
            var sender = context.CreateCommandSender();
            var receiver = context.CreateCommandReceiver(destQueue);
            receiver.DeclareCommandQueue();

            var command = new CommandMessage
            {
                DestinationQueue = destQueue,
                CorrelationId = Guid.NewGuid()
            };

            // Act
            receiver.StartReceivingCommands(e => new CommandMessage {Body = Encoding.Unicode.GetBytes(expected)});

            var result = sender.SendCommandAsync(command);

            // Assert
            Assert.AreEqual(expected, Encoding.Unicode.GetString(result.Result.Body));
        }

        [TestMethod]
        [DataRow("Hello", " World")]
        [DataRow("Start", " Stop")]
        [DataRow("Start", " Stop")]
        [DataRow("Android", " Apple")]
        [DataRow("rm", " -rf /")]
        [DataRow("Person", " of Interest")]
        public void SentMessageIsProperlySentAndReceivedWithManipulations(string startMessage, string appendMessage)
        {
            // Arrange
            var context = new TestBusContext();
            var sender = context.CreateCommandSender();
            var receiver = context.CreateCommandReceiver("dest.queue");
            receiver.DeclareCommandQueue();

            var command = new CommandMessage
            {
                DestinationQueue = "dest.queue",
                CorrelationId = Guid.NewGuid(),
                Body = Encoding.Unicode.GetBytes(startMessage)
            };

            // Act
            receiver.StartReceivingCommands(e =>
            {
                var textBody = Encoding.Unicode.GetString(e.Body);
                return new CommandMessage {Body = Encoding.Unicode.GetBytes(textBody + appendMessage)};
            });

            var result = sender.SendCommandAsync(command);

            // Assert
            Assert.AreEqual(startMessage + appendMessage, Encoding.Unicode.GetString(result.Result.Body));
        }

        [TestMethod]
        [DataRow("TestQueue")]
        [DataRow("MVM.Queue")]
        [DataRow("NewQueueWithItems")]
        public void PausingEventListenerPausesDeliveryInQueue(string queueName)
        {
            var context = new TestBusContext();
            var sender = context.CreateCommandSender();
            var receiver = context.CreateCommandReceiver(queueName);
            receiver.DeclareCommandQueue();

            var command = new CommandMessage
            {
                DestinationQueue = queueName,
                CorrelationId = Guid.NewGuid(),
                Body = Encoding.Unicode.GetBytes("Hello World")
            };

            bool messageReceived = false;

            receiver.StartReceivingCommands(e =>
            {
                messageReceived = true;
                return new CommandMessage();
            });

            // Act
            receiver.Pause();

            sender.SendCommandAsync(command);

            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreEqual(false, messageReceived);
        }

        [TestMethod]
        [DataRow("TestQueue")]
        [DataRow("MVM.Queue")]
        public void ResumingEventListenerReActivatesDeliveryInQueue(string queueName)
        {
            var context = new TestBusContext();
            var sender = context.CreateCommandSender();
            var receiver = context.CreateCommandReceiver(queueName);
            receiver.DeclareCommandQueue();

            var command = new CommandMessage
            {
                DestinationQueue = queueName,
                CorrelationId = Guid.NewGuid(),
                Body = Encoding.Unicode.GetBytes("Hello World")
            };

            bool messageReceived = false;

            receiver.StartReceivingCommands(e =>
            {
                messageReceived = true;
                return new CommandMessage();
            });

            receiver.Pause();

            // Act
            sender.SendCommandAsync(command);

            Thread.Sleep(WaitTime);

            receiver.Resume();

            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreEqual(true, messageReceived);
        }
    }
}
