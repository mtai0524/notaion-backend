namespace Notaion.Application.DTOs
{
    public class PaginatedResultDto<T>
    {
        public List<T>? Items { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

}
