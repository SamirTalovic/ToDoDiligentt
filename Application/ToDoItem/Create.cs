using Application.Core;
using Domains;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Persistance;
using ToDoDiligent.Services;

namespace Application.ToDoItem
{
    public class Create
    {
        public class CreateTodoItemCommand : IRequest<Result<int>>
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }

        public class Handler : IRequestHandler<CreateTodoItemCommand, Result<int>>
        {
            private readonly DataContext _context;
            private readonly IHubContext<TodoHub> _hubContext;
            private readonly TodoBackgroundJobService _backgroundJobService;
            public Handler(DataContext context, IHubContext<TodoHub> hubContext, TodoBackgroundJobService backgroundJobService)
            {
                _context = context;
                _hubContext = hubContext;
                _backgroundJobService = backgroundJobService;
            }

            public async Task<Result<int>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
            {
                var todoItem = new TodoItem
                {
                    Title = request.Title,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TodoItems.Add(todoItem);

                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveTodoUpdate", todoItem.Id, todoItem.Title, todoItem.Description);

                    BackgroundJob.Enqueue(() => _backgroundJobService.CleanupOldTodoItems());

                    return Result<int>.Success(todoItem.Id);
                }

                return Result<int>.Failure("Failed to create todo item");
            }
        }

        public class Validator : AbstractValidator<CreateTodoItemCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.Description).NotEmpty();
            }
        }
    }
}
