using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Models;
using System.Security.Claims;

namespace OnlineEnrolmentSystem.Pages.Enrol.AddDrop
{
    public class HistoryModel : PageModel
    {
        private readonly StudentDBContext _context;

        public HistoryModel(StudentDBContext context)
        {
            _context = context;
        }

        public List<AddDropHistory> HistoryRecords { get; set; } = new();

        public async Task OnGetAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            HistoryRecords = await _context.AddDropHistories
                .Include(h => h.Class)
                .ThenInclude(c => c.Course)
                .Where(h => h.StudentId == studentId)
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();
        }
    }
}
