using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Models;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace OnlineEnrolmentSystem.Pages.Payments
{
    public class HistoryModel : PageModel
    {
        private readonly StudentDBContext _context;
        public HistoryModel(StudentDBContext context) => _context = context;

        // Filters (bind from query)
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Export { get; set; } = false;

        public List<Payment> Payments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var query = _context.Payments
                .Include(p => p.Student)
                .Where(p => p.StudentId == studentId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                query = query.Where(p => p.Description.Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(Status))
                query = query.Where(p => p.Status == Status);

            if (StartDate.HasValue)
            {
                var sd = StartDate.Value.Date;
                query = query.Where(p => p.PaymentDate >= sd);
            }

            if (EndDate.HasValue)
            {
                var ed = EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.PaymentDate <= ed);
            }

            query = query.OrderByDescending(p => p.PaymentDate);

            var results = await query.ToListAsync();

            if (Export)
                return ExportToCsv(results);

            Payments = results;
            return Page();
        }

        private IActionResult ExportToCsv(List<Payment> payments)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PaymentId,Description,Amount,PaymentMethod,Status,ReceiptNumber,PaymentDate");

            foreach (var p in payments)
            {
                var desc = (p.Description ?? "").Replace(",", " ");
                var line = string.Join(",",
                    p.PaymentId,
                    EscapeCsv(desc),
                    p.Amount.ToString("F2", CultureInfo.InvariantCulture),
                    EscapeCsv(p.PaymentMethod),
                    EscapeCsv(p.Status),
                    EscapeCsv(p.ReceiptNumber),
                    EscapeCsv(p.PaymentDate.ToString("dd MMM yyyy, HH:mm"))
                );
                sb.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var filename = $"payments_{DateTime.Now:yyyyMMdd}.csv";
            return File(bytes, "text/csv", filename);
        }

        private string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
