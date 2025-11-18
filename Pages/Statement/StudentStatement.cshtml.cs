using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace OnlineEnrolmentSystem.Pages.Statement
{
    public class StudentStatementModel : PageModel
    {
        private readonly StudentDBContext _context;

        public StudentStatementModel(StudentDBContext context)
        {
            _context = context;
        }

        public Student? StudentInfo { get; set; }
        public List<Enrolment> Enrolments { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public decimal TotalPaid { get; set; }
        public decimal TotalFees { get; set; }
        public decimal OutstandingBalance { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostDownloadPdfAsync()
        {
            await LoadDataAsync();

            // Set QuestPDF license
            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);

                    page.Header().Text("Student Statement").FontSize(20).Bold().AlignCenter();
                    page.Content().Column(col =>
                    {
                        // Student Info
                        col.Item().Text($"Name: {StudentInfo?.Name}");
                        col.Item().Text($"Email: {StudentInfo?.Email}");
                        col.Item().Text($"Student ID: {StudentInfo?.StudentId}");
                        col.Item().Text($"Date: {DateTime.Now:dd/MM/yyyy}");
                        col.Item().LineHorizontal(1);

                        // Enrolments
                        col.Item().Text("Enrolments:").FontSize(14).Bold();
                        if (Enrolments.Any())
                        {
                            foreach (var e in Enrolments)
                            {
                                col.Item().Text(
                                    $"{e.Class.Course.CourseCode} - {e.Class.Course.CourseName} ({e.Class.Section}) | Credits: {e.Class.Course.Credits}"
                                );
                            }
                        }
                        else
                        {
                            col.Item().Text("No Enrolments found.");
                        }

                        col.Item().LineHorizontal(1);

                        // Payments
                        col.Item().Text("Payments:").FontSize(14).Bold();
                        if (Payments.Any())
                        {
                            foreach (var p in Payments)
                            {
                                col.Item().Text(
                                    $"{p.PaymentDate:dd/MM/yyyy} | {p.Amount:C} | {p.Status} | {p.PaymentMethod}"
                                );
                            }
                        }
                        else
                        {
                            col.Item().Text("No payments found.");
                        }

                        col.Item().LineHorizontal(1);

                        // Summary
                        col.Item().Text($"Total Fees: {TotalFees:C}");
                        col.Item().Text($"Total Paid: {TotalPaid:C}");
                        col.Item().Text($"Outstanding Balance: {OutstandingBalance:C}");
                    });
                });
            });

            // Generate PDF file & download
            var pdfFile = pdf.GeneratePdf();
            return File(pdfFile, "application/pdf", "StudentStatement.pdf");
        }

        private async Task LoadDataAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Load student info
            StudentInfo = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            // Enrolments with filtering (start & end date)
            var enrolQuery = _context.Enrolments
                .Include(e => e.Class)
                .ThenInclude(c => c.Course)
                .Where(e => e.StudentId == studentId && e.Status == "Enroled");

            if (StartDate.HasValue)
                enrolQuery = enrolQuery.Where(e => e.EnrolmentDate >= StartDate.Value);

            if (EndDate.HasValue)
                enrolQuery = enrolQuery.Where(e => e.EnrolmentDate <= EndDate.Value);

            Enrolments = await enrolQuery.ToListAsync();

            // Payments with filtering (start & end date)
            var payQuery = _context.Payments
                .Where(p => p.StudentId == studentId);

            if (StartDate.HasValue)
                payQuery = payQuery.Where(p => p.PaymentDate >= StartDate.Value);

            if (EndDate.HasValue)
                payQuery = payQuery.Where(p => p.PaymentDate <= EndDate.Value);

            Payments = await payQuery
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Invoices with filtering (start & end date)
            var invoiceQuery = _context.Invoices
                .Where(p => p.StudentId == studentId);

            if (StartDate.HasValue)
                invoiceQuery = invoiceQuery.Where(p => p.IssueDate >= StartDate.Value);

            if (EndDate.HasValue)
                invoiceQuery = invoiceQuery.Where(p => p.IssueDate <= EndDate.Value);

            var Invoices = await invoiceQuery
                .OrderByDescending(p => p.IssueDate)
                .ToListAsync();

            // Calculate total fees from invoices
            TotalFees = Invoices.Sum(e => e.TotalAmount - e.Adjustment);

            TotalPaid = Payments
                .Where(p => p.Status == "Paid")
                .Sum(p => p.Amount);

            OutstandingBalance = TotalFees - TotalPaid;
        }
    }
}
