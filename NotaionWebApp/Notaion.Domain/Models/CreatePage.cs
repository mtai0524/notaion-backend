namespace Notaion.Domain.Models
{
    public class CreatePage
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? UserId { get; set; }
        public bool? Public { get; set; }
    }
}
