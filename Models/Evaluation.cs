using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEnrolmentSystem.Models
{
    public class Evaluation
    {
        [Key]
        public int EvaluationId { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; } = default!;

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; } = default!;

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
