namespace Minor.Miffy.MicroServices.Test.Integration.Models
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public override bool Equals(object obj) => obj is Person person && Equals(person);

        private bool Equals(Person person) =>
            FirstName.Equals(person.FirstName)
            && LastName.Equals(person.LastName)
            && PhoneNumber.Equals(person.PhoneNumber)
            && Email.Equals(person.Email);

        public override int GetHashCode() =>
            FirstName.GetHashCode()
            ^ LastName.GetHashCode()
            ^ Email.GetHashCode()
            ^ PhoneNumber.GetHashCode();
    }
}