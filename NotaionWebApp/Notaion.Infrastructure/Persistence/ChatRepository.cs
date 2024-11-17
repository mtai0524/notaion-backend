using AutoMapper;
using Notaion.Application.DTOs;
using Notaion.Application.Repositories;
using Notaion.Domain.Entities;
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
        public List<GetChatRequest> GetChats()
        {
            return this._context.Chat
                .OrderBy(c => c.SentDate)
                .Where(x => x.Hide == false)
                .ToList()
                .Select(p => this._mapper.Map<GetChatRequest>(p)).ToList();
        }

        public List<GetChatRequest> GetChatsHide()
        {
            return this._context.Chat
              .OrderBy(c => c.SentDate)
              .Where(x => x.Hide == true)
              .ToList()
              .Select(p => this._mapper.Map<GetChatRequest>(p)).ToList();
        }
    }
}
