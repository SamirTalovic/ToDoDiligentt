using Application.ToDoItem;
using Domains;
using FluentAssertions;
using Moq;
using Persistance;
using Xunit;

namespace Tests
{
    public class DetailsTodoItemQueryTests
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly Details.Handler _handler;

        public DetailsTodoItemQueryTests()
        {
            _mockContext = new Mock<DataContext>();
            _handler = new Details.Handler(_mockContext.Object);
        }

        [Fact]
        public async Task ShouldReturnTodoItemSuccessfully()
        {
            var todoItem = new TodoItem { Id = 1, Title = "Test Todo", Description = "Test Description" };
            _mockContext.Setup(x => x.TodoItems.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync(todoItem);

            var query = new Details.Query { Id = 1 };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Title.Should().Be("Test Todo");
        }

        [Fact]
        public async Task ShouldFailWhenTodoItemNotFound()
        {
            _mockContext.Setup(x => x.TodoItems.FindAsync(It.IsAny<int>()))
                        .ReturnsAsync((TodoItem)null);

            var query = new Details.Query { Id = 1 };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Todo item not found");
        }
    }
}
