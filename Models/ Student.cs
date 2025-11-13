using System.ComponentModel.DataAnnotations;
using EduClockPlus.Models;

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
        [Range(0, 100)]
        public int Attendance { get; set; } = 100;
        public string? Performance { get; set; } = "Good";
        public string? Subjects { get; set; } = "-";
        public DateTime? ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }
        public bool IsClockedIn { get; set; } = false;

        public int SchoolId { get; set; } 
        public School? School { get; set; }
    }
}
