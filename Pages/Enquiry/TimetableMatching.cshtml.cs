using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineEnrolmentSystem.Pages.Enquiry
{
    public class TimetableMatchingModel : PageModel
    {
        private readonly StudentDBContext _context;

        public TimetableMatchingModel(StudentDBContext context)
        {
            _context = context;
        }

        // All available classes
        public List<Class> AllClasses { get; set; } = new();

        // Selected classes for checking
        [BindProperty]
        [Required(ErrorMessage = "Please select at least one class.")]
        public List<int> SelectedClassIds { get; set; } = new();

        public string? MatchResult { get; set; }

        public async Task OnGetAsync()
        {
            AllClasses = await _context.Classes
                .Include(c => c.Course)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCheckAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            if (SelectedClassIds.Count == 0)
            {
                MatchResult = "Please select at least 1 course.";
                await OnGetAsync();
                return Page();
            }

            var selectedClasses = await _context.Classes
                .Include(c => c.Course)
                .Where(c => SelectedClassIds.Contains(c.ClassId))
                .ToListAsync();

            // Check for conflicts
            var conflicts = FindConflicts(selectedClasses);

            if (conflicts.Count == 0)
            {
                MatchResult = "No timetable conflicts found!";
            }
            else
            {
                MatchResult = "Conflicts found:\n" + string.Join("\n", conflicts);
            }

            await OnGetAsync(); // reload classes for redisplay
            return Page();
        }

        private List<string> FindConflicts(List<Class> classes)
        {
            var conflicts = new List<string>();

            for (int i = 0; i < classes.Count; i++)
            {
                for (int j = i + 1; j < classes.Count; j++)
                {
                    if (IsOverlap(classes[i].Schedule, classes[j].Schedule))
                    {
                        conflicts.Add($"{classes[i].Course.CourseCode} ({classes[i].Schedule}) conflicts with {classes[j].Course.CourseCode} ({classes[j].Schedule})");
                    }
                }
            }

            return conflicts;
        }

        // Overlap checker — schedule format "Mon 09:00-11:00"
        private bool IsOverlap(string schedule1, string schedule2)
        {
            if (string.IsNullOrEmpty(schedule1) || string.IsNullOrEmpty(schedule2))
                return false;

            var parts1 = schedule1.Split(' ');
            var parts2 = schedule2.Split(' ');

            if (parts1[0] != parts2[0]) // different day
                return false;

            var time1 = parts1[1].Split('-');
            var time2 = parts2[1].Split('-');

            var start1 = TimeSpan.Parse(time1[0]);
            var end1 = TimeSpan.Parse(time1[1]);

            var start2 = TimeSpan.Parse(time2[0]);
            var end2 = TimeSpan.Parse(time2[1]);

            return start1 < end2 && start2 < end1;
        }
    }
}
