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
            <style>
                body {{
                    font-family: 'Segoe UI', Arial, sans-serif;
                    background-color: #f7faff;
                    margin: 0;
                    padding: 0;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: 20px auto;
                    background-color: #ffffff;
                    border-radius: 10px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.05);
                    overflow: hidden;
                    border-top: 5px solid #007bff;
                }}
                .header {{
                    background-color: #007bff;
                    color: #ffffff;
                    text-align: center;
                    padding: 20px;
                }}
                .header h2 {{
                    margin: 0;
                    font-size: 22px;
                    letter-spacing: 0.5px;
                }}
                .content {{
                    padding: 25px 30px;
                    color: #333;
                    font-size: 15px;
                    line-height: 1.6;
                }}
                .content strong {{
                    color: #007bff;
                }}
                .status-badge {{
                    display: inline-block;
                    background-color: #e8f0fe;
                    color: #007bff;
                    padding: 6px 14px;
                    border-radius: 20px;
                    font-weight: 600;
                    font-size: 14px;
                    margin-top: 10px;
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
                    <h2>EduClockPlus Notification</h2>
                </div>
                <div class='content'>
                    <p>Dear {student.Parent.User!.FullName},</p>

                    <p>Your child <strong>{student.FullName}</strong> was marked as:</p>

                    <div class='status-badge'>{status}</div>

                    <p style='margin-top:20px;'>Date: <strong>{DateTime.Now:dddd, MMM dd yyyy}</strong></p>

                    <p>We wanted to keep you updated on your child’s attendance status.</p>
                    <p>Best regards,<br/>
                    <strong>EduClockPlus Team</strong></p>
                </div>
                <div class='footer'>
                    © {DateTime.Now.Year} EduClockPlus. All rights reserved.
                </div>
            </div>";

            await _emailService.SendEmailAsync(student.Parent.User.Email, "Attendance Notification", message);

            TempData["Success"] = $"{student.FullName}'s attendance recorded and parent notified.";

            return RedirectToAction("Index");
        }
    }
}
