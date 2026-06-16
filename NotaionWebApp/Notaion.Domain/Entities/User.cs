using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Domain.Entities
{
    public class User : IdentityUser
    {
        public string? Avatar { get; set; }

        // Last time the user was seen online (set on SignalR disconnect).
        // Lets the UI show "Hoạt động X phút trước" for offline users.
        public DateTime? LastSeen { get; set; }
    }
}
