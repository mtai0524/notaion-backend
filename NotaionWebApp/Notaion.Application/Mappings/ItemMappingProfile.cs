using Notaion.Application.DTOs.Items;
using Notaion.Domain.Entities;
using AutoMapper;
namespace Notaion.Application.Mappings
{
    public class ItemMappingProfile : Profile
    {
        public ItemMappingProfile()
        {
            CreateMap<Item, ItemDTO>();
            CreateMap<ItemDTO, Item>();
        }
    }
}
