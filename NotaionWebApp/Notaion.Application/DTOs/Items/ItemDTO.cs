namespace Notaion.Application.DTOs.Items
{
    public class ItemDTO
    {
        public string? Content { get; set; }
        public string? Heading { get; set; }
        public int? Order { get; set; }
        public bool IsHide { get; set; } = false;
    }
}
