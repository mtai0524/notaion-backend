using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Domain.Common
{
    public class BaseEntity<TKey> : IEntity<TKey>
    {
        public TKey Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
