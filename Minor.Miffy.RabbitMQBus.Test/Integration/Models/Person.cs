namespace Minor.Miffy.RabbitMQBus.Test.Integration.Models
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public override bool Equals(object obj) => obj?.GetHashCode() == GetHashCode();
    }
}