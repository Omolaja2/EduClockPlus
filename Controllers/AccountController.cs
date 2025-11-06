using Microsoft.AspNetCore.Mvc;
using EduClockPlus.Models.DB;
using ClassClockPlus.Models;
using EduClockPlus.Models;
using EduClockPlus.Services;
namespace EduClockPlus.Controllers
{
    public class AccountController : Controller
    {
        private readonly EduclockDbContext _context;
        private readonly EmailService _emailservice;

        public AccountController(EduclockDbContext context, EmailService emailService)
        {
            _context = context;
            _emailservice = emailService;
        }
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string role, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "Please select a role.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Email == username &&
                u.PasswordHash == password &&
                u.Role == role);

            if (user == null)
            {
                ViewBag.Error = "Invalid credentials or account not found.";
                return View();
            }

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Role);

            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Teacher" => RedirectToAction("Dashboard", "Teacher"),
                "Parent" => RedirectToAction("Dashboard", "Parent"),
                _ => View()
            };
        }
        public IActionResult RegisterSchool() => View();
        [HttpPost]
        public async Task<IActionResult> RegisterSchool(
            string schoolName, string address, string email,
            string adminName, string adminEmail, string adminPassword)
        {
            if (string.IsNullOrWhiteSpace(schoolName) ||
                string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }
            bool schoolExists = _context.Schools.Any(s => s.SchoolName == schoolName || s.Email == email);
            bool adminExists = _context.Users.Any(u => u.Email == adminEmail);
            if (schoolExists || adminExists)
            {
                ViewBag.Error = "This school or admin already exists in the system.";
                return View();
            }
            var newSchool = new School
            {
                Id = Guid.NewGuid(),
                SchoolName = schoolName,
                Address = address,
                Email = email
            };
            _context.Schools.Add(newSchool);
            _context.SaveChanges();
            var adminUser = new User
            {
                UserID = Guid.NewGuid(),
                FullName = adminName,
                Email = adminEmail,
                PasswordHash = adminPassword,
                Role = "Admin",
                SchoolId = (int)newSchool.Id.GetHashCode(),
                School = newSchool
            };
            _context.Users.Add(adminUser);
            _context.SaveChanges();

            try
            {
                string subject = $"Welcome to EduClockPlus, {schoolName}!";
                string body = $@"
            <h2>Welcome to EduClockPlus🎓</h2>
            <p>Dear {adminName},</p>
            <p>Your school <strong>{schoolName}</strong> has been successfully registered.</p>
            <p>You can now log in to your Admin Dashboard and begin adding Teachers and Parents.</p>
            <p><a href='{Request.Scheme}://{Request.Host}/Account/Login' style='color:#007bff;'>Go to Login Page</a></p>
            <br/>
            <p>– The EduClockPlus Team. Thanks</p>";

                await _emailservice.SendEmailAsync(adminEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
            TempData["Success"] = "School registered successfully! Kindly Check your email for login details.";
            return RedirectToAction("Login");
        }
        [HttpPost]
        public IActionResult Logout()
        {
            return RedirectToAction("Login", "Account");
        }
    }

}
