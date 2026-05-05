using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Notaion.Hubs
{
    public class DailyNoteHub : Hub
    {
        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task LeaveGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
    }
}
