using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineEnrolmentSystem.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }   // Primary Key

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public required string Email { get; set; }    // Must be unique (configure in DbContext)

        [Required]
        [Phone]
        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public required string Address { get; set; }

        [Required]
        public required string PasswordHash { get; set; }  // Store hashed password, not plain text

        // Auto-generated timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
