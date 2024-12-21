using Notaion.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notaion.Domain.Entities
{
    [Table("Item")]
    public class Item : BaseEntity<Guid>
    {
        public string? Content { get; set; }
        public string? Heading { get; set; }
        public int? Order { get; set; }
        public bool IsHide { get; set; } = false;
    }
}
