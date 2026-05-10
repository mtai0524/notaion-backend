using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notaion.Models;
using Notaion.Infrastructure.Context;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Notaion.Hubs;
using Notaion.Domain.Entities;
using Notaion.Domain.Models;
using Notaion.Domain.Interfaces;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository accountRepo;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountsController(IAccountRepository repo, UserManager<User> userManager, ApplicationDbContext context, IHubContext<ChatHub> hubContext, SignInManager<User> signInManager, IConfiguration configuration)
        {
            accountRepo = repo;
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet("get-users-demo-jenkins-ngrok-github-webhook-hehe")]
        public async Task<IActionResult> GetAllUser()
        {
            var user = await _context.User.
            Select(u => new
            {
                u.Id,
                u.UserName,
            })
            .ToListAsync();
            return Ok(user);
        }

        //[HttpDelete("delete-user/{id}")]
        //public async Task<IActionResult> DeleteUserById(string id)
        //{
        //    var user = await _context.User.FirstOrDefaultAsync(x => x.Id == id);
        //    _context.Remove(user);
        //    await _context.SaveChangesAsync();
        //    return Ok(user);
        //}

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            var user = await _context.User
                .Where(u => u.Id == userId)
                .Select(u => new { u.Id, u.UserName, u.Avatar })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }


        [HttpGet("profile/{identifier}")]
        public async Task<IActionResult> Profile(string identifier)
        {
            var isUUID = Guid.TryParse(identifier, out _);

            User user;

            if (isUUID)
            {
                // find user by Id
                user = await _context.User.FindAsync(identifier);
            }
            else
            {
                // find user by username
                user = await _context.User
                    .Where(x => x.UserName == identifier)
                    .FirstOrDefaultAsync();
            }

            if (user == null)
            {
                return NotFound();
            }

            var userProfile = new UserProfile
            {
                UserName = user.UserName,
                Email = user.Email,
                Avatar = user.Avatar,
            };

            return Ok(userProfile);
        }

        [HttpPut("change-avatar/{id}/{avatar}")]
        public async Task<IActionResult> ChangeAvatar(string id, string avatar)
        {
            string decodedAvatarUrl = WebUtility.UrlDecode(avatar);
            var user = await _context.User.FindAsync(id);

            if (user != null)
            {
                user.Avatar = decodedAvatarUrl;
                _context.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Update successfully", avatar = user.Avatar });
            }

            return Ok(new { message = "Failed" });
        }


        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(SignUpModel signUpModel)
        {
            var result = await accountRepo.SignUpAsync(signUpModel);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors });
            }

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            var result = await accountRepo.SignInAsync(signInModel);

            if (result.Contains("UserNotFound"))
            {
                return Unauthorized(new { message = "User not found" });
            }

            if (result.Contains("InvalidPassword"))
            {
                return Unauthorized(new { message = "Incorrect password" });
            }

            User user = null;
            if (signInModel.Email.Contains("@"))
            {
                // find user by email
                user = await _userManager.FindByEmailAsync(signInModel.Email);
            }
            else
            {
                // find user by username
                user = await _userManager.FindByNameAsync(signInModel.Email);
            }

            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Create object with userId and userName
            var userInfo = new
            {
                userId = user.Id,
                userName = user.UserName,
                avatar = user.Avatar
            };

            // Send object to all clients when login
            await _hubContext.Clients.All.SendAsync("ReceiveOnlineUsers", new[] { userInfo });

            return Ok(new { token = result });
        }

        [HttpGet("discord-login")]
        public IActionResult DiscordLogin()
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Discord", Url.Action("DiscordCallback"));
            return Challenge(properties, "Discord");
        }

        [HttpGet("discord-callback")]
        public async Task<IActionResult> DiscordCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Redirect($"{_configuration["FrontendUrl"] ?? "http://localhost:2405"}/login?error=discord_failed");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            User user = null;
            // Lấy thông tin từ Discord
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? info.Principal.FindFirstValue("urn:discord:username") ?? "DiscordUser";
            var discordId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var avatarHash = info.Principal.FindFirstValue("urn:discord:avatar:hash");
            var avatarUrl = info.Principal.FindFirstValue("urn:discord:avatar:url");

            if (string.IsNullOrEmpty(avatarUrl) && !string.IsNullOrEmpty(avatarHash))
            {
                avatarUrl = $"https://cdn.discordapp.com/avatars/{discordId}/{avatarHash}.png";
            }
            if (string.IsNullOrEmpty(avatarUrl))
            {
                avatarUrl = "https://cdn.discordapp.com/embed/avatars/0.png";
            }

            if (result.Succeeded)
            {
                user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                // Cập nhật Avatar nếu có thay đổi
                if (user != null && user.Avatar != avatarUrl)
                {
                    user.Avatar = avatarUrl;
                    await _userManager.UpdateAsync(user);
                }
            }
            else
            {
                // Nếu không có email, tạo email giả
                if (string.IsNullOrEmpty(email)) email = $"{discordId}@discord.com";
                
                user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = name,
                        Email = email,
                        EmailConfirmed = true,
                        Avatar = avatarUrl
                    };
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                         user.UserName = $"{name}_{discordId.Substring(0, 4)}";
                         await _userManager.CreateAsync(user);
                    }
                }
                await _userManager.AddLoginAsync(user, info);
                
                // Đảm bảo avatar được lưu đúng
                user.Avatar = avatarUrl;
                await _userManager.UpdateAsync(user);
            }

            var token = accountRepo.GenerateJwtToken(user);
            
            // Thông báo User online qua SignalR
            var userInfo = new { userId = user.Id, userName = user.UserName, avatar = user.Avatar };
            await _hubContext.Clients.All.SendAsync("ReceiveOnlineUsers", new[] { userInfo });

            // Lấy FrontendUrl từ cấu hình hoặc mặc định là localhost
            var frontendUrl = _configuration["FrontendUrl"];
            
            // Nếu không có cấu hình, ta thử lấy từ Origin của Request (dành cho bản deploy)
            if (string.IsNullOrEmpty(frontendUrl) || frontendUrl.Contains("your-frontend-domain"))
            {
                 var origin = Request.Headers["Referer"].ToString();
                 if (!string.IsNullOrEmpty(origin))
                 {
                     frontendUrl = new Uri(origin).GetLeftPart(UriPartial.Authority);
                 }
                 else
                 {
                     frontendUrl = "http://localhost:2405"; // Fallback cuối cùng
                 }
            }

            return Redirect($"{frontendUrl}/login-success?token={token}");
        }


    }
}
