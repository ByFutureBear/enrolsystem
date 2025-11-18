using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Models;
using System.Security.Claims;

namespace OnlineEnrolmentSystem.Pages.Payments
{
    public class PayModel : PageModel
    {
        private readonly StudentDBContext _context;

        public PayModel(StudentDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Payment Payment { get; set; }

        public Invoice? Invoice { get; set; }
        public decimal Outstanding { get; set; }

        public async Task<IActionResult> OnGetAsync(int? invoiceId)
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Step 1: Get invoice
            if (invoiceId.HasValue)
            {
                Invoice = await _context.Invoices
                    .Include(i => i.Student)
                    .Where(i => i.StudentId == studentId)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId.Value);
            }
            else
            {
                Invoice = await _context.Invoices
                    .Include(i => i.Student)
                    .Where(i => i.StudentId == studentId && i.Status != "Paid")
                    .OrderBy(i => i.IssueDate)
                    .FirstOrDefaultAsync();
            }

            if (Invoice == null)
            {
                return Page();
            }

            // Step 2: Calculate outstanding
            var totalPaid = await _context.Payments
                .Where(p => p.StudentId == Invoice.StudentId && p.Description == Invoice.InvoiceNumber)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            Outstanding = (Invoice.TotalAmount - Invoice.Adjustment) - totalPaid;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int invoiceId)
        {
            Invoice = await _context.Invoices.FindAsync(invoiceId);

            if (Invoice == null)
            {
                return Page();
            }

            // Calculate outstanding again to validate
            var totalPaid = await _context.Payments
                .Where(p => p.StudentId == Invoice.StudentId && p.Description == Invoice.InvoiceNumber)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            Outstanding = (Invoice.TotalAmount - Invoice.Adjustment) - totalPaid;

            if (Outstanding <= 0)
            {
                return Page();
            }

            // Save payment data
            Payment.StudentId = Invoice.StudentId;
            Payment.Amount = Outstanding;
            Payment.Status = "Paid";
            Payment.Description = Invoice.InvoiceNumber;
            Payment.ReceiptNumber = $"RCPT-{System.Guid.NewGuid().ToString().Substring(0, 8)}";

            _context.Payments.Add(Payment);

            // Update invoice status if fully paid
            if (Outstanding > 0)
            {
                Invoice.Status = "Paid";
            }

            await _context.SaveChangesAsync();

            TempData["PaymentSuccess"] = "Payment successful!";
            await OnGetAsync(null);
            return Page();
        }
    }
}
