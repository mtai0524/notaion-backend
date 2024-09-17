using Microsoft.AspNetCore.SignalR;
using Notaion.Hubs;

namespace Notaion.Helpers
{
    public class LoginHandler
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public LoginHandler(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task OnUserLoggedIn(string userName)
        {
            await _hubContext.Clients.All.SendAsync("UserLoggedIn", userName);
        }
    }
}
