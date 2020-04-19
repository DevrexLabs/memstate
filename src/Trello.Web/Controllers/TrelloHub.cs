using System;
using System.Threading.Tasks;
using Memstate;
using Memstate.Examples.Trello.Core;
using Microsoft.AspNetCore.SignalR;

namespace Trello.Web.Controllers
{
    public class TrelloHub : Hub
    {
        private readonly Client<TrelloModel> _memstateClient;

        public TrelloHub(Client<TrelloModel> memstateClient)
        {
            _memstateClient = memstateClient;
        }

        public Task Subscribe(string boardId)
        {
            Console.WriteLine("Hub -> Subsribe");
            return Groups.AddToGroupAsync(Context.ConnectionId, boardId);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Hub -> OnConnected");
            return base.OnConnectedAsync();
        }
    }
}