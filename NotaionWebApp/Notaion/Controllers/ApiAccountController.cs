using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
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
            var clientId = _configuration["Authentication:Discord:AppId"];
            if (string.IsNullOrEmpty(clientId) || clientId == "YOUR_DISCORD_CLIENT_ID")
            {
                return BadRequest("Discord Authentication is not configured on the server.");
            }

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Discord", Url.Action("DiscordCallback"));
            return Challenge(properties, "Discord");
        }

        [HttpGet("discord-callback")]
        public async Task<IActionResult> DiscordCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // Ghi log lỗi để bạn kiểm tra trong console của server
                Console.WriteLine("Discord login failed: info is null. Check ClientId/Secret and Redirect URIs.");
                return Redirect($"{_configuration["FrontendUrl"] ?? "https://notaion.onrender.com"}/login?error=discord_info_null");
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

            // Lấy FrontendUrl từ cấu hình
            var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');
            
            // Nếu không có cấu hình hoặc vẫn là placeholder, dùng fallback an toàn
            if (string.IsNullOrEmpty(frontendUrl) || frontendUrl.Contains("your-frontend-domain"))
            {
                 frontendUrl = "https://notaion.onrender.com"; 
            }

            return Redirect($"{frontendUrl}/login-success?token={token}");
        }

        [HttpGet("github-login")]
        public IActionResult GitHubLogin()
        {
            var clientId = _configuration["Authentication:GitHub:AppId"];
            if (string.IsNullOrEmpty(clientId) || clientId == "YOUR_GITHUB_CLIENT_ID")
            {
                return BadRequest("GitHub Authentication is not configured on the server.");
            }

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("GitHub", Url.Action("GitHubCallback"));
            return Challenge(properties, "GitHub");
        }

        [HttpGet("github-callback")]
        public async Task<IActionResult> GitHubCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                Console.WriteLine("GitHub login failed: info is null. Check ClientId/Secret and Redirect URIs.");
                return Redirect($"{_configuration["FrontendUrl"] ?? "https://notaion.onrender.com"}/login?error=github_info_null");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            User user = null;
            // Lấy thông tin từ GitHub
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "GitHubUser";
            var githubId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            // GitHub avatar dựng từ numeric user id (URL chính tắc, không cần claim mapping).
            var avatarUrl = string.IsNullOrEmpty(githubId)
                ? "https://avatars.githubusercontent.com/u/0"
                : $"https://avatars.githubusercontent.com/u/{githubId}?v=4";

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
                // Nếu không có email (email private trên GitHub), tạo email giả
                if (string.IsNullOrEmpty(email)) email = $"{githubId}@github.com";

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
                        user.UserName = $"{name}_{githubId.Substring(0, Math.Min(4, githubId.Length))}";
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

            // Lấy FrontendUrl từ cấu hình
            var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');

            if (string.IsNullOrEmpty(frontendUrl) || frontendUrl.Contains("your-frontend-domain"))
            {
                frontendUrl = "https://notaion.onrender.com";
            }

            return Redirect($"{frontendUrl}/login-success?token={token}");
        }

        // ============================================================
        // ACCOUNT LINKING — link/unlink external providers (Discord, GitHub, ...)
        // to the CURRENT account, and list what's linked.
        // Uses ASP.NET Identity's AspNetUserLogins table (no new schema).
        // ============================================================

        // The link flow is a top-level browser redirect, so the JWT can't ride
        // in an Authorization header (cross-origin). It is passed as ?token and
        // validated here, then the userId is carried through the OAuth round
        // trip via AuthenticationProperties.
        private string ValidateTokenGetUserId(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["JWT:ValidIssuer"],
                    ValidAudience = _configuration["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
                };
                var principal = handler.ValidateToken(token, parameters, out _);
                return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch
            {
                return null;
            }
        }

        // Where to send the browser back to after the linking round trip.
        private string ResolveLinkReturnUrl(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl)) return returnUrl.TrimEnd('/');
            var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(frontendUrl) || frontendUrl.Contains("your-frontend-domain"))
                frontendUrl = "https://notaion.onrender.com";
            return $"{frontendUrl}/setting";
        }

        [HttpGet("discord-link")]
        public IActionResult DiscordLink([FromQuery] string token, [FromQuery] string returnUrl = null)
        {
            var clientId = _configuration["Authentication:Discord:AppId"];
            if (string.IsNullOrEmpty(clientId) || clientId == "YOUR_DISCORD_CLIENT_ID")
                return BadRequest("Discord Authentication is not configured on the server.");

            var userId = ValidateTokenGetUserId(token);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or expired token.");

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Discord", Url.Action("DiscordLinkCallback"));
            properties.Items["LinkUserId"] = userId;
            properties.Items["LinkReturnUrl"] = ResolveLinkReturnUrl(returnUrl);
            return Challenge(properties, "Discord");
        }

        [HttpGet("discord-link-callback")]
        public async Task<IActionResult> DiscordLinkCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            var returnUrl = "https://notaion.onrender.com/setting";
            if (info?.AuthenticationProperties != null &&
                info.AuthenticationProperties.Items.TryGetValue("LinkReturnUrl", out var ru) &&
                !string.IsNullOrEmpty(ru))
            {
                returnUrl = ru;
            }

            if (info == null)
                return Redirect($"{returnUrl}?link=error");

            info.AuthenticationProperties.Items.TryGetValue("LinkUserId", out var userId);
            if (string.IsNullOrEmpty(userId))
                return Redirect($"{returnUrl}?link=error");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Redirect($"{returnUrl}?link=error");

            // If this external login is already attached to someone, don't steal it.
            var existing = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existing != null)
            {
                return existing.Id == user.Id
                    ? Redirect($"{returnUrl}?link=already&provider=Discord")
                    : Redirect($"{returnUrl}?link=conflict&provider=Discord");
            }

            var addResult = await _userManager.AddLoginAsync(user, info);
            if (!addResult.Succeeded)
                return Redirect($"{returnUrl}?link=error");

            // Adopt the Discord avatar only if the account has none yet.
            var avatarUrl = info.Principal.FindFirstValue("urn:discord:avatar:url");
            if (!string.IsNullOrEmpty(avatarUrl) && string.IsNullOrEmpty(user.Avatar))
            {
                user.Avatar = avatarUrl;
                await _userManager.UpdateAsync(user);
            }

            return Redirect($"{returnUrl}?link=success&provider=Discord");
        }

        [HttpGet("github-link")]
        public IActionResult GitHubLink([FromQuery] string token, [FromQuery] string returnUrl = null)
        {
            var clientId = _configuration["Authentication:GitHub:AppId"];
            if (string.IsNullOrEmpty(clientId) || clientId == "YOUR_GITHUB_CLIENT_ID")
                return BadRequest("GitHub Authentication is not configured on the server.");

            var userId = ValidateTokenGetUserId(token);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid or expired token.");

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("GitHub", Url.Action("GitHubLinkCallback"));
            properties.Items["LinkUserId"] = userId;
            properties.Items["LinkReturnUrl"] = ResolveLinkReturnUrl(returnUrl);
            return Challenge(properties, "GitHub");
        }

        [HttpGet("github-link-callback")]
        public async Task<IActionResult> GitHubLinkCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            var returnUrl = "https://notaion.onrender.com/setting";
            if (info?.AuthenticationProperties != null &&
                info.AuthenticationProperties.Items.TryGetValue("LinkReturnUrl", out var ru) &&
                !string.IsNullOrEmpty(ru))
            {
                returnUrl = ru;
            }

            if (info == null)
                return Redirect($"{returnUrl}?link=error");

            info.AuthenticationProperties.Items.TryGetValue("LinkUserId", out var userId);
            if (string.IsNullOrEmpty(userId))
                return Redirect($"{returnUrl}?link=error");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Redirect($"{returnUrl}?link=error");

            // If this external login is already attached to someone, don't steal it.
            var existing = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existing != null)
            {
                return existing.Id == user.Id
                    ? Redirect($"{returnUrl}?link=already&provider=GitHub")
                    : Redirect($"{returnUrl}?link=conflict&provider=GitHub");
            }

            var addResult = await _userManager.AddLoginAsync(user, info);
            if (!addResult.Succeeded)
                return Redirect($"{returnUrl}?link=error");

            // Adopt the GitHub avatar only if the account has none yet.
            var githubId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var avatarUrl = string.IsNullOrEmpty(githubId)
                ? null
                : $"https://avatars.githubusercontent.com/u/{githubId}?v=4";
            if (!string.IsNullOrEmpty(avatarUrl) && string.IsNullOrEmpty(user.Avatar))
            {
                user.Avatar = avatarUrl;
                await _userManager.UpdateAsync(user);
            }

            return Redirect($"{returnUrl}?link=success&provider=GitHub");
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("linked-providers")]
        public async Task<IActionResult> LinkedProviders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userId == null ? null : await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var logins = await _userManager.GetLoginsAsync(user);
            var hasPassword = await _userManager.HasPasswordAsync(user);

            var providers = logins
                .Select(l => new { provider = l.LoginProvider, displayName = l.ProviderDisplayName ?? l.LoginProvider })
                .ToList();

            return Ok(new { hasPassword, providers });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("unlink-provider/{provider}")]
        public async Task<IActionResult> UnlinkProvider(string provider)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userId == null ? null : await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var logins = await _userManager.GetLoginsAsync(user);
            var hasPassword = await _userManager.HasPasswordAsync(user);

            // Never remove the user's only way to sign in.
            if (!hasPassword && logins.Count <= 1)
                return BadRequest(new { message = "Đây là phương thức đăng nhập duy nhất của bạn. Hãy đặt mật khẩu trước khi gỡ liên kết." });

            var login = logins.FirstOrDefault(l => string.Equals(l.LoginProvider, provider, StringComparison.OrdinalIgnoreCase));
            if (login == null) return NotFound(new { message = "Provider chưa được liên kết." });

            var result = await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
            if (!result.Succeeded) return BadRequest(new { message = "Gỡ liên kết thất bại." });

            return Ok(new { message = "Đã gỡ liên kết", provider = login.LoginProvider });
        }
    }
}
