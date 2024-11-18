using AutoMapper;
using Notaion.Application.DTOs.Users;
using Notaion.Domain.Entities;

namespace Notaion.Application.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserResponseDto>();
        }
    }
}
