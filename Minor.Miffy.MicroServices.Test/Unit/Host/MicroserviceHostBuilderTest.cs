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
            var host = new MicroserviceHostBuilder();
            bool hasBeenCalled = false;

            // Act
            host.RegisterDependencies(e => hasBeenCalled = true);

            // Assert
            Assert.IsTrue(hasBeenCalled);
        }

        [TestMethod]
        public void QueueNameIsAutomaticallySet()
        {
            // Act
            var host = new MicroserviceHostBuilder();

            // Assert
            Assert.IsNotNull(host.QueueName);
            Assert.IsTrue(host.QueueName.StartsWith("unset_queue_name_"));
        }

        [TestMethod]
        [DataRow("test.queue")]
        [DataRow("VeryAwesomeQueue")]
        public void WithQueueNameSetsQueueProperly(string queueName)
        {
            // Arrange
            var host = new MicroserviceHostBuilder();

            // Act
            host.WithQueueName(queueName);

            // Assert
            Assert.AreEqual(queueName, host.QueueName);
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

        private class TestType1 {}
        private class TestType2 {}
        private class TestType3 {}
        private class TestType4 {}

        [TestMethod]
        public void RegisterDependenciesRegistersOneExtraDependency()
        {
            // Arrange
            var builder = new MicroserviceHostBuilder();

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
            var builder = new MicroserviceHostBuilder();

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
            var builder = new MicroserviceHostBuilder();

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
