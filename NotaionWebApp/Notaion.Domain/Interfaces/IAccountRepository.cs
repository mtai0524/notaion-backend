using Microsoft.AspNetCore.Identity;
using Notaion.Domain.Models;
using Notaion.Models;

namespace Notaion.Domain.Interfaces
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> SignUpAsync(SignUpModel model);
        public Task<string> SignInAsync(SignInModel model);
        public string GenerateJwtToken(Notaion.Domain.Entities.User user);
        Task<IEnumerable<object>> GetAllUsersAsync();
    }
}
