using System.ComponentModel.DataAnnotations;
using EduClockPlus.Models;

namespace ClassClockPlus.Models
{
    public class Attendance
    {
        [Key]
        public Guid AttendanceID { get; set; }
        public Guid StudentID { get; set; }
        public Student Student { get; set; } = default!;
        public Guid TeacherID { get; set; }
        public Teacher? Teacher { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = default!;
        [MaxLength(100)]
        public string StudentName { get; set; } = string.Empty;                     
    }
}
