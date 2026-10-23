using System.ComponentModel.DataAnnotations;

namespace Notaion.Models
{
    public class SetPasswordModel
    {
        // Empty for an account that has no password yet (Discord/GitHub sign-in).
        public string? CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
