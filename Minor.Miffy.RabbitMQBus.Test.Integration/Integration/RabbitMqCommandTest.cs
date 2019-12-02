using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.RabbitMQBus.Test.Integration.Integration.Models;
using Newtonsoft.Json;

namespace Minor.Miffy.RabbitMQBus.Test.Integration.Integration
{
    [TestClass]
    public class RabbitMqCommandReceiverTest
    {
        [TestMethod]
        [DataRow("test.queue", "Jan")]
        [DataRow("add.hello", "Bart")]
        [DataRow("random.queue.name", "Piet")]
        [DataRow("testQueue", "Vind")]
        [DataRow("hello.queue", "Truus")]
        [DataRow("test.queue", "Femke")]
        public void CommandIsProperlySentThrough(string destQueue, string message)
        {
            // arrange
            using var context = new RabbitMqContextBuilder()
                .WithConnectionString("amqp://guest:guest@localhost")
                .WithExchange("HelloExchange")
                .CreateContext();

            using var receiver = context.CreateCommandReceiver(destQueue);
            receiver.DeclareCommandQueue();

            var sender = context.CreateCommandSender();

            var nameObject = new NameMessage(message);
            var command = new CommandMessage
            {
                DestinationQueue = destQueue,
                Timestamp = 294859,
                CorrelationId = Guid.NewGuid(),
                ReplyQueue = "ReplyQueue",
                Body = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(nameObject))
            };

            // Act
            receiver.StartReceivingCommands(commandMessage =>
            {
                NameMessage name = JsonConvert.DeserializeObject<NameMessage>(Encoding.Unicode.GetString(commandMessage.Body));

                return new CommandMessage
                {
                    CorrelationId = commandMessage.CorrelationId,
                    Timestamp = commandMessage.Timestamp,
                    Body = Encoding.Unicode.GetBytes($"Hallo {name.Name}")
                };
            });

            var result = sender.SendCommandAsync(command);

            // Assert
            var stringResult = Encoding.Unicode.GetString(result.Result.Body);
            Assert.AreEqual($"Hallo {message}", stringResult);
        }
        
        [TestMethod]
        [DataRow("test.queue", "Jan", "Peters")]
        [DataRow("add.hello", "Bart", "van de Weg")]
        [DataRow("random.queue.name", "Piet", "Jeters")]
        [DataRow("testQueue", "Vind", "van de Kruisweg")]
        [DataRow("hello.queue", "Truus", "van Ijzerman")]
        [DataRow("test.queue", "Femke", "Lechman")]
        public void PersonFullNamerIsProperlyReceived(string destQueue, string firstName, string lastName)
        {
            // arrange
            using var context = new RabbitMqContextBuilder()
                .WithConnectionString("amqp://guest:guest@localhost")
                .WithExchange("HelloExchange")
                .CreateContext();

            using var receiver = context.CreateCommandReceiver(destQueue);
            receiver.DeclareCommandQueue();

            var sender = context.CreateCommandSender();

            var nameObject = new Person
            {
                FirstName = firstName,
                LastName = lastName
            };
            
            var command = new CommandMessage
            {
                DestinationQueue = destQueue,
                Timestamp = 294859,
                CorrelationId = Guid.NewGuid(),
                ReplyQueue = "ReplyQueue",
                Body = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(nameObject))
            };

            // Act
            receiver.StartReceivingCommands(commandMessage =>
            {
                Person person = JsonConvert.DeserializeObject<Person>(Encoding.Unicode.GetString(commandMessage.Body));
                person.FullName = $"{person.FirstName} {person.LastName}";
                string jsonResponse = JsonConvert.SerializeObject(person);
                
                return new CommandMessage
                {
                    CorrelationId = commandMessage.CorrelationId,
                    Timestamp = commandMessage.Timestamp,
                    Body = Encoding.Unicode.GetBytes(jsonResponse)
                };
            });

            var result = sender.SendCommandAsync(command);

            // Assert
            var stringResult = Encoding.Unicode.GetString(result.Result.Body);
            var personResult = JsonConvert.DeserializeObject<Person>(stringResult);
            Assert.AreEqual($"{firstName} {lastName}", personResult.FullName);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            RabbitMqCleanUp.DeleteExchange("TestExchange", "amqp://guest:guest@localhost");
        }
    }
}