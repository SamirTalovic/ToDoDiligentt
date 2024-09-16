using Application.ToDoItem;
using Domains;
using FluentAssertions;
using Moq;
using Persistance;
using ToDoDiligent.Services;
using Xunit;

namespace Tests
{
    public class DeleteTodoItemCommandTests
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly Delete.Handler _handler;

        public DeleteTodoItemCommandTests()
        {
            _mockContext = new Mock<DataContext>();
                
            _handler = new Delete.Handler(_mockContext.Object);
        }

        [Fact]
        public async Task ShouldDeleteTodoItemSuccessfully()
        {
            var todoItem = new TodoItem { Id = 1 };
            _mockContext.Setup(x => x.TodoItems.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync(todoItem);
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            var command = new Delete.Command { Id = 1 };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldFailWhenTodoItemNotFound()
        {
            _mockContext.Setup(x => x.TodoItems.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync((TodoItem)null);

            var command = new Delete.Command { Id = 1 };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Todo item not found");
        }
    }
}
