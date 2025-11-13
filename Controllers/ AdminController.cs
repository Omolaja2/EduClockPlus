using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduClockPlus.Models.DB;
namespace ClassClockPlus.Controllers
{
    public class AdminController : Controller
    {
        private readonly EduclockDbContext _context;

        public AdminController(EduclockDbContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            var schoolId = HttpContext.Session.GetInt32("SchoolId");

            if (schoolId == null)
                return RedirectToAction("Login", "Account");

            var teachers = _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Students)
                .Where(t => t.SchoolId == schoolId) 
                .Select(t => new
                {
                    t.FullName,
                    t.Email,
                    t.PhoneNumber,
                    t.ClassName,
                    t.Subject,
                    StudentCount = t.Students != null ? t.Students.Count : 0,
                    Role = t.User != null ? t.User.Role : "Teacher"
                })
                .ToList();

            var parents = _context.Parents
                .Include(p => p.User)
                .Where(p => p.SchoolId == schoolId)
                .Select(p => new
                {
                    FullName = p.User != null ? p.User.FullName : "N/A",
                    Email = p.User != null ? p.User.Email : "N/A",
                    Role = p.User != null ? p.User.Role : "Parent"
                })
                .ToList();

            ViewBag.Teachers = teachers;
            ViewBag.Parents = parents;

            return View();
        }

        public IActionResult SchoolReport()
        {
            var schoolId = HttpContext.Session.GetInt32("SchoolId");
            if (schoolId == null)
                return RedirectToAction("Login", "Account");

            var totalTeachers = _context.Teachers.Count(t => t.SchoolId == schoolId);
            var totalStudents = _context.Students.Count(s => s.SchoolId == schoolId);
            var totalParents = _context.Parents.Count(p => p.SchoolId == schoolId);
            var totalClasses = _context.Teachers
                .Where(t => t.SchoolId == schoolId && t.ClassName != null)
                .Select(t => t.ClassName)
                .Distinct()
                .Count();

            var totalAttendanceRecords = _context.Attendance
                .Count(a => a.SchoolId == schoolId);
            var presentRecords = _context.Attendance
                .Count(a => a.SchoolId == schoolId && a.Status == "Present");
            var attendancePercent = totalAttendanceRecords > 0
                ? (presentRecords * 100 / totalAttendanceRecords)
                : 0;

            var studentsByClass = _context.Teachers
                .Include(t => t.Students)
                .Where(t => t.SchoolId == schoolId && t.ClassName != null)
                .AsEnumerable()
                .GroupBy(t => t.ClassName)
                .Select(g => new
                {
                    ClassName = g.Key,
                    StudentCount = g.Sum(t => t.Students?.Count ?? 0)
                })
                .OrderBy(g => g.ClassName)
                .ToList();


            var teachersByClass = _context.Teachers
                .Include(t => t.User)
                .Include(t => t.Students)
                .Where(t => t.SchoolId == schoolId && t.ClassName != null)
                .AsEnumerable()
                .GroupBy(t => t.ClassName)
                .Select(g => new
                {
                    ClassName = g.Key,
                    Teachers = g.Select(t => new
                    {
                        t.FullName,
                        t.Email,
                        t.Subject,
                        StudentCount = t.Students?.Count ?? 0,
                        Role = t.User?.Role ?? "Teacher"
                    }).ToList()
                })
                .OrderBy(g => g.ClassName)
                .ToList();

            var parents = _context.Parents
                .Include(p => p.User)
                .Where(p => p.SchoolId == schoolId)
                .AsEnumerable()
                .Select(p => new
                {
                    FullName = p.User?.FullName ?? "N/A",
                    Email = p.User?.Email ?? "N/A",
                    Role = p.User?.Role ?? "Parent"
                })
                .ToList();

            ViewBag.TotalTeachers = totalTeachers;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalParents = totalParents;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.AttendancePercent = attendancePercent;
            ViewBag.StudentsByClass = studentsByClass;
            ViewBag.TeachersByClass = teachersByClass;
            ViewBag.Parents = parents;

            return View();
        }
    }
}
