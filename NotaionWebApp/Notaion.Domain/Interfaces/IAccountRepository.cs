using Microsoft.AspNetCore.Identity;
using Notaion.Domain.Models;
using Notaion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Domain.Interfaces
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> SignUpAsync(SignUpModel model);
        public Task<string> SignInAsync(SignInModel model);
        Task<IEnumerable<object>> GetAllUsersAsync();
    }
}
