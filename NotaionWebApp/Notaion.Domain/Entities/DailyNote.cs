using System.ComponentModel.DataAnnotations;
using Notaion.Domain.Enums;

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

        // New Custom Properties
        public string? CustomCategory { get; set; }
        public string? CustomColor { get; set; }
        public string? CustomRgb { get; set; }
        public string? FontSize { get; set; }
        public double Opacity { get; set; } = 1.0;
        public NoteBorderStyle BorderStyle { get; set; } = NoteBorderStyle.Solid;
        public bool Glow { get; set; }
        public double Blur { get; set; }
        public NotePattern Pattern { get; set; } = NotePattern.None;
        public NoteTitleAlign TitleAlign { get; set; } = NoteTitleAlign.Left;
        public bool IsCompleted { get; set; }
        public bool IsMinimized { get; set; }
        public bool HideHeader { get; set; }
        public int NoteTheme { get; set; } // 0: Dark, 1: Light
        public string? CustomTextColor { get; set; }
        public string? CustomTextColorHex { get; set; }
        public string? FontFamily { get; set; }
        public double LineHeight { get; set; } = 1.6;
        public double GlowRadius { get; set; } = 20;
        public double BlurIntensity { get; set; } = 5;
        public double Rotation { get; set; }
        public bool Highlighted { get; set; }
        public bool Compact { get; set; }
        public bool Locked { get; set; }
        public bool Pinned { get; set; }
        public string? DrawingData { get; set; } // Base64 string for sketches
        
        // Deletion/Hiding
        public bool IsDeleted { get; set; }

        // Linking (Store as comma-separated IDs or JSON)
        public string? LinkedNoteIds { get; set; }
    }
}
