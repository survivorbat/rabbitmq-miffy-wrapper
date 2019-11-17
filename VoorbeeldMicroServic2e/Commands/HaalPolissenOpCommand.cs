using System.Collections.Generic;
using Minor.Miffy.MicroServices.Commands;
using VoorbeeldMicroService.Models;

namespace VoorbeeldMicroService.Commands
{
    public class HaalPolissenOpCommand : DomainCommand
    {
        public IEnumerable<Polis> Polisses = new List<Polis>();

        public HaalPolissenOpCommand() : base("MVM.TestService.HaalPolissenOpQueue") { }
    }
}