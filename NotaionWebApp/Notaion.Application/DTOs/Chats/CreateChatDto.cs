using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Application.DTOs.Chats
{
    public class CreateChatDto
    {
        public string? Content { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }
}
