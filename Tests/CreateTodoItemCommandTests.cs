using Application.ToDoItem;
using Domains;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Persistance;
using ToDoDiligent.Services;
using Xunit;

namespace Tests
{
    public class CreateTodoItemCommandTests
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly Mock<IHubContext<TodoHub>> _mockHubContext;
        private readonly Mock<TodoBackgroundJobService> _mockBackgroundJobService;
        private readonly Create.Handler _handler;

        public CreateTodoItemCommandTests()
        {
            _mockContext = new Mock<DataContext>();
            _mockHubContext = new Mock<IHubContext<TodoHub>>();
            _mockBackgroundJobService = new Mock<TodoBackgroundJobService>();  

            _handler = new Create.Handler(_mockContext.Object, _mockHubContext.Object, _mockBackgroundJobService.Object);
        }

        [Fact]
        public async Task ShouldCreateTodoItemSuccessfully()
        {
            var command = new Create.CreateTodoItemCommand
            {
                Title = "Test Todo",
                Description = "Test Description"
            };

            _mockContext.Setup(x => x.TodoItems.Add(It.IsAny<TodoItem>()));
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeGreaterThan(0);
        }
    }
}
