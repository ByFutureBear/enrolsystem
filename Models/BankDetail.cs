using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEnrolmentSystem.Models
{
    public class BankDetail
    {
        [Key]
        public int BankDetailId { get; set; }   // Primary Key

        [Required]
        [StringLength(100)]
        public required string BankName { get; set; }

        [Required]
        [StringLength(30)]
        public required string BankAccountNumber { get; set; }

        [Required]
        [StringLength(100)]
        public required string BankHolderName { get; set; }

        // Foreign key to Student
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }
    }
}
