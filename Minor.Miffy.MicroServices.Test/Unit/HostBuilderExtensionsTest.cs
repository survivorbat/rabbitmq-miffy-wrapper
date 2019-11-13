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
            HostBuilderExtensions.ConfigureMicroServiceHostDefaults(builder.Object,
                hostBuilder => hasBeenCalled = true);

            // Assert
            Assert.IsTrue(hasBeenCalled);
        }
    }
}