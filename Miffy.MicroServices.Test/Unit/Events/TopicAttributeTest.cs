using Microsoft.VisualStudio.TestTools.UnitTesting;
using Miffy.MicroServices.Events;

namespace Miffy.MicroServices.Test.Unit.Events
{
    [TestClass]
    public class TopicAttributeTest
    {
        [TestMethod]
        [DataRow("TestTopic")]
        [DataRow("MVM.Blackjack")]
        [DataRow("MVM.Blackjack.#")]
        public void TopicIsProperlySet(string topic)
        {
            // Act
            TopicAttribute attribute = new TopicAttribute(topic);

            // Assert
            Assert.AreEqual(topic, attribute.TopicPattern);
        }

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
        public void TopicIsProperlyTranslatedToRegex(string topic, string expectedRegex)
        {
            // Act
            TopicAttribute attribute = new TopicAttribute(topic);

            // Assert
            Assert.AreEqual(expectedRegex, attribute.TopicRegularExpression.ToString());
        }
    }
}
