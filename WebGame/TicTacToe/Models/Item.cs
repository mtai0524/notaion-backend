﻿using System.ComponentModel.DataAnnotations;

namespace Notaion.Models
{
    public class Item
    {
        [Key]
        public string Id { get; set; }
        public string? Content { get; set; }
        public string? Heading { get; set; }
        public int? Order { get; set; }
    }
}