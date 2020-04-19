using System;
using System.Threading.Tasks;
using Memstate;
using Memstate.Examples.Trello.Core;
using Microsoft.AspNetCore.Mvc;

namespace Trello.Web.Controllers
{
    public class TrelloController : Controller
    {
        private Client<TrelloModel> _memstateClient;

        public TrelloController(Client<TrelloModel> memstateClient, EventMediator _)
        {
            _memstateClient = memstateClient;
        }

        public async Task<IActionResult> Index()
        {
            var query = new GetBoardsQuery();
            var boards = await _memstateClient.Execute(query);
            return View(boards);
        }

        public async Task<IActionResult> Board(string id)
        {
            var query = new GetBoardQuery(id);
            var board = await _memstateClient.Execute(query);
            return View(board);
        }

        public async Task<JsonResult> Columns(string id)
        {
            var query = new GetColumnsQuery(id);
            var columns = await _memstateClient.Execute(query);
            return Json(columns);
        }

        public async Task<JsonResult> Cards(string id)
        {
            var query = new GetCardsQuery()
            {
                ColumnId = id
            };
            var cards = await _memstateClient.Execute(query);
            return Json(cards);
        }
        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            var command = new CreateBoardCommand(name);
            var id = await _memstateClient.Execute(command);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<JsonResult> Column(string id, string name)
        {
            var command = new AddColumnCommand(id, name);
            var columnId = await _memstateClient.Execute(command);
            return Json(new { columnId });
        }
    }
}