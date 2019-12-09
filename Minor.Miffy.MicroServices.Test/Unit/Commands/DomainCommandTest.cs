using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Commands;
using Newtonsoft.Json;

namespace Minor.Miffy.MicroServices.Test.Unit.Commands
{
    [TestClass]
    public class DomainCommandTest
    {
        [TestMethod]
        [DataRow("DestinationQueue")]
        [DataRow("MVM.Blackjack.Destination")]
        public void ConstructorProperlyInitializesFields(string destinationQueue)
        {
            // Act
            DomainCommand @event = new TestCommand(destinationQueue);
            
            // Assert
            Assert.IsNotNull(@event.Id);
            Assert.IsNotNull(@event.Timestamp);
            Assert.AreEqual(destinationQueue, @event.DestinationQueue);
        }

        [TestMethod]
        [DataRow("calculate.queue", "Jan is toegevoegd")]
        [DataRow("foreign.queue", "Event happened")]
        [DataRow("serializer.queue", "All the things")]
        public void ProperFieldsAreSerialized(string destinationQueue, string data)
        {
            // Arrange
            TestCommand @event = new TestCommand(destinationQueue) {DataField = data};
            
            // Act
            string result = JsonConvert.SerializeObject(@event);
            
            Console.WriteLine(result);
            
            // Assert
            Assert.IsTrue(result.Contains($"\"DestinationQueue\":\"{destinationQueue}\""));
            Assert.IsTrue(result.Contains($"\"DataField\":\"{data}\""));
        }
    }
}