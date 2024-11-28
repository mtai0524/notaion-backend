using System.ComponentModel;

namespace Notaion.Application.Options
{
    public class QueryOptions
    {
        [DefaultValue(1)] // Swagger sẽ nhận diện giá trị mặc định
        public int PageNumber { get; set; } = 1;

        [DefaultValue(10)] // Swagger sẽ nhận diện giá trị mặc định
        public int PageSize { get; set; } = 10;
    }
}
