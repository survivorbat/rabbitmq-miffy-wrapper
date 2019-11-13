using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;


namespace Minor.Miffy.Test.Component
{
    /// <summary>
    /// TODO: Add pattern matching
    /// </summary>
    [TestClass]
    public class TestBusContextTest
    {
        [TestMethod]
        public void CreateMessageReceiverReturnsInitializedReceiver()
        {
            // Arrange
            var context = new TestBusContext();

            string queueName = "queue";
            string[] topics = new string[2];
            
            // Act
            IMessageReceiver receiver = context.CreateMessageReceiver(queueName, topics);

            // Assert
            Assert.AreSame(queueName, receiver.QueueName);
            Assert.AreSame(topics, receiver.TopicFilters);
        }
        
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
            Thread.Sleep(500);
            
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
            Thread.Sleep(500);
            
            // Assert
            Assert.IsTrue(callbackCalled);
        }
        
        [TestMethod]
        [DataRow("MVM.#.test", "MVM.foo.test", true)]
        [DataRow("MVM.#.##", "MVM.bar.foo.zed.lorem", true)]
        [DataRow("MVM.##.test", "MVM.foo.bar.test", true)]
        [DataRow("Blackjack.#", "Blackjack.foo", true)]
        [DataRow("Blackjack.Whitejack", "#.Blackjack", false)]
        [DataRow("Test.Test.#", "Test.Test.Nee.Nee", false)]
        [DataRow("##.Foo", "Bar.Bar.Foo.Bar", false)]
        [DataRow("##.Foo", "Bar.Bar.Bar", false)]
        [DataRow("##", "Bar.Foo.Dez.Bez", true)]
        [DataRow("#.Foo", "Foo.Foo.Foo", false)]
        [DataRow("#.Foo", "Foo.Foo", true)]
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
            Thread.Sleep(500);
            
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
            Thread.Sleep(500);
            
            // Assert
            Assert.IsTrue(callbackCalled);
        }
    }
}