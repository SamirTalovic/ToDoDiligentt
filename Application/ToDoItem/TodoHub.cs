using Microsoft.AspNetCore.SignalR;

namespace ToDoDiligent.Services
{
    public class TodoHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task UpdateTodoItem(int id, string title, string description)
        {
            await Clients.All.SendAsync("ReceiveTodoUpdate", id, title, description);
        }
    }
}
