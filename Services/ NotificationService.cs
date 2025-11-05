using EduClockPlus.Services; // ✅ Make sure this matches your actual namespace
using System.Threading.Tasks;

namespace EduClockPlus.Services // ❌ It was ClassClockPlus before — make sure it’s the same as the rest of your project
{
    public class NotificationService
    {
        private readonly EmailService _emailService;

        public NotificationService(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task SendAttendanceNotification(string parentEmail, string studentName, string status, string teacherName)
        {
            string subject = $"Attendance Alert: {studentName}";
            string body = $"Hello, your child <b>{studentName}</b> has been <b>{status}</b> by <b>{teacherName}</b>.";

            await _emailService.SendEmailAsync(parentEmail, subject, body);
        }
    }
}
