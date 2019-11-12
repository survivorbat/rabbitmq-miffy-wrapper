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
            Thread.Sleep(4000);
            
            // Assert
            Assert.IsTrue(callbackCalled);
        }
        
        [TestMethod]
        [DataRow("test.topic")]
        [DataRow("topicTest")]
        [DataRow("Test#Topic#Test")]
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
            Thread.Sleep(4000);
            
            // Assert
            Assert.IsTrue(callbackCalled);
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
            var receiver = context.CreateMessageReceiver(queueName, new []{ "TestTopic" });
            
            var message = new EventMessage {Topic = "TestTopic" };

            var callbackCalled = false;
            
            receiver.StartReceivingMessages();
            receiver.StartHandlingMessages(e => callbackCalled = true);
            
            // Act
            sender.SendMessage(message);
            Thread.Sleep(4000);
            
            // Assert
            Assert.IsTrue(callbackCalled);
        }
    }
}