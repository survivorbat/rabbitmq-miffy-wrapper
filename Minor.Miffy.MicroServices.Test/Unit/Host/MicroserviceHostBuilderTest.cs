using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Host;
using Moq;
using RabbitMQ.Client;

namespace Minor.Miffy.MicroServices.Test.Unit.Host
{
    [TestClass]
    public class MicroserviceHostBuilderTest
    {
        [TestMethod]
        public void RegisterDependenciesInvokesConfigMethod()
        {
            // Arrange
            var host = new MicroserviceHostBuilder();
            bool hasBeenCalled = false;

            // Act
            host.RegisterDependencies(e => hasBeenCalled = true);
            
            // Assert
            Assert.IsTrue(hasBeenCalled);
        }

        [TestMethod]
        public void CreateHostReturnsHostWithContext()
        {
            // Arrange
            var contextMock = new Mock<IBusContext<IConnection>>();
            var context = contextMock.Object;
            
            var builder = new MicroserviceHostBuilder();
            builder.WithBusContext(context);

            // Act
            var host = builder.CreateHost();
            
            // Arrange
            Assert.AreSame(context, host.Context);
        }
    }
}