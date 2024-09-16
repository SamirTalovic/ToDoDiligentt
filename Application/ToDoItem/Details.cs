using Application.Core;
using Domains;
using FluentValidation;
using MediatR;
using Persistance;

namespace Application.ToDoItem
{
    public class Details
    {
        public class Query : IRequest<Result<TodoItem>>
        {
            public int Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<TodoItem>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<TodoItem>> Handle(Query request, CancellationToken cancellationToken)
            {
                var todoItem = await _context.TodoItems.FindAsync(request.Id);

                if (todoItem == null) return Result<TodoItem>.Failure("Todo item not found");

                var dto = new TodoItem
                {
                    Id = todoItem.Id,
                    Title = todoItem.Title,
                    Description = todoItem.Description,
                    IsCompleted = todoItem.IsCompleted,
                    CreatedAt = todoItem.CreatedAt,
                    CompletedAt = todoItem.CompletedAt
                };

                return Result<TodoItem>.Success(dto);
            }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
            }
        }
    }
}
