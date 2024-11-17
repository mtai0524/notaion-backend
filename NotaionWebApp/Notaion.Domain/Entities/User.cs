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
    }
}
