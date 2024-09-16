using Application.ToDoItem;
using Domains;
using FluentAssertions;
using Moq;
using Persistance;
using ToDoDiligent.Services;
using Xunit;

namespace Tests
{
    public class UpdateTodoItemCommandTests
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly Mock<TodoBackgroundJobService> _mockBackgroundJobService;
        private readonly Update.Handler _handler;

        public UpdateTodoItemCommandTests()
        {
            _mockContext = new Mock<DataContext>();

            _handler = new Update.Handler(_mockContext.Object);
        }

        [Fact]
        public async Task ShouldUpdateTodoItemSuccessfully()
        {
            var todoItem = new TodoItem { Id = 1, Title = "Old Title", Description = "Old Description" };
            _mockContext.Setup(x => x.TodoItems.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync(todoItem);
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            var command = new Update.UpdateTodoItemCommand
            {
                Id = 1,
                Title = "New Title",
                Description = "New Description"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldFailWhenTodoItemNotFound()
        {
            _mockContext.Setup(x => x.TodoItems.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync((TodoItem)null);

            var command = new Update.UpdateTodoItemCommand
            {
                Id = 1,
                Title = "New Title",
                Description = "New Description"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Todo item not found");
        }
    }
}
