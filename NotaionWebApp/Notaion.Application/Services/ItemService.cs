using AutoMapper;
using Notaion.Application.DTOs.Items;
using Notaion.Application.Interfaces.Services;
using Notaion.Domain.Interfaces;

namespace Notaion.Application.Services;

public class ItemService : IItemService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ItemDTO>> GetItemsAsync()
    {
        var items = await _unitOfWork.ItemRepository.GetAllAsync();
        return _mapper.Map<List<ItemDTO>>(items);
    }

    public async Task<List<ItemDTO>> GetItemHiddenAsync()
    {
        var items = await _unitOfWork.ItemRepository.GetAsync(x => x.IsHide == false);
        return _mapper.Map<List<ItemDTO>>(items);
    }
}