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

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp(SignUpModel signUpModel)
        {
            var result = await accountRepo.SignUpAsync(signUpModel);
            if (result.Succeeded)
            {
                return Ok(result.Succeeded);
            }

            return Unauthorized();
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            var result = await accountRepo.SignInAsync(signInModel);

            if (result.Contains("UserNotFound"))
            {
                return Unauthorized(new { message = "User not found. Please check your username or email." });
            }

            if (result.Contains("InvalidPassword"))
            {
                return Unauthorized(new { message = "Incorrect password. Please try again." });
            }

            return Ok(new { token = result });
        }

    }
}