using System.Collections;
using Minor.Miffy.MicroServices.Events;

namespace Minor.Miffy.MicroServices.Test.Component.EventListeners
{
    [EventListener("TestDependency")]
    public class MissingDependencyEventListener
    {
        /// <summary>
        /// Inject a random dependency that does not exist
        /// </summary>
        /// <param name="randomDependency"></param>
        public MissingDependencyEventListener(MethodEventListener randomDependency)
        {
            // This code should not be reached and throw an exception
        }

        /// <summary>
        /// Unreachable code because of missing dependency
        /// </summary>
        /// <param name="dummyEvent"></param>
        [Topic("test.topic")]
        public void Handle(DummyEvent dummyEvent)
        {
            // Neither should this code be reached
        }
    }
}