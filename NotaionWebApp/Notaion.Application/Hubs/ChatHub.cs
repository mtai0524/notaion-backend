using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Notaion.Application.Hubs
{
    public class ChatHub : Hub
    {
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

            var userList = OnlineUsers.Values.Select(u => new { u.UserId, u.UserName, u.Avatar }).ToList();

            await Clients.All.SendAsync("ReceiveOnlineUsers", userList);
        }



        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (OnlineUsers.TryRemove(Context.ConnectionId, out var user))
            {
                await Clients.All.SendAsync("UserDisconnected", user.UserId);
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

                await Clients.All.SendAsync("UserDisconnected", removedUser.UserId);
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
