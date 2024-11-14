using Microsoft.AspNetCore.Identity;

namespace Notaion.Domain.Models
{
    public class User : IdentityUser
    {
        public string? Avatar { get; set; }
    }
}
