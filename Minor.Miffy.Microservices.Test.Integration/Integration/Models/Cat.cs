namespace Minor.Miffy.Microservices.Test.Integration.Integration.Models
{
    public class Cat
    {
        public string Name { get; set; }
        
        public override bool Equals(object obj) => obj is Cat cat && Equals(cat);

        private bool Equals(Cat cat) => Name.Equals(cat.Name);

        public override int GetHashCode() => Name.GetHashCode();
    }
}