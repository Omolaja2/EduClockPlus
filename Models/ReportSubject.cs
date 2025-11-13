using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduClockPlus.Models;

namespace ClassClockPlus.Models
{
    public class ReportSubject
    {
        [Key]
        public Guid SubjectID { get; set; }

        [ForeignKey("ReportCard")]
        public Guid ReportID { get; set; }
        public ReportCard? ReportCard { get; set; }
        public string SubjectName { get; set; } = default!;
        public int Score { get; set; }
        public string Grade { get; set; } = default!;
        public int SchoolId { get; set; }
        public School? School { get; set; }
    }
}
