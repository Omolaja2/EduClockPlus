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


        public IActionResult Dashboard()

        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var parent = _context.Parents
                .Include(p => p.User)
                .Include(p => p.Students!)
                    .ThenInclude(s => s.Teacher)
                .FirstOrDefault(p => p.User!.Email == email);

            if (parent == null)
                return RedirectToAction("Login", "Account");

            var notifications = _context.Notifications
                .Where(n => n.ParentID == parent.ParentID && n.SentAt >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(n => n.SentAt)
                .ToList();

            var childrenData = parent.Students!
                .Select(s =>
                {
                    var totalDays = _context.Attendance.Count(a => a.StudentID == s.StudentID);
                    var presentDays = _context.Attendance.Count(a => a.StudentID == s.StudentID && a.Status == "Present");
                    var attendancePercent = totalDays > 0 ? (presentDays * 100 / totalDays) : 0;

                    return new
                    {
                        s.StudentID,
                        s.FullName,
                        s.ClassName,
                        s.ImagePath,
                        s.IsClockedIn,
                        s.IsClockedOut,
                        s.Performance,
                        Attendance = attendancePercent,
                        Teacher = s.Teacher,
                    };
                }).ToList();

            ViewBag.Parent = parent;
            ViewBag.Children = childrenData;
            ViewBag.Notifications = notifications;
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

            var attendanceHistory = _context.Attendance
                .Where(a => a.StudentID == id)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();

            ViewBag.AttendanceHistory = attendanceHistory;

            return PartialView("_StudentDetailsPartial", student);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(string fullName, string email, string phone, string password)
        {
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
                UserID = user.UserID
            };

            _context.Users.Add(user);
            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();

            string loginLink = $"{Request.Scheme}://{Request.Host}/Account/Login";
            string body = $@"
            <div style='font-family:Poppins,Arial,sans-serif;background-color:#f9fbfd;padding:25px;border-radius:10px;'>
                <h2 style='color:#007bff;'>Welcome to EduClockPlus 🎓</h2>
                <p>Dear <strong>{fullName}</strong>,</p>
                <p>Your parent account has been created successfully! Here are your login credentials:</p>
                <div style='background:#eaf3ff;padding:10px 15px;border-radius:8px;margin-top:10px;'>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Password:</strong> {password}</p>
                </div>
                <p style='margin-top:15px;'>Click below to access your Parent Dashboard:</p>
                <p><a href='{loginLink}' style='color:white;background-color:#007bff;padding:10px 20px;border-radius:6px;text-decoration:none;'>Login to Portal</a></p>
                <p style='margin-top:25px;color:#555;'>Best regards,<br/><strong>The EduClockPlus Team</strong></p>
            </div>";

            await _emailService.SendEmailAsync(email, "Your EduClockPlus Account Details", body);

            TempData["Success"] = "Parent added successfully and email sent!";
            return RedirectToAction("Dashboard", "Admin");
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

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction("Dashboard");

        }
    }
}
