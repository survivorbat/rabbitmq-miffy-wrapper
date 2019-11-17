using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Unit.Events
{
    [TestClass]
    public class TopicAttributeTest
    {
        [TestMethod]
        [DataRow("TestTopic")]
        [DataRow("MVM.Blackjack")]
        [DataRow("MVM.Blackjack.#")]
        public void TopicIsProperlySet(string topic)
        {
            // Act
            TopicAttribute attribute = new TopicAttribute(topic);
            
            // Assert
            Assert.AreEqual(topic, attribute.TopicPattern);
        }
    }
}