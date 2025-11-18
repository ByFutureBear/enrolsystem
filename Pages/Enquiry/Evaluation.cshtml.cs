using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


namespace OnlineEnrolmentSystem.Pages.Enquiry
{
    public class EvaluationModel : PageModel
    {
        private readonly StudentDBContext _context;

        public EvaluationModel(StudentDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EvaluationInput Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public List<Course> AvailableCourses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get student's enroled course IDs
            var enroledCourseIds = await _context.Enrolments
                .Where(e => e.StudentId == studentId && e.Status == "Enroled")
                .Include(e => e.Class)
                .Select(e => e.Class.CourseId)
                .Distinct()
                .ToListAsync();

            // Get already evaluated course IDs by this student
            var evaluatedCourseIds = await _context.Evaluations
                .Where(ev => ev.StudentId == studentId)
                .Select(ev => ev.CourseId)
                .ToListAsync();

            // Filter courses: enroled but not yet evaluated
            AvailableCourses = await _context.Courses
                .Where(c => enroledCourseIds.Contains(c.CourseId) &&
                            !evaluatedCourseIds.Contains(c.CourseId))
                .OrderBy(c => c.CourseName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // reload available courses
                return Page();
            }

            // Double-check: prevent duplicate evaluation
            var alreadyEvaluated = await _context.Evaluations
                .AnyAsync(ev => ev.StudentId == studentId && ev.CourseId == Input.CourseId);

            if (alreadyEvaluated)
            {
                ModelState.AddModelError("", "You have already evaluated this course.");
                await OnGetAsync();
                return Page();
            }

            var evaluation = new Evaluation
            {
                StudentId = studentId,
                CourseId = Input.CourseId,
                Rating = Input.Rating,
                Comments = Input.Comments
            };

            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();

            StatusMessage = "Thank you! Your evaluation has been submitted.";
            return RedirectToPage();
        }

        public class EvaluationInput
        {
            [Required]
            [Display(Name = "Course")]
            public int CourseId { get; set; }

            [Required, Range(1, 5)]
            [Display(Name = "Rating (1=Poor, 5=Excellent)")]
            public int Rating { get; set; }

            [StringLength(500)]
            public string? Comments { get; set; }
        }
    }
}