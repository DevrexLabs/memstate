namespace Memstate.Tcp
{
    internal class Pong : Response
    {
        public Pong(Ping ping) : base(ping.Id)
        {
        }
    }
}