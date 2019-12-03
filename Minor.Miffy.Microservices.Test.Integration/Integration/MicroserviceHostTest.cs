using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.MicroServices;
using Minor.Miffy.MicroServices.Commands;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Host;
using Minor.Miffy.Microservices.Test.Integration.Integration.EventListeners;
using Minor.Miffy.Microservices.Test.Integration.Integration.Events;
using Minor.Miffy.Microservices.Test.Integration.Integration.Models;
using Minor.Miffy.RabbitMQBus;

namespace Minor.Miffy.Microservices.Test.Integration.Integration
{
    [TestClass]
    public class MicroserviceHostTest
    {
        private const int WaitTime = 5000;
            
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
            
            using var host = new MicroserviceHostBuilder()
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
            Thread.Sleep(WaitTime);
            
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
            
            using var host = new MicroserviceHostBuilder()
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
            Thread.Sleep(WaitTime);
            
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
            
            using var builder = new MicroserviceHostBuilder()
                .WithBusContext(busContext)
                .AddEventListener<FanInEventListener>();
            
            using var host = builder.CreateHost();
            
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
            Thread.Sleep(WaitTime);
            
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
            
            using var host = new MicroserviceHostBuilder()
                .WithBusContext(busContext)
                .AddEventListener<PersonEventListener>()
                .AddEventListener<CatEventListener>()
                .CreateHost();
            
            host.Start();
            
            var publisher = new EventPublisher(busContext);

            var catEvent = new CatAddedEvent { Cat = new Cat {Name = name}};
            
            // Act
            publisher.Publish(catEvent);
            Thread.Sleep(WaitTime);
            
            // Assert
            Assert.IsNull(PersonEventListener.ResultEvent);
            Assert.AreEqual(catEvent, CatEventListener.ResultEvent);
        }
        
        [TestMethod]
        [DataRow("Bram")]
        [DataRow("Aspra", "Chris", "Kat")]
        [DataRow("Jan", "Piet")]
        [DataRow("Job", "Jen", "Jas", "Snor")]
        [DataRow("Olaf", "Janneke", "Piet", "Poes")]
        [DataRow("Kat1", "Kat2", "Kat3", "Kat4", "Kat5")]
        public void EventListenerHandlesMultipleEvents(params string[] names)
        {
            // Arrange
            using var busContext = new RabbitMqContextBuilder()
                .WithExchange("TestExchange")
                .WithConnectionString("amqp://guest:guest@localhost")
                .CreateContext();
            
            using var host = new MicroserviceHostBuilder()
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
            
            Thread.Sleep(WaitTime);

            CollectionAssert.AreEquivalent(catEvents, SpamEventListener.ResultEvents);
        }
        
        [TestMethod]
        [DataRow("TestException")]
        [DataRow("NullPointerException")]
        [DataRow("EverythingIsOnFireException")]
        public void CommandReturnsProperException(string message)
        {
            // Arrange
            using var busContext = new RabbitMqContextBuilder()
                .WithExchange("TestExchange")
                .WithConnectionString("amqp://guest:guest@localhost")
                .CreateContext();

            using var host = new MicroserviceHostBuilder()
                .WithBusContext(busContext)
                .AddEventListener<ErrorEventListener>()
                .CreateHost();
            
            host.Start();

            var command = new DummyCommand(message);
            var publisher = new CommandPublisher(busContext);

            // Act
            Task<DummyCommand> Act() => publisher.PublishAsync<DummyCommand>(command);
            
            // Arrange
            var exception = Assert.ThrowsExceptionAsync<DestinationQueueException>(Act);
            Assert.AreEqual("Received error command from queue Test.Command.Listener", exception.Result.Message);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            SpamEventListener.ResultEvents = new List<CatAddedEvent>();
            CatEventListener.ResultEvent = null;
            WildCardPersonEventListener.ResultEvent = null;
            WildCardPersonEventListener2.ResultEvent = null;
            PersonEventListener.ResultEvent = null;
            FanInEventListener.ResultEvent = null;
            RabbitMqCleanUp.DeleteExchange("TestExchange", "amqp://guest:guest@localhost");
        }
    }
}