using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;
using Moq;

namespace Minor.Miffy.Test.Unit.TestBus
{
    [TestClass]
    public class TestCommandSenderTest
    {
        private const int WaitTime = 1500;

        [TestMethod]
        public void SendCommandCreatesReplyQueue()
        {
            // Arrange
            var context = new Mock<TestBusContext>();
            var dictionary = new Dictionary<string, TestBusQueueWrapper<CommandMessage>>();

            context.SetupGet(e => e.CommandQueues)
                .Returns(dictionary);
            
            var sender = new TestCommandSender(context.Object);
            var request = new CommandMessage
            {
                DestinationQueue = "destination.queue",
                CorrelationId = Guid.Empty
            };
            
            // Act
            sender.SendCommandAsync(request);

            Thread.Sleep(WaitTime);
            
            // Assert
            Assert.AreEqual(1, dictionary.Keys.Count);
        }
        
        [TestMethod]
        [DataRow("test.queue", "Hello World")]
        [DataRow("dest.queue", "Beautiful world")]
        [DataRow("random.queue", "Machines everywhere")]
        [DataRow("queue.to.send.message.to", "Random Message")]
        public void SendCommandAddsValueToDestinationQueue(string queueName, string message)
        {
            // Arrange
            var context = new Mock<TestBusContext>();
            var dictionary = new Dictionary<string, TestBusQueueWrapper<CommandMessage>>();

            context.SetupGet(e => e.CommandQueues)
                .Returns(dictionary);
            
            dictionary[queueName] = new TestBusQueueWrapper<CommandMessage>();
            
            var sender = new TestCommandSender(context.Object);
            var request = new CommandMessage
            {
                DestinationQueue = queueName,
                ReplyQueue = "reply.queue",
                CorrelationId = Guid.Empty,
                Body = Encoding.Unicode.GetBytes(message)
            };
            
            // Act
            sender.SendCommandAsync(request);

            Thread.Sleep(WaitTime);
            
            // Assert
            Assert.IsTrue(dictionary.ContainsKey(queueName));
            dictionary[queueName].Queue.TryDequeue(out var result);
            Assert.AreEqual(message, Encoding.Unicode.GetString(result.Body));
        }
    }
}