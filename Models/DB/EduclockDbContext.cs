using ClassClockPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace EduClockPlus.Models.DB
{
    public class EduclockDbContext : DbContext
    {
        public EduclockDbContext(DbContextOptions<EduclockDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<ReportCard> ReportCards { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<User> User { get; set; }
    }
}