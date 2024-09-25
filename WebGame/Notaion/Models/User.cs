using Microsoft.AspNetCore.Identity;

namespace Notaion.Models
{
    public class User : IdentityUser
    {
        public string? Avatar { get; set; }
    }
}
