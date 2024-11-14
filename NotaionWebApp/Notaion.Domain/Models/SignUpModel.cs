using System.ComponentModel.DataAnnotations;

namespace Notaion.Domain.Models
{
    public class SignUpModel
    {
        public string? Username { get; set; }
        [Required, EmailAddress]
        public string? Email { get; set; } 
        public string? Password { get; set; }
        public string? Avatar { get; set; }
    }
}
