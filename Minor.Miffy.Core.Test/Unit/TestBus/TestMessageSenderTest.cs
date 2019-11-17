using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;
using Moq;

namespace Minor.Miffy.Test.Unit.TestBus
{
    [TestClass]
    public class TestMessageSenderTest
    {
        [TestMethod]
        public void SendMessageDoesNothingOnEmptyDictionary()
        {
            // Arrange
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var dictionary = new Dictionary<TestBusKey, TestBusQueueWrapper<EventMessage>>();

            context.SetupGet(e => e.DataQueues)
                .Returns(dictionary);
            
            var sender = new TestMessageSender(context.Object);
            
            var message = new EventMessage { Topic = "UnknownTopic"};
            
            // Act
            sender.SendMessage(message);
            
            // Assert
            Assert.AreEqual(0, dictionary.Count);
        }

        [TestMethod]
        [DataRow("testTopic")]
        [DataRow("topic.#.miffy")]
        public void SendMessageDoesNothingWithoutAvailableQueue(string topic)
        {
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var dictionary = new Dictionary<TestBusKey, TestBusQueueWrapper<EventMessage>>();

            context.SetupGet(e => e.DataQueues)
                .Returns(dictionary);
            
            var sender = new TestMessageSender(context.Object);
            
            var message = new EventMessage { Topic = "UnknownTopic"};
            
            // Act
            sender.SendMessage(message);
            
            // Assert
            var key = new TestBusKey("testQueue", topic);
            Assert.IsFalse(dictionary.ContainsKey(key));
        }
        
        [TestMethod]
        [DataRow("testTopic")]
        [DataRow("topic.#.miffy")]
        [DataRow("miffy.*")]
        public void SendMessageAddsMessageToQueueIfQueueExists(string topic)
        {
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var dictionary = new Dictionary<TestBusKey, TestBusQueueWrapper<EventMessage>>();
            var key = new TestBusKey("testQueue", topic);

            dictionary[key] = new TestBusQueueWrapper<EventMessage>();

            context.SetupGet(e => e.DataQueues)
                .Returns(dictionary);
            
            var sender = new TestMessageSender(context.Object);
            
            var message = new EventMessage { Topic = topic};
            
            // Act
            sender.SendMessage(message);
            
            // Assert
            Assert.IsTrue(dictionary.ContainsKey(key));
            Assert.AreEqual(1, dictionary[key].Queue.Count);
        }
        
        [TestMethod]
        [DataRow("testTopic")]
        [DataRow("topic.#.miffy")]
        [DataRow("miffy.*")]
        public void SendMessageFlagsAutoResetEvent(string topic)
        {
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var dictionary = new Dictionary<TestBusKey, TestBusQueueWrapper<EventMessage>>();
            var key = new TestBusKey("testQueue", topic);

            dictionary[key] = new TestBusQueueWrapper<EventMessage>();

            context.SetupGet(e => e.DataQueues)
                .Returns(dictionary);
            
            var sender = new TestMessageSender(context.Object);
            
            var message = new EventMessage { Topic = topic};
            
            // Act
            sender.SendMessage(message);
            
            // Assert
            Assert.IsTrue(dictionary.ContainsKey(key));
            Assert.IsTrue(dictionary[key].AutoResetEvent.WaitOne(0));
        }
        
        [TestMethod]
        [DataRow("testTopic", "testTopic")]
        [DataRow("topic.#.miffy", "topic.topic.topic.miffy")]
        [DataRow("miffy.*", "miffy.all")]
        [DataRow("miffy.*.test", "miffy.all.test")]
        public void TopicsAreSelectedByRegex(string regexTopic, string topic)
        {
            Mock<TestBusContext> context = new Mock<TestBusContext>(MockBehavior.Strict);
            var dictionary = new Dictionary<TestBusKey, TestBusQueueWrapper<EventMessage>>();
            var key = new TestBusKey("testQueue", topic);

            dictionary[key] = new TestBusQueueWrapper<EventMessage>();

            context.SetupGet(e => e.DataQueues)
                .Returns(dictionary);
            
            var sender = new TestMessageSender(context.Object);
            var message = new EventMessage { Topic = topic};
            
            // Act
            sender.SendMessage(message);
            
            // Assert
            Assert.IsTrue(dictionary.ContainsKey(key));
        }
    }
}