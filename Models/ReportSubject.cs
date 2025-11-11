using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int Score { get; set; } // e.g. 85
        public string Grade { get; set; } = default!; // A, B, C, etc.
    }
}
