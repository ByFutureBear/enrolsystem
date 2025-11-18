using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEnrolmentSystem.Models
{
    public class AddDropHistory
    {
        [Key]
        public int HistoryId { get; set; }   // Primary Key

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

        [Required]
        [StringLength(10)]
        public required string Action { get; set; }   // "Add" or "Drop"

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;  // Auto-generated
    }
}
