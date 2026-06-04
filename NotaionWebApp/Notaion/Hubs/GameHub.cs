using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Notaion.Hubs
{
    // Co-op pixel platformer realtime hub.
    // Client-authoritative: each client simulates its own player and broadcasts
    // its state; the hub only relays to others in the same room. State lives in
    // memory (no DB) and is dropped when a connection leaves.
    public class GameHub : Hub
    {
        public class PlayerInfo
        {
            public string ConnectionId { get; set; }
            public string Room { get; set; }
            public string UserId { get; set; }
            public string Name { get; set; }
            public string Avatar { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Vx { get; set; }
            public int Facing { get; set; } = 1;
            public string Anim { get; set; } = "idle";
        }

        // connectionId -> snapshot
        private static readonly ConcurrentDictionary<string, PlayerInfo> Players = new();

        public async Task JoinGame(string room, string userId, string name, string avatar)
        {
            room = string.IsNullOrWhiteSpace(room) ? "world-1" : room;

            var info = new PlayerInfo
            {
                ConnectionId = Context.ConnectionId,
                Room = room,
                UserId = userId,
                Name = string.IsNullOrWhiteSpace(name) ? "Player" : name,
                Avatar = avatar
            };
            Players[Context.ConnectionId] = info;

            await Groups.AddToGroupAsync(Context.ConnectionId, room);

            // Newcomer needs everyone already here.
            var others = Players.Values
                .Where(p => p.Room == room && p.ConnectionId != Context.ConnectionId)
                .ToList();
            await Clients.Caller.SendAsync("ExistingPlayers", others);

            // Everyone here needs the newcomer.
            await Clients.OthersInGroup(room).SendAsync("PlayerJoined", info);
        }

        public async Task UpdateState(float x, float y, float vx, int facing, string anim)
        {
            if (!Players.TryGetValue(Context.ConnectionId, out var info))
                return;

            info.X = x;
            info.Y = y;
            info.Vx = vx;
            info.Facing = facing;
            info.Anim = anim;

            await Clients.OthersInGroup(info.Room)
                .SendAsync("PlayerState", Context.ConnectionId, x, y, vx, facing, anim);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Players.TryRemove(Context.ConnectionId, out var info))
            {
                await Clients.Group(info.Room).SendAsync("PlayerLeft", Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
