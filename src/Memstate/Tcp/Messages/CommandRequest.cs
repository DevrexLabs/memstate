namespace Memstate.Tcp
{
    internal class CommandRequest : Request
    {
        public CommandRequest(Command command)
        {
            Command = command;
        }

        public Command Command { get; set; }
    }
}