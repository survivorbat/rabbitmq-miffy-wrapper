using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Minor.Miffy.MicroServices.Test.Unit
{
    [TestClass]
    public class HostBuilderExtensionsTest
    {
        [TestMethod]
        public void ConfigureMicroServiceHostDefaultsInvokesConfiguration()
        {
            // Arrange
            var builder = new Mock<IHostBuilder>();
            bool hasBeenCalled = false;

            // Act
            builder.Object.ConfigureMicroServiceHostDefaults(hostBuilder => hasBeenCalled = true);

            // Assert
            Assert.IsTrue(hasBeenCalled);
        }
    }
}