using System;
using System.ComponentModel.DataAnnotations;

namespace Notaion.Domain.Entities
{
    public class DailyNote
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Color { get; set; }
        public string? Category { get; set; }
        public string? Timestamp { get; set; }
        public string? Date { get; set; } // YYYY-MM-DD
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int ZIndex { get; set; }
        public string? UserId { get; set; }
    }
}
