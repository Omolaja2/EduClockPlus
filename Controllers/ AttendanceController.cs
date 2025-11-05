using Microsoft.AspNetCore.Mvc;
using EduClockPlus.Models.DB;
using EduClockPlus.Services;
using Microsoft.EntityFrameworkCore;
using ClassClockPlus.Models;

namespace EduClockPlus.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly EduclockDbContext _context;
        private readonly EmailService _emailService;

        public AttendanceController(EduclockDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            var records = _context.Attendance
                .Include(a => a.Student)
                .ThenInclude(s => s.Teacher)
                .Include(a => a.Student.Parent)
                .OrderByDescending(a => a.Date)
                .ToList();

            return View(records);
        }

        [HttpGet]
        public IActionResult Mark()
        {
            var students = _context.Students
                .Include(s => s.Teacher)
                .ToList();

            ViewBag.Students = students;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Mark(Guid studentId, bool isPresent)
        {
            var student = await _context.Students
                .Include(s => s.Parent).ThenInclude(p => p.User)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(s => s.StudentID == studentId);

            if (student == null)
            {
                TempData["Error"] = "Student not found.";
                return RedirectToAction("Mark");
            }

            var record = new Attendance
            {
                AttendanceID = Guid.NewGuid(),
                StudentID = student.StudentID,
                Date = DateTime.Now,
            };

            _context.Attendance.Add(record);
            await _context.SaveChangesAsync();

            string status = isPresent ? "present" : "absent";
            string message = $@"
                Dear {student.Parent.User!.FullName},<br/>
                Your child <strong>{student.FullName}</strong> was marked as <b>{status}</b> today ({DateTime.Now:dddd, MMM dd yyyy}).<br/>
                <br/>
                Regards,<br/>EduClockPlus";

            await _emailService.SendEmailAsync(student.Parent.User.Email, "Attendance Notification", message);

            TempData["Success"] = $"{student.FullName}'s attendance recorded and parent notified.";
            return RedirectToAction("Index");
        }
    }
}
