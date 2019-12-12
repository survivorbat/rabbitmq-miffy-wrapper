using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Events;
using Newtonsoft.Json;

namespace Minor.Miffy.MicroServices.Test.Unit.Events
{
    [TestClass]
    public class DomainEventTest
    {
        [TestMethod]
        [DataRow("TestTopic")]
        [DataRow("MVM.Blackjack")]
        public void ConstructorProperlyInitializesFields(string topic)
        {
            // Act
            DomainEvent @event = new TestEvent(topic);

            // Assert
            Assert.IsNotNull(@event.Id);
            Assert.IsNotNull(@event.Timestamp);
            Assert.AreEqual(topic, @event.Topic);
        }

        [TestMethod]
        [DataRow("test.topic", "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")]
        [DataRow("topic.test", "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")]
        public void ContructorProperlyInitializesGuid(string topic, string guidString)
        {
            // Arrange
            Guid guid = Guid.Parse(guidString);

            // Act
            DomainEvent @event = new TestEvent(topic, guid);

            // Assert
            Assert.IsNotNull(@event.Id);
            Assert.IsNotNull(@event.Timestamp);
            Assert.AreEqual(guid, @event.ProcessId);
            Assert.AreEqual(topic, @event.Topic);
        }

        [TestMethod]
        [DataRow("Blackjack", "Jan is toegevoegd")]
        [DataRow("TestEvent", "Event happened")]
        [DataRow("Serializing", "All the things")]
        public void ProperFieldsAreSerialized(string topic, string data)
        {
            // Arrange
            TestEvent @event = new TestEvent(topic) {DataField = data};

            // Act
            string result = JsonConvert.SerializeObject(@event);

            // Assert
            Assert.IsTrue(result.Contains($"\"Topic\":\"{topic}\""));
            Assert.IsTrue(result.Contains($"\"DataField\":\"{data}\""));
        }

        private class TestType : DomainEvent
        {
            public TestType() : base(null)
            {
            }
        }

        private class RandomType : DomainEvent
        {
            public RandomType() : base(null)
            {
            }
        }

        private class SpecialType : DomainEvent
        {
            public SpecialType() : base(null)
            {
            }
        }

        [TestMethod]
        [DataRow(typeof(RandomType), "RandomType")]
        [DataRow(typeof(TestType), "TestType")]
        [DataRow(typeof(SpecialType), "SpecialType")]
        public void TypeIsProperlySetInField(Type type, string typeName)
        {
            // Arrange
            DomainEvent @event = Activator.CreateInstance(type) as DomainEvent;

            // Assert
            Assert.AreEqual(typeName, @event?.Type);
        }
    }
}
