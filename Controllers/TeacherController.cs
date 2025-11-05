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
    }

    public class StudentDto
    {
        public string StudentName { get; set; }
        public string Grade { get; set; }
        public Guid ParentId { get; set; }
    }
}
