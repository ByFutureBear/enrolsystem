using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Models;
using System.Security.Claims;

namespace OnlineEnrolmentSystem.Pages.Enrol
{
    public class IndexModel : PageModel
    {
        private readonly StudentDBContext _context;

        public IndexModel(StudentDBContext context)
        {
            _context = context;
        }

        // List of student's current enrolments
        public List<Enrolment> MyEnrolments { get; set; } = new();

        // List of available classes
        public List<Class> AvailableClasses { get; set; } = new();

        [BindProperty]
        public int SelectedClassId { get; set; }

        public async Task OnGetAsync()
        {
            // Get current student id (from login identity or session)
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Load student's current enrolments
            MyEnrolments = await _context.Enrolments
                .Include(e => e.Class)
                .ThenInclude(c => c.Course)
                .Where(e => e.StudentId == studentId && e.Status == "Enroled")
                .ToListAsync();

            // Find courses the student has already completed
            var completedCourseIds = await _context.Enrolments
                .Where(e => e.StudentId == studentId && e.Status == "Completed")
                .Select(e => e.Class.Course.CourseId)
                .Distinct()
                .ToListAsync();

            // Find courses the student is currently enroled in
            var enroledCourseIds = await _context.Enrolments
                .Where(e => e.StudentId == studentId && e.Status == "Enroled")
                .Select(e => e.Class.Course.CourseId)
                .Distinct()
                .ToListAsync();

            // Combine both lists into one "blocked" set
            var blockedCourseIds = completedCourseIds
                .Concat(enroledCourseIds)
                .Distinct()
                .ToHashSet();

            // Load available classes, excluding blocked courses
            AvailableClasses = await _context.Classes
                .Include(c => c.Course)
                .Where(c => !blockedCourseIds.Contains(c.CourseId))
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostEnrolAsync()
        {
            if (SelectedClassId == 0)
            {
                ModelState.AddModelError("", "Please select a class to enrol.");
                return await ReloadPage();
            }

            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Load the class (with Course + prerequisite)
            var selectedClass = await _context.Classes
                .Include(c => c.Course)
                .ThenInclude(c => c.PrerequisiteCourse)
                .FirstOrDefaultAsync(c => c.ClassId == SelectedClassId);

            if (selectedClass == null)
            {
                ModelState.AddModelError("", "Selected class not found.");
                return await ReloadPage();
            }

            // Prevent enrol completed course
            var alreadyCompleted = await _context.Enrolments
                .AnyAsync(e => e.StudentId == studentId
                            && e.Class.Course.CourseId == selectedClass.CourseId
                            && e.Status == "Completed");

            if (alreadyCompleted)
            {
                ModelState.AddModelError("", "You have completed this course.");
                return await ReloadPage();
            }

            // Prevent duplicate enrolment
            var alreadyEnroled = await _context.Enrolments
                .AnyAsync(e => e.StudentId == studentId
                            && e.ClassId == SelectedClassId
                            && e.Status == "Enroled");

            if (alreadyEnroled)
            {
                ModelState.AddModelError("", "You are already enroled in this class.");
                return await ReloadPage();
            }

            // Prevent duplicate enrolment in same course (even if different class)
            var alreadyInCourse = await _context.Enrolments
                .AnyAsync(e => e.StudentId == studentId
                            && e.Class.Course.CourseId == selectedClass.CourseId
                            && e.Status == "Enroled");

            if (alreadyInCourse)
            {
                ModelState.AddModelError("", $"You are already enroled in {selectedClass.Course.CourseCode} - {selectedClass.Course.CourseName} (another section).");
                return await ReloadPage();
            }

            // Capacity check
            var enroledCount = await _context.Enrolments
                .CountAsync(e => e.ClassId == SelectedClassId && e.Status == "Enroled");

            if (enroledCount >= selectedClass.Capacity)
            {
                ModelState.AddModelError("", "This class is already full.");
                return await ReloadPage();
            }

            // Prerequisite check
            if (selectedClass.Course.PrerequisiteCourseId != null)
            {
                var prereqId = selectedClass.Course.PrerequisiteCourseId.Value;

                // Check if student has an enrolment in the prerequisite course with Completed status
                bool hasCompletedPrereq = await _context.Enrolments
                    .AnyAsync(e =>
                        e.StudentId == studentId &&
                        e.Class.Course.CourseId == prereqId &&
                        e.Status == "Completed");

                if (!hasCompletedPrereq)
                {
                    ModelState.AddModelError("", $"You must complete the prerequisite course before enroling in {selectedClass.Course.CourseCode}.");
                    return await ReloadPage();
                }
            }

            // All checks passed, create enrolment
            var Enrolment = new Enrolment
            {
                StudentId = studentId,
                ClassId = SelectedClassId,
                EnrolmentDate = DateTime.Now,
                Status = "Enroled"
            };

            _context.Enrolments.Add(Enrolment);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        // Helper to reload data on failed Post
        private async Task<IActionResult> ReloadPage()
        {
            await OnGetAsync();
            return Page();
        }
    }
}
