using Notaion.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notaion.Entities
{
    [Table("FriendShip")]
    public class FriendShip
    {
        [Key]
        public string Id { get; set; } = new Guid().ToString();

        public string? SenderId { get; set; } // Id của người gửi
        [ForeignKey("SenderId")]
        public virtual User? SenderUser { get; set; }

        public string? ReceiverId { get; set; } // Id của người nhận
        [ForeignKey("ReceiverId")]
        public virtual User? ReceiverUser { get; set; }

        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

      
    }
}
