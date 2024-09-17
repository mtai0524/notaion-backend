using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Notaion.Context;
using Notaion.Models;

namespace Notaion.Services
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        public UserService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<string> GetUserNameByIdAsync(string userId)
        {
            var user = await _context.Users
                .Where(x => x.Id == userId)
                .Select(x => x.UserName)
                .FirstOrDefaultAsync();

            return user;
        }


        public async Task<User> GetCurrentLoggedInUser()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            return user;
        }
    }
}
