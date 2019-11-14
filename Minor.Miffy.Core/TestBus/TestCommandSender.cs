using System.Threading.Tasks;

namespace Minor.Miffy.TestBus
{
    public class TestCommandSender : ICommandSender
    {
        private readonly TestBusContext _context;
        
        public TestCommandSender(TestBusContext context) => _context = context;

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public Task<CommandMessage> SendCommandAsync(CommandMessage request)
        {
            throw new System.NotImplementedException();
        }
    }
}