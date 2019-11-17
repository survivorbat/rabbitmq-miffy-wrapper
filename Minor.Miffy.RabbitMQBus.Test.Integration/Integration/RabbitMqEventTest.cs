using System;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Minor.Miffy.RabbitMQBus.Test.Integration.Integration
{
    [TestClass]
    public class RabbitMqEventTest
    {
        [TestMethod]
        [DataRow("listen.queuee", "Jan")]
        [DataRow("add.hello.queue", "Bart")]
        [DataRow("random.queue.signature", "Piet")]
        [DataRow("testQueueEvent", "Vind")]
        [DataRow("hello.queue.event", "Truus")]
        [DataRow("test.queue.event", "Femke")]
        public void CallbackIsCalledOnReceivedEvent(string queue, string message)
        {
            // arrange
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
            
            var sender = context.CreateMessageSender();
            sender.SendMessage(eventMessage);
            
            Thread.Sleep(500);
            
            Assert.IsTrue(messageReceived);
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
            // arrange
            using var context = new RabbitMqContextBuilder()
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
            
            var sender = context.CreateMessageSender();
            sender.SendMessage(eventMessage);
            
            Thread.Sleep(500);

            Assert.AreEqual(messageReceived, expected);
        }
    }
}