using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassClockPlus.Models
{
    public class Parent
    {
        [Key]
        public Guid ParentID { get; set; }

        public Guid UserID { get; set; }
        public User? User { get; set; }

        public string? Phone { get; set; }

        public ICollection<Student>?  Students { get; set; }
    }
}
