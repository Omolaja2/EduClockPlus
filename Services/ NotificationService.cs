using EduClockPlus.Services; 
using System.Threading.Tasks;

namespace EduClockPlus.Services 
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
