using System.ComponentModel.DataAnnotations;

namespace Notaion.Models
{
    public class SignUpModel
    {
        [Required]
        public string? Username { get; set; }
        [Required, EmailAddress]
        public string? Email { get; set; } 
        [Required]
        public string? Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
        [Required]
        public string? Avatar { get; set; }
    }
}
