using Application.ToDoItem;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Core;

namespace ToDoDiligent.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoItemsController : BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> Create(Create.CreateTodoItemCommand command)
        {
            return HandleResult(await Mediator.Send(command));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return HandleResult(await Mediator.Send(new Details.Query { Id = id }));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Update.UpdateTodoItemCommand command)
        {
            command.Id = id;
            return HandleResult(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return HandleResult(await Mediator.Send(new Delete.Command { Id = id }));
        }
    }
}
