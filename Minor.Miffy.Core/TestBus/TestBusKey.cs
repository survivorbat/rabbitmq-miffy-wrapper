using System;
using System.Text.RegularExpressions;

namespace Minor.Miffy.TestBus
{
    /// <summary>
    /// Wrapper class that contains a queuename and a topic name
    /// </summary>
    internal class TestBusKey
    {
        /// <summary>
        /// Name of the queue
        /// </summary>
        private readonly string _queueName;
        
        /// <summary>
        /// Name of the topic
        /// </summary>
        private readonly string _topicName;
        
        /// <summary>
        /// Pattern of the topic name
        /// </summary>
        internal Regex TopicPattern { get; }

        /// <summary>
        /// Initialize a testbuskey
        /// </summary>
        public TestBusKey(string queueName, string topicName)
        {
            _queueName = queueName;
            _topicName = topicName;

            string regex = topicName
                .Replace(".", @"\.")
                .Replace("*", @"[^.]*")
                .Replace("#", @".*");

            TopicPattern = new Regex($"^{regex}$");
        }

        /// <summary>
        /// Equals so that values can be compared
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            
            TestBusKey key = obj as TestBusKey;
            return Equals(key);
        }

        private bool Equals(TestBusKey other) => _queueName == other._queueName && _topicName == other._topicName;
        
        public override int GetHashCode() => (_queueName.GetHashCode() * 397) ^ (_topicName.GetHashCode() * 379);
    }
}