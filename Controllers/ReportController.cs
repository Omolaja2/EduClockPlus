using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassClockPlus.Models;
using EduClockPlus.Models.DB;
using EduClockPlus.Services;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Properties;


namespace EduClockPlus.Controllers
{
    public class ReportController : Controller
    {
        private readonly EduclockDbContext _context;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public ReportController(EduclockDbContext context, EmailService emailService, IWebHostEnvironment env)
        {
            _context = context;
            _emailService = emailService;
            _env = env;    
        }


        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Students = _context.Students.Include(s => s.Parent).ThenInclude(p => p.User).ToList();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Guid studentId, string term, string comments, List<string> subjects, List<int> scores)
        {
            var student = await _context.Students
                .Include(s => s.Parent).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.StudentID == studentId);

            if (student == null)
                return Json(new { success = false, message = "Student not found." });

            var report = new ReportCard
            {
                ReportID = Guid.NewGuid(),
                StudentID = studentId,
                Term = term,
                Comments = comments,
                UploadDate = DateTime.Now
            };


            _context.ReportCards.Add(report);
            await _context.SaveChangesAsync();

            for (int i = 0; i < subjects.Count; i++)
            {
                _context.ReportSubjects.Add(new ReportSubject
                {
                    SubjectID = Guid.NewGuid(),
                    ReportID = report.ReportID,
                    SubjectName = subjects[i],
                    Score = scores[i],
                    Grade = GetGrade(scores[i])
                });
            }
            await _context.SaveChangesAsync();

            string pdfPath = await GenerateReportPDF(report, student);
            report.FilePath = pdfPath;
            await _context.SaveChangesAsync();

            if (student.Parent?.User?.Email != null)
            {
                await _emailService.SendEmailWithAttachmentAsync(
                    student.Parent.User.Email,
                    $"📘 {student.FullName}'s Report Card - {term}",
                    $"Dear Parent,<br><br>Please find attached {student.FullName}'s {term} report card.<br><br>Best regards,<br>EduClockPlus",
                    pdfPath
                );
            }
            return Json(new { success = true, message = "Report card generated and sent to parent." });
        }


        private string GetGrade(int score)
        {
            if (score >= 70) return "A";
            if (score >= 60) return "B";
            if (score >= 50) return "C";
            if (score >= 40) return "D";
            return "F";
        }
        

        private async Task<string> GenerateReportPDF(ReportCard report, Student student)
        {
            var subjects = await _context.ReportSubjects
                .Where(r => r.ReportID == report.ReportID)
                .ToListAsync();

            var reportsDir = Path.Combine(_env.WebRootPath, "reports");
            Directory.CreateDirectory(reportsDir);

            string safeName = string.Concat(student.FullName.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(reportsDir, $"{safeName}_{report.Term}.pdf");

            var writerProps = new WriterProperties();
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var writer = new PdfWriter(stream, writerProps))
            using (var pdf = new PdfDocument(writer))
            using (var doc = new Document(pdf))

            {
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                doc.Add(new Paragraph($"Report Card - {report.Term}")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER));

                doc.Add(new Paragraph($"Student: {student.FullName}").SetFont(normalFont));
                doc.Add(new Paragraph($"Date: {DateTime.Now:MMMM dd, yyyy}").SetFont(normalFont));
                doc.Add(new Paragraph(" "));

                var table = new Table(3).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().Add(new Paragraph("Subject").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Score").SetFont(boldFont)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Grade").SetFont(boldFont)));

                foreach (var s in subjects)
                {
                    table.AddCell(new Cell().Add(new Paragraph(s.SubjectName)));
                    table.AddCell(new Cell().Add(new Paragraph(s.Score.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(s.Grade)));
                }

                doc.Add(table);
                doc.Add(new Paragraph(" "));
                doc.Add(new Paragraph($"Teacher Comments: {report.Comments ?? "N/A"}").SetFont(normalFont));
            }

            if (!System.IO.File.Exists(filePath))
                throw new Exception($"Report file not found after generation: {filePath}");

            Console.WriteLine($"✅ PDF successfully generated at: {filePath}");
            return filePath;

        }
    }
}
