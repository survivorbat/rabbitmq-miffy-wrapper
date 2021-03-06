using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;
using Miffy.MicroServices.Host;
using Miffy.Microservices.Test.Integration.Integration.EventListeners;
using Miffy.Microservices.Test.Integration.Integration.Events;
using Miffy.Microservices.Test.Integration.Integration.Models;
using Miffy.RabbitMQBus;

namespace Miffy.Microservices.Test.Integration.Integration
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
        [DataRow("{}{}{}")]
        [DataRow("{223223}")]
        [DataRow("{|]")]
        public void EventListenerDoesCallListenerOnInvalidJson(string body)
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

            EventPublisher publisher = new EventPublisher(busContext);

            // Act
            publisher.Publish(242424, "PeopleApp.Persons.New", Guid.NewGuid(), "PersonEvent", body);
            Thread.Sleep(WaitTime);

            // Assert
            Assert.IsNull(PersonEventListener.ResultEvent);
        }

        [TestMethod]
        [DataRow("Mark", "van Brugge", "m.brugge@infosupport.net", "0603463096")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060323305")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060344556")]
        [DataRow("Cristian", "de Hamer", "c.hamer@info.com", "0603445562")]
        public void EventListenerReceivesMessageAsync(string firstName, string lastName, string email, string phone)
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
            publisher.PublishAsync(personEvent);
            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreEqual(personEvent, PersonEventListener.ResultEvent);
        }

        [TestMethod]
        [DataRow("Mark", "van Brugge", "m.brugge@infosupport.net", "0603463096")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060323305")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060344556")]
        [DataRow("Cristian", "de Hamer", "c.hamer@info.com", "0603445562")]
        public void ReceivingMessagesIsPaused(string firstName, string lastName, string email, string phone)
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
            host.Pause();

            publisher.Publish(personEvent);
            Thread.Sleep(WaitTime);

            // Assert
            Assert.AreNotEqual(personEvent, PersonEventListener.ResultEvent);
        }

        [TestMethod]
        [DataRow("Mark", "van Brugge", "m.brugge@infosupport.net", "0603463096")]
        [DataRow("Haspran", "Hermadosi", "h.h@iosi.com", "060323305")]
        public void ReceivingMessagesIsResumed(string firstName, string lastName, string email, string phone)
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
            host.Pause();

            publisher.Publish(personEvent);
            Thread.Sleep(WaitTime);

            host.Resume();
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
                .WithConnectionString("amqp://guest:guest@localhost")
                .WithExchange("TestExchange")
                .CreateContext();

            var builder = new MicroserviceHostBuilder()
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
                .WithQueueName("QueueName3")
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
                .WithQueueName("QueueName2")
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
            RabbitMqCleanUp.DeleteQueue("PeopleApp.Cats.New", "amqp://guest:guest@localhost");
            RabbitMqCleanUp.DeleteQueue("Test.Command.Listener", "amqp://guest:guest@localhost");
            RabbitMqCleanUp.DeleteQueue("PeopleApp.Persons.New", "amqp://guest:guest@localhost");
        }
    }
}
