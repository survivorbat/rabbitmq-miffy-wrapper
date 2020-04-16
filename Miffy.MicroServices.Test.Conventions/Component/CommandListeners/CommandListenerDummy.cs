using System.Collections.Generic;
using Miffy.MicroServices.Commands;
using Miffy.MicroServices.Events;
using Miffy.MicroServices.Test.Conventions.Component.Commands;
using Miffy.MicroServices.Test.Conventions.Component.Models;

namespace Miffy.MicroServices.Test.Conventions.Component.CommandListeners
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
