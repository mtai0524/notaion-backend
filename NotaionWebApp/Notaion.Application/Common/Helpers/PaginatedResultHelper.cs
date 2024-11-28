using Notaion.Application.DTOs;

namespace Notaion.Application.Common.Helpers
{
    public static class PaginatedResultHelper
    {
        public static PaginatedResultDto<T> CreatePaginatedResult<T>(
            List<T> items,
            int totalItems,
            int currentPage,
            int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginatedResultDto<T>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = currentPage,
                PageSize = pageSize
            };
        }
    }

}
