using Microsoft.AspNetCore.Identity;
using Notaion.Models;

namespace Notaion.Repositories
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> SignUpAsync(SignUpModel model);
        public Task<string> SignInAsync(SignInModel model);
    }
}
