using Microsoft.AspNetCore.Mvc;
using EduClockPlus.Models.DB;
using Microsoft.EntityFrameworkCore;
using EduClockPlus.Services;
using ClassClockPlus.Models;
namespace EduClockPlus.Controllers
{
    public class ParentController : Controller
    {
        private readonly EduclockDbContext _context;
        private readonly EmailService _emailService;

        public ParentController(EduclockDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(string fullName, string email, string phone, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }
            // // Check for duplicate email
            // if (_context.Users.Any(u => u.Email == email))
            // {
            //     ViewBag.Error = "A user with this email already exists.";
            //     return View();
            // }
            var user = new User
            {
                UserID = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = password,
                Role = "Parent"
            };

            var parent = new Parent
            {
                ParentID = Guid.NewGuid(),
                UserID = user.UserID,
                User = user,
                Phone = phone
            };

            _context.Users.Add(user);
            _context.Parents.Add(parent);
            _context.SaveChanges();

            TempData["Success"] = "Parent added successfully!";
            return RedirectToAction("Dashboard", "Admin");
        }

        public IActionResult Dashboard()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var parent = _context.Parents
                .Include(p => p.User)
                .Include(p => p.Students!)
                .ThenInclude(c => c.Teacher)
                .FirstOrDefault(p => p.User!.Email == email);

            if (parent == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Parent = parent;
            return View();
        }

        public IActionResult StudentDetails(Guid id)
        {
            var student = _context.Students
                .Include(s => s.Teacher)
                .Include(s => s.Parent)
                .FirstOrDefault(s => s.StudentID == id);

            if (student == null)
                return NotFound();

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> SendFeedback(Guid teacherId, string message)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherID == teacherId);

            if (teacher == null)
            {
                TempData["Error"] = "Teacher not found.";
                return RedirectToAction("Dashboard");
            }

            await _emailService.SendEmailAsync(
                teacher.Email!,
                "Parent Feedback",
                message
            );

            TempData["Success"] = "Your message was successfully sent to the teacher.";
            return RedirectToAction("Dashboard");
        }
    }
}
