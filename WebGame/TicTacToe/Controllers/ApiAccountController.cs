using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notaion.Repositories;
using Notaion.Models;
using Notaion.Context;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace WebAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository accountRepo;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountsController(IAccountRepository repo, UserManager<User> userManager, ApplicationDbContext context)
        {
            accountRepo = repo;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            var user = await _context.User
                .Where(u => u.Id == userId)
                .Select(u => new { u.Id, u.UserName, u.Avatar }) // Giả sử bạn có trường AvatarUrl
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
                    .Where(x=> x.UserName == identifier)
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

        [HttpPost("change-avatar/{id}/{avatar}")]
        public async Task<IActionResult> ChangeAvatar(string id, string avatar)
        {
            string decodedAvatarUrl = WebUtility.UrlDecode(avatar);
            var user = await _context.User.FindAsync(id);

            if (user != null)
            {
                user.Avatar = decodedAvatarUrl;
                _context.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Update successfully" });
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

            return Ok(new { token = result });
        }

    }
}