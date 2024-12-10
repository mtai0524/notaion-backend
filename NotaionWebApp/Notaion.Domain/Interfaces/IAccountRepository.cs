using Microsoft.AspNetCore.Identity;
using Notaion.Domain.Models;
using Notaion.Models;

namespace Notaion.Domain.Interfaces
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> SignUpAsync(SignUpModel model);
        public Task<string> SignInAsync(SignInModel model);
        Task<IEnumerable<object>> GetAllUsersAsync();
    }
}
