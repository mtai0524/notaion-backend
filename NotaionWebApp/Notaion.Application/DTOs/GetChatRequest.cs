using Notaion.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Application.DTOs
{
    public class GetChatRequest
    {
        public string? Content { get; set; }
        public DateTime? SentDate { get; set; }
        public string? UserName { get; set; }
        public bool IsHiden { get; set; }
    }
}
