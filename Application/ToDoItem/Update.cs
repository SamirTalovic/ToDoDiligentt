using Application.Core;
using FluentValidation;
using MediatR;
using Persistance;

namespace Application.ToDoItem
{
    public class Update
    {
        public class UpdateTodoItemCommand : IRequest<Result<Unit>>
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
        }

        public class Handler : IRequestHandler<UpdateTodoItemCommand, Result<Unit>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
            {
                var todoItem = await _context.TodoItems.FindAsync(request.Id);

                if (todoItem == null) return Result<Unit>.Failure("Todo item not found");

                todoItem.Title = request.Title ?? todoItem.Title;
                todoItem.Description = request.Description ?? todoItem.Description;

                var result = await _context.SaveChangesAsync() > 0;

                if (result) return Result<Unit>.Success(Unit.Value);

                return Result<Unit>.Failure("Failed to update todo item");
            }
        }

        public class Validator : AbstractValidator<UpdateTodoItemCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Id).GreaterThan(0);
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.Description).NotEmpty();
            }
        }
    }
}
