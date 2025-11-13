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

        public IActionResult Login()
        {
            return View();
        }

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
                ViewBag.Error = "Invalid credentials or account not found!";
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
                <style>
                    body {{
                        font-family: 'Segoe UI', Arial, sans-serif;
                        background-color: #f4f8ff;
                        margin: 0;
                        padding: 0;
                    }}
                    .email-container {{
                        max-width: 600px;
                        margin: 25px auto;
                        background-color: #ffffff;
                        border-radius: 10px;
                        box-shadow: 0 4px 12px rgba(0,0,0,0.08);
                        overflow: hidden;
                        border-top: 5px solid #007bff;
                    }}
                    .header {{
                        background-color: #007bff;
                        color: #ffffff;
                        text-align: center;
                        padding: 25px;
                    }}
                    .header h2 {{
                        margin: 0;
                        font-size: 22px;
                        letter-spacing: 0.4px;
                    }}
                    .content {{
                        padding: 30px;
                        color: #333;
                        font-size: 15px;
                        line-height: 1.7;
                    }}
                    .content strong {{
                        color: #007bff;
                    }}
                    .btn {{
                        display: inline-block;
                        background-color: #007bff;
                        color: #fff !important;
                        padding: 10px 20px;
                        border-radius: 6px;
                        text-decoration: none;
                        font-weight: 600;
                        margin-top: 15px;
                    }}
                    .footer {{
                        text-align: center;
                        background-color: #f1f6ff;
                        padding: 15px;
                        font-size: 13px;
                        color: #777;
                        border-top: 1px solid #e0e6ef;
                    }}
                </style>

                <div class='email-container'>
                    <div class='header'>
                        <h2>Welcome to EduClockPlus 🎓</h2>
                    </div>
                    <div class='content'>
                        <p>Dear <strong>Hi {adminName}!</strong>,</p>

                        <p>Congratulations! Your school <strong>{schoolName}</strong> has been successfully registered on <strong>EduClockPlus</strong>.</p>

                        <p>You can now log in to your <strong>Admin Dashboard</strong> to manage teachers, students, and parents efficiently.</p>

                        <a href='{Request.Scheme}://{Request.Host}/Account/Login' class='btn'>Go to Login Page</a>

                        <p style='margin-top: 25px;'>If you have any questions, our support team is always here to help  email us at educlock@gmail.com.</p>

                        <p>Best regards,<br/>
                        <strong>The EduClockPlus Team</strong></p>
                    </div>
                    <div class='footer'>
                        © {DateTime.Now.Year} EduClockPlus. All rights reserved.
                    </div>
                </div>";
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
