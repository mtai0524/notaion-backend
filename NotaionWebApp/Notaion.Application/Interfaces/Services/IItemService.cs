using Notaion.Application.DTOs.Items;

namespace Notaion.Application.Interfaces.Services;

public interface IItemService
{
    Task<List<ItemDTO>> GetItemsAsync();
    Task<List<ItemDTO>> GetItemHiddenAsync();
}