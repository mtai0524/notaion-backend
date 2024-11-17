using Notaion.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Application.Repositories
{
    public interface IChatRepository
    {
        List<GetChatRequest> GetChats();
        List<GetChatRequest> GetChatsHide();
    }
}
