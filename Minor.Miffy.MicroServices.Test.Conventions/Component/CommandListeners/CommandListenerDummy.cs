using System.Collections.Generic;
using Minor.Miffy.MicroServices.Events;
using Minor.Miffy.MicroServices.Test.Conventions.Component.Commands;
using Minor.Miffy.MicroServices.Test.Conventions.Component.Models;

namespace Minor.Miffy.MicroServices.Test.Conventions.Component.CommandListeners
{
    public class CommandListenerDummy
    {
        [CommandListener("command.test")]
        public IEnumerable<Animal> GetAnimals(GetAnimalsCommand command)
        {
            return new []
            {
                new Animal
                {
                    Name = "Jeffrey",
                    Species = "Cat"
                },
                new Animal
                {
                    Name = "Jonas",
                    Species = "Elephant"
                }
            };
        }
    }
}
