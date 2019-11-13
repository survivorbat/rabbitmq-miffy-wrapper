using Microsoft.VisualStudio.TestTools.UnitTesting;
using VoorbeeldMicroService.Constants;

namespace VoorbeeldMicroService.Test.Unit
{
    [TestClass]
    public class PolisToegevoegdEventTest
    {
        [TestMethod]
        public void ConstructorPassesProperTopicName()
        {
            // Act
            var @event = new PolisToegevoegdEvent();
            
            // Assert
            Assert.AreEqual(TopicNames.MvmPolisbeheerPolisToegevoegd, @event.Topic);
        }
    }
}