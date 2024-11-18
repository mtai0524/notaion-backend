using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Notaion.Application.DTOs.Chats;
using Notaion.Domain.Entities;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Infrastructure.Persistence
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public ChatRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Chat>> GetChatsHiddenAsync()
        {
            return _mapper.Map<List<Chat>>(await _context.Chat
              .Where(c => c.Hide == true)
              .OrderBy(c => c.SentDate)
              .ToListAsync());
        }

        async Task<List<Chat>> IChatRepository.GetChatsAsync()
        {
            return _mapper.Map<List<Chat>>(await _context.Chat
              .Where(c => c.Hide == false)
              .OrderBy(c => c.SentDate)
              .ToListAsync());
        }

    }
}
