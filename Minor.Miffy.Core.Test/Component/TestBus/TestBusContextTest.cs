using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;

namespace Minor.Miffy.Test.Component.TestBus
{
    [TestClass]
    public class TestBusContextTest
    {
        private const int WaitTime = 1500;

        [TestMethod]
        public void SendingMessageCallsCallback()
        {
            // Arrange
            var context = new TestBusContext();
            var sender = context.CreateMessageSender();
            var receiver = context.CreateMessageReceiver("testQueue", new []{ "TestTopic" });

            var message = new EventMessage {Topic = "TestTopic"};

            var callbackCalled = false;

            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => callbackCalled = true);

            // Act
            sender.SendMessage(message);
            Thread.Sleep(WaitTime);

            // Assert
            Assert.IsTrue(callbackCalled);
        }

        [TestMethod]
        [DataRow("test.topic")]
        [DataRow("topicTest")]
        [DataRow("MVM.BlackJack")]
        [DataRow("Aanmeldingen")]
        public void SendingMessageWithTopicsWorks(string topicName)
        {
            // Arrange
            var context = new TestBusContext();
            var sender = context.CreateMessageSender();
            var receiver = context.CreateMessageReceiver("testQueue", new []{ topicName });

            var message = new EventMessage {Topic = topicName};

            var callbackCalled = false;

            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => callbackCalled = true);

            // Act
            sender.SendMessage(message);
            Thread.Sleep(WaitTime);

            // Assert
            Assert.IsTrue(callbackCalled);
        }

        [TestMethod]
        [DataRow("test.topic,topic.test,tepac.test", "test.topic")]
        [DataRow("test.topic,topic.test,tepac.test", "topic.test")]
        [DataRow("foo,bar,bez", "foo")]
        [DataRow("foo,bar,bez", "bar")]
        [DataRow("foo,bar,bez", "bez")]
        [DataRow("#bez#,foo", "bez.foo")]
        [DataRow("b.*.t,t.*.c", "b.c.t")]
        public void ReceiverCanMessageWithMultipleTopics(string topics, string chosenTopic)
        {
            // Arrange
            var context = new TestBusContext();
            var sender = context.CreateMessageSender();
            var receiver = context.CreateMessageReceiver("testQueue",  topics.Split(',') );

            var message = new EventMessage {Topic = chosenTopic};

            var callbackCalled = false;

            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => callbackCalled = true);

            // Act
            sender.SendMessage(message);
            Thread.Sleep(WaitTime);

            // Assert
            Assert.IsTrue(callbackCalled);
        }

        [TestMethod]
        [DataRow("MVM.*.test", "MVM.foo.test", true)]
        [DataRow("MVM.*.#", "MVM.bar.foo.zed.lorem", true)]
        [DataRow("MVM.#.test", "MVM.foo.bar.test", true)]
        [DataRow("Blackjack.*", "Blackjack.foo", true)]
        [DataRow("Blackjack.Whitejack", "*.Blackjack", false)]
        [DataRow("Test.Test.*", "Test.Test.Nee.Nee", false)]
        [DataRow("#.Foo", "Bar.Bar.Foo.Bar", false)]
        [DataRow("#.Foo", "Bar.Bar.Bar", false)]
        [DataRow("#", "Bar.Foo.Dez.Bez", true)]
        [DataRow("*.Foo", "Foo.Foo.Foo", false)]
        [DataRow("*.Foo", "Foo.Foo", true)]
        public void SendingMessageWithTopicsWorkWithWildcards(string topicPattern, string topicName, bool shouldHaveBeenCalled)
        {
            // Arrange
            var context = new TestBusContext();
            var sender = context.CreateMessageSender();
            var receiver = context.CreateMessageReceiver("testQueue", new []{ topicPattern });

            var message = new EventMessage {Topic = topicName};

            var callbackCalled = false;

            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => callbackCalled = true);

            // Act
            sender.SendMessage(message);
            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreEqual(shouldHaveBeenCalled, callbackCalled);
        }

        [TestMethod]
        [DataRow("TestQueue")]
        [DataRow("MVM.Queue")]
        [DataRow("NewQueueWithItems")]
        public void SendingMessageWithDifferentQueueNamesWorks(string queueName)
        {
            // Arrange
            var context = new TestBusContext();
            var sender = context.CreateMessageSender();
            var receiver = context.CreateMessageReceiver(queueName, new [] { "TestTopic" });

            var message = new EventMessage {Topic = "TestTopic" };

            var callbackCalled = false;

            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => callbackCalled = true);

            // Act
            sender.SendMessage(message);
            Thread.Sleep(WaitTime);

            // Assert
            Assert.IsTrue(callbackCalled);
        }

        [TestMethod]
        [DataRow("TestQueue")]
        [DataRow("MVM.Queue")]
        [DataRow("NewQueueWithItems")]
        public void PausingEventListenerPausesDeliveryInQueue(string queueName)
        {
            // Arrange
            TestBusContext context = new TestBusContext();
            IMessageSender sender = context.CreateMessageSender();
            IMessageReceiver receiver = context.CreateMessageReceiver(queueName, new [] { "TestTopic" });

            EventMessage message = new EventMessage {Topic = "TestTopic" };

            bool callbackCalled = false;

            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => callbackCalled = true);

            // Act
            receiver.Pause();

            sender.SendMessage(message);
            Thread.Sleep(WaitTime);

            // Assert
            Assert.IsFalse(callbackCalled);
        }

        [TestMethod]
        [DataRow("TestQueue")]
        [DataRow("MVM.Queue")]
        public void ResumingEventListenerReActivatesDeliveryInQueue(string queueName)
        {
            // Arrange
            TestBusContext context = new TestBusContext();
            IMessageSender sender = context.CreateMessageSender();
            IMessageReceiver receiver = context.CreateMessageReceiver(queueName, new [] { "TestTopic" });

            EventMessage message = new EventMessage {Topic = "TestTopic" };

            bool callbackCalled = false;

            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => callbackCalled = true);

            receiver.Pause();
            // Act
            sender.SendMessage(message);

            Thread.Sleep(WaitTime);

            receiver.Resume();

            Thread.Sleep(WaitTime);

            // Assert
            Assert.IsTrue(callbackCalled);
        }
    }
}
