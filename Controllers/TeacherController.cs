using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduClockPlus.Models.DB;
using ClassClockPlus.Models;
using EduClockPlus.Services;

namespace EduClockPlus.Controllers
{
    public class TeacherController : Controller
    {
        private readonly EduclockDbContext _context;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public TeacherController(EduclockDbContext context, EmailService emailService, IWebHostEnvironment env)
        {
            _context = context;
            _emailService = emailService;
            _env = env;
        }

        public IActionResult Dashboard()
        {
            var currentTeacherEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(currentTeacherEmail))
                return RedirectToAction("Login", "Account");

            var teacher = _context.Teachers
                .Include(t => t.User)
                .FirstOrDefault(t => t.Email == currentTeacherEmail || t.User!.Email == currentTeacherEmail);

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            var students = _context.Students
                .Where(s => s.TeacherID == teacher.TeacherID)
                .Include(s => s.Parent).ThenInclude(p => p.User)
                .ToList();

            var parents = _context.Parents.Include(p => p.User).ToList();

            ViewBag.Teacher = teacher;
            ViewBag.Students = students;
            ViewBag.Parents = parents;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SaveStudent(IFormFile? imageFile, string studentName, string gender, Guid parentId)
        {
            var teacherEmail = HttpContext.Session.GetString("UserEmail");
            var teacher = _context.Teachers
                .Include(t => t.User)
                .FirstOrDefault(t => t.Email == teacherEmail || t.User!.Email == teacherEmail);

            if (teacher == null)
                return Json(new { success = false, message = "Teacher not found." });

            string? imagePath = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "students");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await imageFile.CopyToAsync(stream);
                imagePath = $"/uploads/students/{fileName}";
            }

            var student = new Student
            {
                StudentID = Guid.NewGuid(),
                FullName = studentName,
                Gender = gender,
                ClassName = teacher.ClassName,
                TeacherID = teacher.TeacherID,
                ParentID = parentId,
                ImagePath = imagePath,
                IsClockedIn = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return Json(new { success = true, student });
        }


        [HttpPost]
        public async Task<IActionResult> ToggleClock([FromBody] ClockDto dto)
        {
            var student = await _context.Students
                .Include(s => s.Parent).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.StudentID == dto.StudentId);

            if (student == null)
                return Json(new { success = false, message = "Student not found." });

            student.IsClockedIn = !student.IsClockedIn;
            if (student.IsClockedIn)
            {
                student.ClockInTime = DateTime.Now;
                await _emailService.SendEmailAsync(student.Parent.User!.Email,
                    $"✅ {student.FullName} Clocked In",
                    $"{student.FullName} clocked in at {student.ClockInTime:hh:mm tt}.");

                _context.Attendance.Add(new Attendance
                {
                    AttendanceID = Guid.NewGuid(),
                    StudentID = student.StudentID,
                    TeacherID = student.TeacherID,
                    StudentName = student.FullName,
                    Date = DateTime.UtcNow.Date,
                    Status = "Present"
                });
            }
            else
            {
                student.ClockOutTime = DateTime.Now;
                await _emailService.SendEmailAsync(student.Parent.User!.Email,
                    $"🕓 {student.FullName} Clocked Out",
                    $"{student.FullName} clocked out at {student.ClockOutTime:hh:mm tt}.");
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isClockedIn = student.IsClockedIn });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttendanceBatch([FromBody] List<AttendanceInput> attendanceList)
        {
            var teacherEmail = HttpContext.Session.GetString("UserEmail");
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == teacherEmail);
            if (teacher == null)
                return Json(new { success = false, message = "Unauthorized" });

            var today = DateTime.UtcNow.Date;

            foreach (var item in attendanceList)
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentID == item.StudentId);
                if (student == null)
                    continue;

                var existing = await _context.Attendance
                    .FirstOrDefaultAsync(a => a.StudentID == item.StudentId && a.Date == today);

                if (existing == null)
                {
                    _context.Attendance.Add(new Attendance
                    {
                        AttendanceID = Guid.NewGuid(),
                        StudentID = item.StudentId,
                        TeacherID = teacher.TeacherID,
                        StudentName = student.FullName,
                        Date = today,
                        Status = item.Status
                    });
                }
                else
                {
                    existing.Status = item.Status;
                    existing.StudentName = student.FullName;
                    _context.Attendance.Update(existing);
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return Json(new { success = false, message = "Student not found." });

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Student removed successfully!." });
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentDetails(Guid id)
        {
            var student = await _context.Students
                .Include(s => s.Parent).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.StudentID == id);

            if (student == null)
                return NotFound("Student not found");

            var attendance = await _context.Attendance
                .Where(a => a.StudentID == id)
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToListAsync();

            return PartialView("_StudentDetailsPartial", new
            {
                student.FullName,
                student.Gender,
                student.ClassName,
                student.ImagePath,
                student.IsClockedIn,
                student.ClockInTime,
                student.ClockOutTime,
                ParentName = student.Parent?.User?.FullName,
                ParentEmail = student.Parent?.User?.Email,
                Attendance = attendance
            });
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Add(string fullName, string email, string phoneNumber, string className, string subject, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Email is required";
                return View();
            }

            var user = new User
            {
                UserID = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = password,
                Role = "Teacher"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var teacher = new Teacher
            {
                TeacherID = Guid.NewGuid(),
                UserID = user.UserID,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                ClassName = className,
                Subject = subject,
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            string loginLink = $"{Request.Scheme}://{Request.Host}/Account/Login";
            string body = $@"
            <div style='font-family:Poppins,Arial,sans-serif;background-color:#f9fbfd;padding:25px;border-radius:10px;'>
                <h2 style='color:#007bff;'>Welcome to EduClockPlus 🎓</h2>
                <p>Dear <strong>{fullName}</strong>,</p>
                <p>Your teacher account has been successfully created! . Below are your login details:</p>
                <div style='background:#eaf3ff;padding:10px 15px;border-radius:8px;margin-top:10px;'>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Password:</strong> {password}</p>
                </div>
                <p style='margin-top:15px;'>You can now log in to your dashboard using the link below:</p>
                <p><a href='{loginLink}' style='color:white;background-color:#007bff;padding:10px 20px;border-radius:6px;text-decoration:none;'>Login to Portal</a></p>
                <p style='margin-top:25px;color:#555;'>Best regards,<br/><strong>The EduClockPlus Team</strong></p>
            </div>";

            await _emailService.SendEmailAsync(email, "Your EduClockPlus Account Details", body);

            TempData["Success"] = "Teacher added successfully and email sent!";
            return RedirectToAction("Add");
        }
    }








    public class ClockDto
    {
        public Guid StudentId { get; set; }
    }
    public class AttendanceInput
    {
        public Guid StudentId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
