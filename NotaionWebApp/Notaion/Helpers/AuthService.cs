using Microsoft.AspNetCore.SignalR;
using Notaion.Hubs;

namespace Notaion.Helpers
{
    public class AuthService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public AuthService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task OnUserLoggedIn(string userName)
        {
            // Notify all clients that a user has logged in
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"{userName} has logged in.");
        }
    }

}
