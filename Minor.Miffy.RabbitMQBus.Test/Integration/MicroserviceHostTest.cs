using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices;
using Minor.Miffy.RabbitMQBus.Test.Integration.EventListeners;
using Minor.Miffy.RabbitMQBus.Test.Integration.Events;
using Minor.Miffy.RabbitMQBus.Test.Integration.Models;
using RabbitMQ.Client;

namespace Minor.Miffy.RabbitMQBus.Test.Integration
{
    [TestClass]
    public class MicroserviceHostTest
    {
        [TestMethod]
        public void EventListenerReceivesMessage()
        {
            // Arrange
            IBusContext<IConnection> busContext = new RabbitMqContextBuilder()
                .WithExchange("TestExchange")
                .WithConnectionString("amqp://guest:guest@localhost")
                .CreateContext();
            
            MicroserviceHost host = new MicroserviceHostBuilder()
                .WithBusContext(busContext)
                .UseConventions()
                .CreateHost();
            
            host.Start();
            
            var publisher = new EventPublisher(busContext);

            var personEvent = new PersonAddedEvent { Person = new Person 
                {
                    FirstName = "Mark",
                    LastName = "Polo",
                    Email = "info@support.com",
                    PhoneNumber = "0672840928"
                }
            };
            
            // Act
            publisher.Publish(personEvent);
            Thread.Sleep(500);
            
            // Assert
            Assert.AreEqual(personEvent.Person, PersonEventListener.ResultEvent);
        }
    }
}