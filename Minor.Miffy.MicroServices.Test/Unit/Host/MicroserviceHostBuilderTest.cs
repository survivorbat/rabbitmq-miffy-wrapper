using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        private class TestType1 {}
        private class TestType2 {}
        private class TestType3 {}
        private class TestType4 {}

        [TestMethod]
        public void RegisterDependenciesRegistersOneExtraDependency()
        {
            // Arrange
            using var builder = new MicroserviceHostBuilder();

            // Act
            builder.RegisterDependencies(e =>
            {
                e.AddTransient<TestType1>();
            });

            // Assert
            Assert.AreEqual(2, builder.ServiceCollection.Count);

            IServiceProvider serviceProvider = builder.ServiceCollection.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<TestType1>());
        }

        [TestMethod]
        public void RegisterDependenciesRegistersMultipleDependencies()
        {
            // Arrange
            using var builder = new MicroserviceHostBuilder();

            // Act
            builder.RegisterDependencies(e =>
            {
                e.AddTransient<TestType1>();
                e.AddTransient<TestType2>();
                e.AddTransient<TestType3>();
                e.AddTransient<TestType4>();
            });

            // Assert
            Assert.AreEqual(5, builder.ServiceCollection.Count);

            IServiceProvider serviceProvider = builder.ServiceCollection.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<TestType1>());
            Assert.IsNotNull(serviceProvider.GetService<TestType2>());
            Assert.IsNotNull(serviceProvider.GetService<TestType3>());
            Assert.IsNotNull(serviceProvider.GetService<TestType4>());
        }

        [TestMethod]
        public void RegisterDependenciesAddsToExistingServiceCollection()
        {
            // Arrange
            using var builder = new MicroserviceHostBuilder();

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<TestType1>();

            // Act
            builder.RegisterDependencies(services => { services.AddTransient<TestType2>(); });
            builder.RegisterDependencies(serviceCollection);

            // Assert
            IServiceProvider serviceProvider = builder.ServiceCollection.BuildServiceProvider();
            Assert.IsNotNull(serviceProvider.GetService<TestType1>());
            Assert.IsNotNull(serviceProvider.GetService<TestType2>());
        }
    }
}
