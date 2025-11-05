using System.ComponentModel.DataAnnotations;
using EduClockPlus.Models;

namespace ClassClockPlus.Models
{
    public class User
    {
        [Key]
        public Guid UserID { get; set; }

        [Required]
        public string FullName { get; set; } = default!;

        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string PasswordHash { get; set; } = default!;

        [Required]
        public string Role { get; set; } = default!; // "Admin", "Teacher", "Parent"
        public int SchoolId { get; set; }
        public School? School { get; set; }
    }
}
