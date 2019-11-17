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
    }
}