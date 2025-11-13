using ClassClockPlus.Models;

namespace EduClockPlus.Models
{
    public class School
    {
        public Guid Id { get; set; }
        public string SchoolName { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string Email { get; set; } = default!;
        public ICollection<User> Users { get; set; } = default!;
        public ICollection<Teacher>? Teachers { get; set; }
        public ICollection<Student>? Students { get; set; }
        public ICollection<Parent>? Parents { get; set; }
    }
}