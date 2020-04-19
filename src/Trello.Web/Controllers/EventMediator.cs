using System;
using System.Threading.Tasks;
using Memstate;
using Memstate.Examples.Trello.Core;
using Microsoft.AspNetCore.SignalR;

namespace Trello.Web.Controllers
{
    public class EventMediator
    {
        private readonly Client<TrelloModel> _client;
        private readonly IHubContext<TrelloHub> _hubContext;

        public EventMediator(Client<TrelloModel> client, IHubContext<TrelloHub> hubContext)
        {
            _client = client;
            _hubContext = hubContext;
        }

        public async Task Configure()
        {
            Console.WriteLine("Connecting domain events to websockets");
            await _client.Subscribe<ColumnAdded>(HandleColumnAdded);
            await _client.Subscribe<CardAdded>(HandleCardAdded);
        }

        private void HandleColumnAdded(ColumnAdded columnAdded)
        {
            _hubContext
                .Clients
                .Group(columnAdded.BoardId)
                .SendAsync(nameof(ColumnAdded), columnAdded.Column);
        }

        private void HandleCardAdded(CardAdded cardAdded)
        {
            _hubContext
                .Clients
                .Group(cardAdded.BoardId)
                .SendAsync(nameof(CardAdded), cardAdded.Card);
        }
    }
}