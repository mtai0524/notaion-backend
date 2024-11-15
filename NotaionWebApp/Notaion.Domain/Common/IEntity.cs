using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Domain.Common
{
    public interface IEntity<TypeOfKey>
    {
        [Key]
        public TypeOfKey Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
