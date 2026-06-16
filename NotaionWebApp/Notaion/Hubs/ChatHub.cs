using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Notaion.Infrastructure.Context;
using Notaion.Models;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Notaion.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        private static DateTime VietnamNow()
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch (TimeZoneNotFoundException)
            {
                // Linux without the Windows TZ alias — fall back to a fixed +7 offset.
                return DateTime.UtcNow.AddHours(7);
            }
        }

        // Persist the user's last-seen time and return the stamp so callers can
        // broadcast it to other clients. Errors are logged (not swallowed) so a
        // bad write is visible instead of silently leaving LastSeen null.
        private async Task<DateTime> TouchLastSeenAsync(string userId)
        {
            var now = VietnamNow();
            if (string.IsNullOrEmpty(userId)) return now;

            try
            {
                var rows = await _context.User
                    .Where(u => u.Id == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastSeen, now));
                Console.WriteLine($"[ChatHub] LastSeen updated for {userId}: {now:O} (rows={rows})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] LastSeen update FAILED for {userId}: {ex.Message}");
            }
            return now;
        }

        //public async Task SendFriendRequest(string receiverConnectionId, string senderId, string receiverId, string senderName)
        //{
        //    await Clients.Client(receiverConnectionId).SendAsync("ReceiveFriendRequest", senderId, receiverId, senderName);
        //}

        public async Task SendNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMessagePrivate(string sender, string reveiver, string message, string currentUsername, string friendUsername)
        {
            await Clients.All.SendAsync("ReceiveMessagePrivate", sender, reveiver, message, currentUsername, friendUsername);
        }

        //console

        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has left the group {groupName}.");
        }
        public async Task CreateGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has created and joined the group {groupName}.");
        }


        private static ConcurrentDictionary<string, (string UserId, string UserName, string Avatar)> OnlineUsers = new ConcurrentDictionary<string, (string UserId, string UserName, string Avatar)>();



        public async Task RegisterUser(RegisterUserModel user) // react call RegisterUser with invoke
        {
            OnlineUsers.TryAdd(Context.ConnectionId, (user.UserId, user.UserName, user.Avatar));

            // Stamp LastSeen on connect as well, so the column always has a value
            // even if the disconnect event is never delivered (tab crash, server
            // restart, ungraceful drop).
            await TouchLastSeenAsync(user.UserId);

            var userList = OnlineUsers.Values.Select(u => new { u.UserId, u.UserName, u.Avatar }).ToList();

            await Clients.All.SendAsync("ReceiveOnlineUsers", userList);
        }



        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (OnlineUsers.TryRemove(Context.ConnectionId, out var user))
            {
                // Only flip to "offline" once the user has no other live connections
                // (e.g. multiple tabs) — otherwise last-seen would be wrong.
                var stillOnline = OnlineUsers.Values.Any(u => u.UserId == user.UserId);
                if (!stillOnline)
                {
                    var lastSeen = await TouchLastSeenAsync(user.UserId);
                    await Clients.All.SendAsync("UserDisconnected", user.UserId, lastSeen);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task LogoutUser(string userId) // react call LogoutUser with invoke
        {
            // find user by userId
            var userConnectionId = OnlineUsers.FirstOrDefault(x => x.Value.UserId == userId).Key;

            if (userConnectionId != null)
            {
                OnlineUsers.TryRemove(userConnectionId, out var removedUser);

                var stillOnline = OnlineUsers.Values.Any(u => u.UserId == removedUser.UserId);
                if (!stillOnline)
                {
                    var lastSeen = await TouchLastSeenAsync(removedUser.UserId);
                    await Clients.All.SendAsync("UserDisconnected", removedUser.UserId, lastSeen);
                }
            }
        }

    }
    public class RegisterUserModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
    }
}
