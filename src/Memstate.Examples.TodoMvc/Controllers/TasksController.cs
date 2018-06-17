using System;
using Memstate.Examples.TodoMvc.Domain;
using Memstate.Examples.TodoMvc.Domain.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Memstate.Examples.TodoMvc.Controllers
{
    [Route("lists/{listId}/tasks")]
    public class TasksController : Controller
    {
        private readonly Engine<TodoModel> _model;

        public TasksController(Engine<TodoModel> model)
        {
            _model = model;
        }
        
        [Route("", Name = "Tasks.Create")]
        [HttpPost]
        public IActionResult Create(Guid listId, string title, string description, DateTime? dueBy)
        {
            var task = _model.Execute(new AddTask(listId, title, description, dueBy)).Result;

            return Json(task);
        }

        [Route("{taskId}", Name = "Tasks.Details")]
        public IActionResult Details(Guid listId, Guid taskId)
        {
            var task = _model.Execute(new FindOne(listId, taskId)).Result;

            if (task == null)
            {
                return NotFound();
            }

            return Json(task);
        }

        [Route("{taskId}", Name = "Tasks.Delete")]
        [HttpDelete]
        public IActionResult Delete(Guid listId, Guid taskId)
        {
            return new NoContentResult();
        }
    }
}