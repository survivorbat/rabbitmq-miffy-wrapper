using System.Collections.Generic;
using System.Linq;
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
                .AddEventListener<PersonEventListener>()
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
        
        [TestMethod]
        [DataRow("Mark", "van Brugge", "m.brugge@infosupport.net", "0603463096")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060323305")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060344556")]
        [DataRow("Cristian", "de Hamer", "c.hamer@info.com", "0603445562")]
        public void EventListenerReceivesMessageWithWildcards(string firstName, string lastName, string email, string phone)
        {
            // Arrange
            using var busContext = new RabbitMqContextBuilder()
                .WithExchange("TestExchange")
                .WithConnectionString("amqp://guest:guest@localhost")
                .CreateContext();
            
            MicroserviceHost host = new MicroserviceHostBuilder()
                .WithBusContext(busContext)
                .AddEventListener<WildCardPersonEventListener>()
                .AddEventListener<WildCardPersonEventListener2>()
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
            Assert.AreEqual(personEvent, WildCardPersonEventListener.ResultEvent);
            Assert.AreEqual(personEvent, WildCardPersonEventListener2.ResultEvent);
        }
        
        [TestMethod]
        [DataRow("Mark", "van Brugge", "m.brugge@infosupport.net", "0603463096")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060323305")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060344556")]
        [DataRow("Cristian", "de Hamer", "c.hamer@info.com", "0603445562")]
        public void FanInListenerReceivesAllMessages(string firstName, string lastName, string email, string phone)
        {
            // Arrange
            using var busContext = new RabbitMqContextBuilder()
                .WithExchange("TestExchange")
                .WithConnectionString("amqp://guest:guest@localhost")
                .CreateContext();
            
            MicroserviceHost host = new MicroserviceHostBuilder()
                .WithBusContext(busContext)
                .AddEventListener<FanInEventListener>()
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
            Assert.AreEqual(personEvent, FanInEventListener.ResultEvent);
        }
        
        [TestMethod]
        [DataRow("Aspra")]
        [DataRow("Luna")]
        [DataRow("Brownie")]
        [DataRow("Zilver")]
        public void CatEventDoesNotTriggerPersonEvent(string name)
        {
            // Arrange
            using var busContext = new RabbitMqContextBuilder()
                .WithExchange("TestExchange")
                .WithConnectionString("amqp://guest:guest@localhost")
                .CreateContext();
            
            MicroserviceHost host = new MicroserviceHostBuilder()
                .WithBusContext(busContext)
                .AddEventListener<PersonEventListener>()
                .AddEventListener<CatEventListener>()
                .CreateHost();
            
            host.Start();
            
            var publisher = new EventPublisher(busContext);

            var catEvent = new CatAddedEvent { Cat = new Cat {Name = name}};
            
            // Act
            publisher.Publish(catEvent);
            Thread.Sleep(500);
            
            // Assert
            Assert.IsNull(PersonEventListener.ResultEvent);
            Assert.AreEqual(catEvent, CatEventListener.ResultEvent);
        }
        
        [TestMethod]
        [DataRow("Aspra", "Chris", "Kat")]
        [DataRow("Bram")]
        [DataRow("Olaf", "Janneke", "Piet", "Poes")]
        [DataRow("Kat1", "Kat2", "Kat3", "Kat4", "Kat5")]
        public void EventListenerHandlesMultipleEvents(params string[] names)
        {
            // Arrange
            using var busContext = new RabbitMqContextBuilder()
                .WithExchange("TestExchange")
                .WithConnectionString("amqp://guest:guest@localhost")
                .CreateContext();
            
            MicroserviceHost host = new MicroserviceHostBuilder()
                .WithBusContext(busContext)
                .AddEventListener<SpamEventListener>()
                .CreateHost();
            
            host.Start();
            
            var publisher = new EventPublisher(busContext);

            var catEvents = names.Select(e => new CatAddedEvent {Cat = new Cat {Name = e}}).ToList();
            
            // Act
            foreach (var @event in catEvents)
            {
                publisher.Publish(@event);
            }
            
            Thread.Sleep(3000);
            
            // Assert
            CollectionAssert.AreEqual(catEvents, SpamEventListener.ResultEvents);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            SpamEventListener.ResultEvents = new List<CatAddedEvent>();
            WildCardPersonEventListener.ResultEvent = null;
            WildCardPersonEventListener2.ResultEvent = null;
            PersonEventListener.ResultEvent = null;
            CatEventListener.ResultEvent = null;
            FanInEventListener.ResultEvent = null;
        }
    }
}