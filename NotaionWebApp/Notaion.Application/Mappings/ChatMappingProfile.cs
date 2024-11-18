using AutoMapper;
using Notaion.Application.DTOs.Chats;
using Notaion.Domain.Entities;

namespace Notaion.Application.Mappings
{
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {
            CreateMap<Chat, ChatResponseDto>()
                        .ForMember(dest => dest.IsHiden, opt => opt.MapFrom(src => src.Hide)); // config auto mapper - prefix
        }
    }
}
