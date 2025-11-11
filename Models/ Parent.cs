using System.ComponentModel.DataAnnotations;

namespace ClassClockPlus.Models
{
    public class Parent
    {
        [Key]
        public Guid ParentID { get; set; }
        public Guid UserID { get; set; }
        public User? User { get; set; }
        public string? Phone { get; set;}
        public ICollection<Student>?  Students { get; set; } 
        public ICollection<Notification>? Notifications { get; set; }
    }
}


