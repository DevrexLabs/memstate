using System;
using Memstate.Examples.TodoMvc.Domain;
using Memstate.Examples.TodoMvc.Domain.Lists;
using Microsoft.AspNetCore.Mvc;

namespace Memstate.Examples.TodoMvc.Controllers
{
    [Route("lists")]
    public class ListsController : Controller
    {
        private readonly Engine<TodoModel> _model;

        public ListsController(Engine<TodoModel> model)
        {
            _model = model;
        }
        
        [Route("", Name = "Lists.List")]
        [HttpGet]
        public IActionResult List()
        {
            var lists = _model.Execute(new FindAll()).Result;
            
            return Json(lists);
        }
        
        [Route("", Name = "Lists.Resolve")]
        [HttpPost]
        public IActionResult Create(string name)
        {
            var list = _model.Execute(new CreateList(name)).Result;
            
            return Created(Url.RouteUrl("Lists.Details", new { listId = list.Id }), list);
        }

        [Route("{listId}", Name = "Lists.Delete")]
        [HttpDelete]
        public IActionResult Delete(Guid listId)
        {
            _model.Execute(new RemoveList(listId)).Wait();
            return Ok();
        }

        [Route("{listId}/", Name = "Lists.Details")]
        [HttpGet]
        public IActionResult Details(Guid listId)
        {
            var list = _model.Execute(new FindOne(listId)).Result;
            return Json(list);
        }
    }
}