using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduClockPlus.Models.DB;


namespace ClassClockPlus.Controllers
{
    public class AdminController : Controller
    {
        private readonly EduclockDbContext _context;
        public AdminController(EduclockDbContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            var teachers = _context.Teachers
                .Include(t => t.User)
                .Select(t => new
                {
                    t.FullName,
                    t.Email,
                    t.PhoneNumber,
                    t.ClassName,
                    t.Subject,
                    StudentCount = t.Students != null ? t.Students.Count : 0,
                    Role = t.User != null ? t.User.Role : "Teacher"
                })
                .ToList();

            var parents = _context.Users
                .Where(u => u.Role == "Parent")
                .Select(u => new
                {
                    u.FullName,
                    u.Email,
                    u.Role
                })
                .ToList();
            ViewBag.Teachers = teachers;
            ViewBag.Parents = parents;

            return View();

        }
       
    }
}
