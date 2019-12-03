using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
            using var host = new MicroserviceHostBuilder();
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
            
            using var builder = new MicroserviceHostBuilder();
            builder.WithBusContext(context);

            // Act
            var host = builder.CreateHost();
            
            // Arrange
            Assert.AreSame(context, host.Context);
        }
        
        [TestMethod]
        public void LoggerFactoryIsProperlyDisposed()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            var loggerFactory = loggerFactoryMock.Object;

            using var builder = new MicroserviceHostBuilder();
            builder.SetLoggerFactory(loggerFactory);

            // Act
            builder.Dispose();
            
            // Arrange
            loggerFactoryMock.Verify(e => e.Dispose());
        }
    }
}