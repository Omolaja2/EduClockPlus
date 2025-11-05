using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduClockPlus.Models.DB;
using ClassClockPlus.Models;

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

        [HttpGet]
        public IActionResult AddTeacher()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddTeacher(string FullName, string Email, string Password, string? PhoneNumber, string? ClassName, string? Subject)
        {
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View();
            }

            // Create a linked User record
            var user = new User
            {
                UserID = Guid.NewGuid(),
                FullName = FullName,
                Email = Email,
                PasswordHash = Password,
                Role = "Teacher"
            };

            var teacher = new Teacher
            {
                TeacherID = Guid.NewGuid(),
                UserID = user.UserID,
                User = user,
                FullName = FullName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                ClassName = ClassName,
                Subject = Subject
            };

            _context.Users.Add(user);
            _context.Teachers.Add(teacher);
            _context.SaveChanges();

            TempData["Success"] = "Teacher added successfully!";
            return RedirectToAction("Dashboard");
        }
    }
}
