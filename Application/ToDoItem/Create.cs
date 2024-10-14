using Application.Core;
using Domains;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Persistance;
using Rebus.Bus;
using ToDoDiligent.Services;

namespace Application.ToDoItem
{
    public class Create
    {
        public class CreateTodoItemCommand : IRequest<Result<int>>
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string AppUserId { get; set; }
        }

        public class Handler : IRequestHandler<CreateTodoItemCommand, Result<int>>
        {
            private readonly DataContext _context;
            private readonly IHubContext<TodoHub> _hubContext;
            private readonly TodoBackgroundJobService _backgroundJobService;
            private readonly IBackgroundJobClient _backgroundJobClient;
            private readonly IBus _bus;
            public Handler(DataContext context, IHubContext<TodoHub> hubContext, TodoBackgroundJobService backgroundJobService, IBackgroundJobClient backgroundJobClient, IBus bus)
            {
                _context = context;
                _hubContext = hubContext;
                _backgroundJobService = backgroundJobService;
                _backgroundJobClient = backgroundJobClient;
                _bus = bus;
            }

            public async Task<Result<int>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
            {
                if (_context == null)
                    throw new InvalidOperationException("DataContext is not initialized.");

                if (_hubContext == null)
                    throw new InvalidOperationException("HubContext is not initialized.");
                var todoItem = new TodoItem
                {
                    Title = request.Title,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    AppUserId = request.AppUserId

                };

                _context.TodoItems.Add(todoItem);

                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveTodoUpdate", todoItem.Id, todoItem.Title, todoItem.Description);

                    _backgroundJobClient.Enqueue(() => _backgroundJobService.CleanupOldTodoItems());
                    _backgroundJobClient.Schedule(() => _backgroundJobService.ScheduleJobs(),TimeSpan.FromSeconds(60));
                    var message = new MyMessage { Text = "Radiiii" };
                    await _bus.Send(message);
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
                RuleFor(x => x.AppUserId).NotEmpty();
            }
        }
    }
}
