using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Minor.Miffy.RabbitMQBus.Test.Integration
{
    [TestClass]
    public class RabbitMqContextBuilderTest
    {
        [TestMethod]
        public void CreateContextReturnsInitializedContext()
        {
            // TODO
        }

        [TestMethod]
        [DataRow("testExcange")]
        [DataRow("Blackjack.Exchange")]
        public void CreateContextInitializesExchange(string exchangeName)
        {
            // TODO
        }
    }
}