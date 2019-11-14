using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices.Test.Integration.EventListeners;
using Minor.Miffy.MicroServices.Test.Integration.Events;
using Minor.Miffy.MicroServices.Test.Integration.Models;
using Minor.Miffy.RabbitMQBus;

namespace Minor.Miffy.MicroServices.Test.Integration
{
    [TestClass]
    public class MicroserviceHostTest
    {
        [TestMethod]
        [DataRow("Mark", "van Brugge", "m.brugge@infosupport.net", "0603463096")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060323305")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060344556")]
        [DataRow("Cristian", "de Hamer", "c.hamer@info.com", "0603445562")]
        public void EventListenerReceivesMessage(string firstName, string lastName, string email, string phone)
        {
            // Arrange
            using var busContext = new RabbitMqContextBuilder()
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
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PhoneNumber = phone
                }
            };
            
            // Act
            publisher.Publish(personEvent);
            Thread.Sleep(500);
            
            // Assert
            Assert.AreEqual(personEvent, PersonEventListener.ResultEvent);
        }
    }
}