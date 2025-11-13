using System.ComponentModel.DataAnnotations;

namespace ClassClockPlus.Models
{
    public class Notification
    {
        [Key]
        public Guid NotificationID { get; set; }
        public Guid ParentID { get; set; }
        public Parent? Parent { get; set; }
        public string? Message { get; set; }
        public string Type { get; set; } = default!;
        public DateTime SentAt { get; set; }
    }
}
