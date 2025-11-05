using Microsoft.AspNetCore.Mvc;
using EduClockPlus.Models.DB;
using ClassClockPlus.Models;
using EduClockPlus.Services;

namespace EduClockPlus.Controllers
{
    public class TeacherController : Controller
    {
        private readonly EduclockDbContext _context;
        private readonly EmailService _emailService;

        public TeacherController(EduclockDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ✅ Show Add Teacher form
        public IActionResult Add() => View();

        // ✅ Handle Add Teacher Post Request
        [HttpPost]
        public async Task<IActionResult> Add(
            string fullName,
            string email,
            string phoneNumber,
            string className,
            string subject,
            string password,
            Guid schoolId)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All required fields must be filled.";
                return View();
            }

            // Check if email already exists
            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "A user with this email already exists.";
                return View();
            }

            // ✅ Step 1: Create the User account
            var newUser = new User
            {
                UserID = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = password, // You can hash later
                Role = "Teacher",
                SchoolId = (int)schoolId.GetHashCode()
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            // ✅ Step 2: Create Teacher record
            var newTeacher = new Teacher
            {
                TeacherID = Guid.NewGuid(),
                UserID = newUser.UserID,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                ClassName = className,
                Subject = subject
            };

            _context.Teachers.Add(newTeacher);
            _context.SaveChanges();

            // ✅ Step 3: Send welcome email
            try
            {
                string subjectLine = $"Welcome to EduClockPlus, {fullName}!";
                string body = $@"
                    <h2>Welcome to EduClockPlus 🎓</h2>
                    <p>Dear {fullName},</p>
                    <p>Your teacher account has been created successfully.</p>
                    <p><strong>Login Email:</strong> {email}</p>
                    <p><strong>Password:</strong> {password}</p>
                    <p>You can now log in to your teacher dashboard.</p>
                    <p><a href='{Request.Scheme}://{Request.Host}/Account/Login' style='color:#007bff;'>Go to Login Page</a></p>
                    <br/>
                    <p>– The EduClockPlus Team</p>";

                await _emailService.SendEmailAsync(email, subjectLine, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }

            TempData["Success"] = "Teacher added successfully and welcome email sent!";
            return RedirectToAction("Dashboard", "Admin");
        }
    }
}
