using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassClockPlus.Models
{
    public class Student
    {
        [Key]
        public Guid StudentID { get; set; }

        [Required]
        public string FullName { get; set; } = default!;

        public string? Gender { get; set; }

        public string? ClassName { get; set; }

        public string? ImagePath { get; set; }

        public Guid ParentID { get; set; }
        public Parent Parent { get; set; } = default!;

        public Guid TeacherID { get; set; }
        public Teacher Teacher { get; set; } = default!;

        public bool IsClockedOut { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
