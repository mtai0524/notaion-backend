using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notaion.Domain.Entities
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = new Guid();       
        public string? Content { get; set; }      
        public string? SenderId { get; set; }     
        public string? ReceiverId { get; set; }   
        public DateTime? Timestamp { get; set; }    
        public string? SenderName { get; set; }    
        public string? ReceiverName { get; set; }  
        public string? SenderAvatar { get; set; }
        public bool IsRead { get; set; } = false;
        public Notification()
        {
            Timestamp = DateTime.UtcNow;  
        }
    }
}
