using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduClockPlus.Models.DB;
using ClassClockPlus.Models;

namespace EduClockPlus.Controllers
{
    public class TeacherController : Controller
    {
        private readonly EduclockDbContext _context;

        public TeacherController(EduclockDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            string currentTeacherEmail = HttpContext.Session.GetString("UserEmail")!;
            var teacher = _context.Teachers.FirstOrDefault(t => t.Email == currentTeacherEmail);

            if (teacher == null)
            {
                ViewBag.Error = "Teacher not found. Please log in again.";
                return View("Error");
            }


            var students = _context.Students
                .Where(s => s.TeacherID == teacher.TeacherID)
                .Include(s => s.Parent)
                .ThenInclude(p => p.User)
                .ToList();
            var parents = _context.Parents.Include(p => p.User).ToList();

            ViewBag.Teacher = teacher;
            ViewBag.Students = students;
            ViewBag.Parents = parents;
            return View();
        }
        [HttpPost]
        public IActionResult SaveStudent([FromBody] StudentDto dto)
        {
            var teacherEmail = HttpContext.Session.GetString("UserEmail");
            var teacher = _context.Teachers.FirstOrDefault(t => t.Email == teacherEmail);

            if (teacher == null)
                return Json(new { success = false, message = "Teacher not found." });

            var student = new Student
            {
                StudentID = Guid.NewGuid(),
                FullName = dto.StudentName,
                ClassName = dto.Grade,
                TeacherID = teacher.TeacherID,
                ParentID = dto.ParentId
            };
            _context.Students.Add(student);
            _context.SaveChanges();

            var parent = _context.Parents.Include(p => p.User).FirstOrDefault(p => p.ParentID == dto.ParentId);

            return Json(new
            {
                success = true,
                student = new
                {
                    student.StudentID,
                    fullName = student.FullName,
                    grade = student.ClassName,
                    parentName = parent?.User?.FullName,
                    parentEmail = parent?.User?.Email
                }
            });

        }

        [HttpGet]
        public IActionResult GetStudentDetails(Guid studentId)
        {
            var student = _context.Students
                .Include(s => s.Parent)
                .ThenInclude(p => p.User)
                .FirstOrDefault(s => s.StudentID == studentId);

            if (student == null)
                return Json(new { success = false, message = "Student not found." });

            return Json(new
            {
                success = true,
                student = new
                {
                    fullName = student.FullName,
                    grade = student.ClassName,
                    parentName = student.Parent?.User?.FullName,
                    parentEmail = student.Parent?.User?.Email
                }
            });
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(string FullName, string Email, string Password, string? PhoneNumber, string? ClassName, string? Subject)
        {
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View();
            }
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
    public class StudentDto
    {
        public string StudentName { get; set; } = default!;
        public string Grade { get; set; } = default!;
        public Guid ParentId { get; set; }
    }
}
