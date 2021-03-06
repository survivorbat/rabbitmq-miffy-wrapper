using System;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

namespace Miffy.RabbitMQBus.Test.Integration.Integration
{
    [TestClass]
    public class RabbitMqEventTest
    {
        private const int WaitTime = 2000;

        [TestMethod]
        [DataRow("listen.queuee", "Jan")]
        [DataRow("add.hello.queue", "Bart")]
        [DataRow("random.queue.signature", "Piet")]
        [DataRow("testQueueEvent", "Vind")]
        [DataRow("hello.queue.event", "Truus")]
        [DataRow("test.queue.event", "Femke")]
        public void CallbackIsCalledOnReceivedEvent(string queue, string message)
        {
            // Arrange
            using var context = new RabbitMqContextBuilder()
                .WithConnectionString("amqp://guest:guest@localhost")
                .WithExchange("HelloExchange")
                .CreateContext();

            bool messageReceived = false;

            using var receiver = context.CreateMessageReceiver(queue, new []{"#"});
            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => messageReceived = true);

            var eventMessage = new EventMessage
            {
                Body = Encoding.Unicode.GetBytes(message),
                Timestamp = 10,
                CorrelationId = Guid.NewGuid(),
                Topic = "irrelevant",
                EventType = "TestEvent"
            };

            // Act
            var sender = context.CreateMessageSender();
            sender.SendMessage(eventMessage);

            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreEqual(true, messageReceived);
        }

        [TestMethod]
        [DataRow("topic.pattern", "topic.pattern", true)]
        [DataRow("topic.*.pattern", "topic.test.pattern", true)]
        [DataRow("topic.#.pattern", "topic.test.test.test.pattern", true)]
        [DataRow("*.foo", "bar.bar", false)]
        [DataRow("*.foo.#", "bar.foo.bar.bar", true)]
        [DataRow("#", "bar.foo.bar.bar.bez.gaz", true)]
        [DataRow("test.#", "test.bar.foo.bar.bar.bez.gaz", true)]
        [DataRow("test.#", "bar.foo.bar.bar.bez.gaz", false)]
        [DataRow("test.*", "bar.test", false)]
        [DataRow("test.*.bar", "bar.test.baz", false)]
        [DataRow("test.*.*.bar", "bar.test.baz.foo", false)]
        public void CallbackIsCalledOnReceivedEventWithSpecificTopic(string topicPattern, string topic, bool expected)
        {
            // Arrange
            using IBusContext<IConnection> context = new RabbitMqContextBuilder()
                .WithConnectionString("amqp://guest:guest@localhost")
                .WithExchange("HelloExchange")
                .CreateContext();

            bool messageReceived = false;

            using var receiver = context.CreateMessageReceiver("topic.queue.test", new []{topicPattern});
            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => messageReceived = true);

            var eventMessage = new EventMessage
            {
                Body = Encoding.Unicode.GetBytes("TestMessage"),
                Timestamp = 10,
                CorrelationId = Guid.NewGuid(),
                Topic = topic,
                EventType = "TestEvent"
            };

            // Act
            var sender = context.CreateMessageSender();
            sender.SendMessage(eventMessage);

            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreEqual(messageReceived, expected);
        }

        [TestMethod]
        [DataRow(103920)]
        [DataRow(6938)]
        [DataRow(639530)]
        public void PausePausesReceivingMessages(long timestamp)
        {
            // Arrange
            using IBusContext<IConnection> context = new RabbitMqContextBuilder()
                .WithConnectionString("amqp://guest:guest@localhost")
                .WithExchange("HelloExchange")
                .CreateContext();

            bool messageReceived = false;

            using var receiver = context.CreateMessageReceiver("topic.queue.test", new []{"test"});
            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => messageReceived = true);

            var eventMessage = new EventMessage
            {
                Body = Encoding.Unicode.GetBytes("TestMessage"),
                Timestamp = timestamp,
                CorrelationId = Guid.NewGuid(),
                Topic = "test",
                EventType = "TestEvent"
            };

            // Act
            receiver.Pause();

            var sender = context.CreateMessageSender();
            sender.SendMessage(eventMessage);

            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreEqual(false, messageReceived);
        }

        [TestMethod]
        [DataRow(103920)]
        [DataRow(6938)]
        [DataRow(639530)]
        public void ResumeResumesReceivingMessagesAfterItWasPaused(long timestamp)
        {
            // Arrange
            using IBusContext<IConnection> context = new RabbitMqContextBuilder()
                .WithConnectionString("amqp://guest:guest@localhost")
                .WithExchange("HelloExchange")
                .CreateContext();

            bool messageReceived = false;

            using var receiver = context.CreateMessageReceiver("topic.queue.test", new []{"test"});
            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => messageReceived = true);

            var eventMessage = new EventMessage
            {
                Body = Encoding.Unicode.GetBytes("TestMessage"),
                Timestamp = timestamp,
                CorrelationId = Guid.NewGuid(),
                Topic = "test",
                EventType = "TestEvent"
            };

            receiver.Pause();

            // Act
            var sender = context.CreateMessageSender();
            sender.SendMessage(eventMessage);

            receiver.Resume();

            // Assert
            Thread.Sleep(WaitTime);

            Assert.AreEqual(true, messageReceived);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            RabbitMqCleanUp.DeleteQueue("topic.queue.test", "amqp://guest:guest@localhost");
            RabbitMqCleanUp.DeleteExchange("TestExchange", "amqp://guest:guest@localhost");
        }
    }
}
