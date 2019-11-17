using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component
{
    public class DummyEvent : DomainEvent
    {
        public DummyEvent(string topic) : base(topic) { }
        
        public string DummyText { get; set; }
        
        private bool Equals(DummyEvent other) =>
            DummyText == other.DummyText
            && Timestamp == other.Timestamp
            && Id == other.Id
            && Topic == other.Topic;

        public override bool Equals(object obj) => Equals((DummyEvent) obj);

        public override int GetHashCode() => DummyText.GetHashCode() + base.GetHashCode();
        
        public override string ToString() => $"[{Timestamp}] <{Id}> {Topic}: {DummyText}";
    }
}