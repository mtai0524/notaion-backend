using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notaion.Context;
using Notaion.Domain.Entities;
using Notaion.Domain.Models;
using Notaion.Entities;
using Notaion.Models;

namespace Notaion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendShipController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly ApplicationDbContext _context;
        public FriendShipController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("accept-friend-request")]
        public async Task<IActionResult> AcceptFriendRequest([FromBody] AcceptFriendRequestModel model)
        {
            var existingFriendship = await _context.FriendShip
                .FirstOrDefaultAsync(f => f.SenderId == model.SenderId && f.ReceiverId == model.ReceiverId);

            if (existingFriendship != null && existingFriendship.Status == "Accepted")
            {
                return BadRequest("You are already friends.");
            }

            var friendship = new FriendShip
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = model.SenderId,
                ReceiverId = model.ReceiverId,
                Status = "Accepted",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FriendShip.Add(friendship);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Friend request accepted." });
        }

        [HttpGet("get-friends/{userId}")]
        public async Task<IActionResult> GetFriends(string userId)
        {
            var friends = await _context.FriendShip
                .Include(f => f.SenderUser) 
                .Include(f => f.ReceiverUser)
                .Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == "Accepted")
                .ToListAsync();

            var result = friends.Select(f => new
            {
                FriendshipId = f.Id,
                SenderId = f.SenderId,
                SenderUserName = f.SenderUser.UserName,
                SenderAvatar =f.SenderUser.Avatar,
                ReceiverId = f.ReceiverId,
                ReceiverUserName =f.ReceiverUser.UserName,
                ReceiverAvatar =  f.ReceiverUser.Avatar,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            });


            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFriend(string id)
        {
            var friendship = await _context.FriendShip.FindAsync(id);
            if (friendship == null)
            {
                return NotFound();
            }

            _context.FriendShip.Remove(friendship);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("check-friendship/{requesterId}/{recipientName}")]
        public async Task<IActionResult> CheckFriendship(string requesterId, string recipientName)
        {
            var recv = await _userManager.FindByNameAsync(recipientName);
            var friendship = await _context.FriendShip
                .FirstOrDefaultAsync(f =>
                    (f.SenderId == requesterId && f.ReceiverId == recv.Id) ||
                    (f.SenderId == recv.Id && f.ReceiverId == requesterId) &&
                    f.Status == "Accepted");

            if (friendship != null)
            {
                return Ok(new { isFriend = true });
            }

            return Ok(new { isFriend = false });
        }

        [HttpDelete]
        public async Task<IActionResult> DeteleAllListFriends()
        {
            var listFriendsAllUser = await _context.FriendShip.ToListAsync();

            _context.RemoveRange(listFriendsAllUser);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
