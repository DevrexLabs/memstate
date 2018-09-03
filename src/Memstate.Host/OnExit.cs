using System;

namespace Memstate.Host
{
    public static class OnExit
    {
        public static void Register(Action exitAction)
        {
            //CTRL+C, CTRL+Break and SIGINT
            Console.CancelKeyPress += delegate { exitAction.Invoke(); };

            //SIGTERM
            AppDomain.CurrentDomain.ProcessExit += delegate { exitAction.Invoke(); };
        }
    }
}