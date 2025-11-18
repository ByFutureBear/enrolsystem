using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEnrolmentSystem.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }   // Primary Key

        // FK → Students
        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        // FK → Classes
        [Required]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public Class Class { get; set; }

        // Auto-generated enrolment timestamp
        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        // Enrolment status (required)
        [Required]
        [StringLength(20)]
        public required string Status { get; set; }   // e.g., "Enroled", "Completed"
    }
}
