using System;

namespace Notaion.Application.DTOs.Chats
{
    public class ParticipantDto
    {
        public string UserName { get; set; } = string.Empty;
        public int MessageCount { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }
}
