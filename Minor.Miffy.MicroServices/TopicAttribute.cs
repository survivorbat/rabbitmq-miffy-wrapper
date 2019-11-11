using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Miffy.MicroServices
{
    /// <summary>
    /// This attribute should decorate each eventhandling method.
    /// All events matching the topicExpression will be handled by this method. 
    /// (the event wil possibly also handled by other methods with a matching 
    /// topicExpression)
    /// </summary>
    public class TopicAttribute : Attribute
    {
        public TopicAttribute(string topicPattern)
        {
            // TODO
        }

        // TODO
    }
}
