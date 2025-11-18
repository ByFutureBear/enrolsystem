using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEnrolmentSystem.Models
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }   // Primary Key

        // Foreign key to Course
        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        [Required]
        [StringLength(10)]
        public required string Section { get; set; }   // e.g., "A", "B1"

        [Required]
        public int Capacity { get; set; }   // Max number of students

        [Required]
        [StringLength(50)]
        public required string Schedule { get; set; }   // e.g., "Mon 10-12"
    }
}
