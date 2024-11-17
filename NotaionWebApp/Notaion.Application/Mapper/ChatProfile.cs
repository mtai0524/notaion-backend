using AutoMapper;
using Notaion.Application.DTOs;
using Notaion.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Application.Mapper
{
    public class ChatProfile : Profile
    {
        public ChatProfile()
        {
            CreateMap<Chat, GetChatRequest>()
                        .ForMember(dest => dest.IsHiden, opt => opt.MapFrom(src => src.Hide)); // config auto mapper - prefix
        }
    }
}
