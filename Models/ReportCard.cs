using System.ComponentModel.DataAnnotations;

namespace ClassClockPlus.Models
{
    public class ReportCard
    {
        [Key]
        public Guid ReportID { get; set; }

        public Guid StudentID { get; set; }
        public Student? Student { get; set; }

        public string Term { get; set; } = default!;
        public string? FilePath { get; set; }
        public string? Comments { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
