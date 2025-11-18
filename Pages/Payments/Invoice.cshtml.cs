using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Models;
using System.Security.Claims;

namespace OnlineEnrolmentSystem.Pages.Payments
{
    public class InvoiceModel : PageModel
    {
        private readonly StudentDBContext _context;

        public InvoiceModel(StudentDBContext context)
        {
            _context = context;
        }

        public IList<Invoice> Invoices { get; set; } = new List<Invoice>();

        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Load invoice history
            Invoices = await _context.Invoices
                .Where(i => i.StudentId == studentId)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return Page();
        }
    }
}
