namespace Notaion.Domain.Models

{
    public class ChatPrivateViewModel
    {
        public string? Content { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public DateTime? SendDate { get; set; }
    }
}
