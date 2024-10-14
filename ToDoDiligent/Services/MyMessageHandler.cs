namespace ToDoDiligent.Services
{
    using Rebus.Handlers;
    using System.Threading.Tasks;

    public class MyMessage
    {
        public string Text { get; set; }
    }

    public class MyMessageHandler : IHandleMessages<MyMessage>
    {
        public Task Handle(MyMessage message)
        {
            Console.WriteLine($"Received message: {message.Text}");
            return Task.CompletedTask;
        }
    }
}
