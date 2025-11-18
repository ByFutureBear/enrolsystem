using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEnrolmentSystem.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }   // Primary Key

        [Required]
        [StringLength(20)]
        public required string CourseCode { get; set; }

        [Required]
        [StringLength(200)]
        public required string CourseName { get; set; }

        [Required]
        public int Credits { get; set; }

        // Self-referencing foreign key (nullable)
        public int? PrerequisiteCourseId { get; set; }

        [ForeignKey("PrerequisiteCourseId")]
        public Course? PrerequisiteCourse { get; set; }
    }
}
