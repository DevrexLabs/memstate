using System;
using System.Threading.Tasks;

namespace Memstate.Runner.Commands
{
    public interface ICommand
    {
        event EventHandler Done;
        
        Task Start(string[] arguments);

        Task Stop();
    }
}