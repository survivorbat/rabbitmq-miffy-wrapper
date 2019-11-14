using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Miffy.TestBus;

namespace Minor.Miffy.Test.Unit.TestBus
{
    [TestClass]
    public class TestBusKeyTest
    {
        [TestMethod]
        [DataRow("test", @"^test$")]
        [DataRow("test.test", @"^test\.test$")]
        [DataRow("test.t.test", @"^test\.t\.test$")]
        [DataRow("foo.bar.bez", @"^foo\.bar\.bez$")]
        [DataRow("*foo", @"^[^.]*foo$")]
        [DataRow("*bar*", @"^[^.]*bar[^.]*$")]
        [DataRow("*.bex.*", @"^[^.]*\.bex\.[^.]*$")]
        [DataRow("*.foo.bex.*", @"^[^.]*\.foo\.bex\.[^.]*$")]
        [DataRow("#bar", @"^.*bar$")]
        [DataRow("bar#", @"^bar.*$")]
        [DataRow("#bar#", @"^.*bar.*$")]
        [DataRow("#bar*", @"^.*bar[^.]*$")]
        [DataRow(".....", @"^\.\.\.\.\.$")]
        [DataRow("#####", @"^.*.*.*.*.*$")]
        [DataRow("*****", @"^[^.]*[^.]*[^.]*[^.]*[^.]*$")]
        public void RegexIsProperlyParsed(string topic, string result)
        {
            // Act
            var key = new TestBusKey("testQueue", topic);

            // Assert
            Assert.AreEqual(result, key.TopicPattern.ToString());
        }

        [TestMethod]
        [DataRow("test.queue", "test.topic", "test.queue", "test.topic", true)]
        [DataRow("test*", "test*", "test*", "test*", true)]
        [DataRow("testA", "test", "testB", "test", false)]
        [DataRow("test", "testA", "test", "testB", false)]
        [DataRow("test", "test*", "test", "test#", false)]
        [DataRow("foo", "bar", "bez", "foo", false)]
        public void TwoKeysWithTheSameValuesAreEqual(string queueA, string topicA, string queueB, string topicB, bool expected)
        {
            // Arrange
            var keyA = new TestBusKey(queueA, topicA);
            var keyB = new TestBusKey(queueB, topicB);
            
            // Act
            bool result = keyA.Equals(keyB);

            // Assert
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void EqualsWithNullReturnsFalse()
        {
            // Arrange
            var key = new TestBusKey("test", "test");
            
            // Act
            bool result = key.Equals(null);

            // Assert
            Assert.IsFalse(result);
        }
    }
}