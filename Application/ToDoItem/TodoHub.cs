using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Domains;
using Microsoft.EntityFrameworkCore;
using Application.ToDoItem;

namespace ToDoDiligent.Services
{
    [Authorize]
    public class TodoHub : Hub
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;

        public TodoHub(UserManager<AppUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [Authorize]
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        [Authorize]
        public async Task UpdateTodoItem(int id, string title, string description)
        {
            Console.WriteLine($"Todo Update: {id}, {title}, {description}");
            await Clients.All.SendAsync("ReceiveTodoUpdate", id, title, description);
        }
        /*
        [Authorize]
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var refreshToken = httpContext.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                Context.Abort();
                return;
            }

            var user = await _userManager.Users.Include(u => u.RefreshTokens)
                                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken && t.IsActive));

            if (user == null)
            {
                Context.Abort();
                return;
            }


            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
            });

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await base.OnConnectedAsync();*/
        }
    }

