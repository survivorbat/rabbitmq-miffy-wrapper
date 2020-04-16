namespace Miffy.MicroServices.Commands
{
    public class MicroserviceCommandListener
    {
        public string Queue { get; set; }
        public CommandReceivedCallback Callback { get; set; }
    }
}
