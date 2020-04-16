namespace Miffy.RabbitMQBus.Test.Integration.Integration.Models
{
    public class NameMessage
    {
        public NameMessage() { }
        public NameMessage(string name) => Name = name;
        public string Name { get; set; }
    }
}