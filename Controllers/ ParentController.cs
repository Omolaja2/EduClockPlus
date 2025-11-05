using Microsoft.AspNetCore.Mvc;
using EduClockPlus.Models.DB;
using Microsoft.EntityFrameworkCore;
using EduClockPlus.Services;

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

        // ✅ Dashboard for Parent
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

        // ✅ Student Details under parent
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

        // ✅ Send feedback to teacher
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

            // Send email feedback to teacher
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
