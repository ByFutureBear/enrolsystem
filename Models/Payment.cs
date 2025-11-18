using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEnrolmentSystem.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }   // Primary Key

        // FK → Students
        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public required string PaymentMethod { get; set; }   // "CreditCard", "OnlineBanking", "Ewallet"

        [Required]
        [StringLength(50)]
        public required string ReceiptNumber { get; set; }

        [Required]
        [StringLength(20)]
        public required string Status { get; set; }   // "Paid", "Pending", "Failed"

        [Required]
        [StringLength(200)]
        public required string Description { get; set; }

        // Optional fields: depending on user's payment method
        [StringLength(50)]
        public string? CardName { get; set; }

        [StringLength(20)]
        public string? CardNumber { get; set; }

        [StringLength(10)]
        public string? CardExpiry { get; set; }

        [StringLength(5)]
        public string? CardCVV { get; set; }

        [StringLength(50)]
        public string? BankAccount { get; set; }

        [StringLength(20)]
        public string? EwalletProvider { get; set; }

    }
}
