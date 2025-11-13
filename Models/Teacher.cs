using System.ComponentModel.DataAnnotations;
using EduClockPlus.Models;

namespace ClassClockPlus.Models
{
    public class Teacher
    {
        [Key]
        public Guid TeacherID { get; set; }
        public Guid UserID { get; set; }
        public User? User { get; set; }
        [Required]
        public string FullName { get; set; } = string.Empty;
        [EmailAddress]
        public string? Email { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
        public string? ClassName { get; set; }
        public string? Subject { get; set; }
        public int SchoolId { get; set; }    
        public School? School { get; set; }
        public ICollection<Student>? Students { get; set; }
    }
}








