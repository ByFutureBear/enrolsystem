using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Models;

namespace OnlineEnrolmentSystem.Data
{
    public class StudentDBContext : DbContext
    {
        public StudentDBContext(DbContextOptions<StudentDBContext> options)
            : base(options) { }

        // Maps to SQL Table
        public DbSet<Student> Students { get; set; }
        public DbSet<BankDetail> BankDetails { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<AddDropHistory> AddDropHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure Email is unique
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();

            // Self-referencing relationship for Course prerequisites
            modelBuilder.Entity<Course>()
                .HasOne(c => c.PrerequisiteCourse)
                .WithMany()
                .HasForeignKey(c => c.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict); // prevent cascade delete loops

            // Class ↔ Course relationship (Many Classes per Course)
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Each Enrolment → belongs to 1 Student
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Each Enrolment → belongs to 1 Class
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Class)
                .WithMany()
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment ↔ Student (many payments per student)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Invoice ↔ Student (many invoices per student)
            modelBuilder.Entity<Invoice>()
                .HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // History ↔ Student
            modelBuilder.Entity<AddDropHistory>()
                .HasOne(h => h.Student)
                .WithMany()
                .HasForeignKey(h => h.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // History ↔ Class
            modelBuilder.Entity<AddDropHistory>()
                .HasOne(h => h.Class)
                .WithMany()
                .HasForeignKey(h => h.ClassId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
