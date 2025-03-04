﻿using Notaion.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Domain.Interfaces
{
    public interface IChatRepository : IGenericRepository<Chat>
    {
        Task<string> GetChatbotResponseAsync(string userMessage);
        Task<Chat> AddChatbotAsync(Chat chat);
    }
}
