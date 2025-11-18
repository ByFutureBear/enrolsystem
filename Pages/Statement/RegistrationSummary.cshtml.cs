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
    public class RegistrationSummaryModel : PageModel
    {
        private readonly StudentDBContext _context;

        public RegistrationSummaryModel(StudentDBContext context)
        {
            _context = context;
        }

        public Student? StudentInfo { get; set; }
        public List<Enrolment> Enrolments { get; set; } = new();

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

                    page.Header().Text("Registration Summary / Class Timetable").FontSize(20).Bold().AlignCenter();
                    page.Content().Column(col =>
                    {
                        // Student Info
                        col.Item().Text($"Name: {StudentInfo?.Name}");
                        col.Item().Text($"Email: {StudentInfo?.Email}");
                        col.Item().Text($"Student ID: {StudentInfo?.StudentId}");
                        col.Item().Text($"Date: {DateTime.Now:dd/MM/yyyy}");
                        col.Item().LineHorizontal(1);

                        // Enrolments
                        col.Item().Text("Enroled Classes:").FontSize(14).Bold();
                        if (Enrolments.Any())
                        {
                            foreach (var e in Enrolments)
                            {
                                col.Item().Text(
                                    $"{e.Class.Course.CourseCode} - {e.Class.Course.CourseName}"
                                );
                                col.Item().Text(
                                    $"Section {e.Class.Section} | Credit Hours: {e.Class.Course.Credits} | Schedule： {e.Class.Schedule}"
                                );
                                col.Item().Text(
                                    $"---"
                                );
                            }
                        }
                        else
                        {
                            col.Item().Text("No classes enroled.");
                        }

                        // Timetable Grid
                        col.Item().Text("Weekly Timetable:").FontSize(14).Bold();

                        var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" };

                        col.Item().Table(table =>
                        {
                            // Table header
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80); // Time column
                                foreach (var _ in days)
                                    columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Time").Bold();
                                foreach (var d in days)
                                    header.Cell().Text(d).Bold();
                            });

                            // Time slots (08:00 - 23:00, step 2 hours)
                            for (int hour = 8; hour < 23; hour += 2)
                            {
                                var slotStart = new TimeSpan(hour, 0, 0);
                                var slotEnd = new TimeSpan(hour + 2, 0, 0);

                                table.Cell().Text($"{slotStart:hh\\:mm}-{slotEnd:hh\\:mm}");

                                foreach (var day in days)
                                {
                                    var classInSlot = Enrolments
                                        .Where(e => IsClassInSlot(e.Class.Schedule, day, slotStart, slotEnd))
                                        .FirstOrDefault();

                                    if (classInSlot != null)
                                    {
                                        table.Cell().Text($"{classInSlot.Class.Course.CourseCode}\nSec {classInSlot.Class.Section}");
                                    }
                                    else
                                    {
                                        table.Cell().Text("");
                                    }
                                }
                            }
                        });

                    });
                });
            });

            // Generate PDF file & download
            var pdfFile = pdf.GeneratePdf();
            return File(pdfFile, "application/pdf", "RegistrationSummary.pdf");
        }

        private async Task LoadDataAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Load student info
            StudentInfo = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            // Enrolments
            Enrolments = await _context.Enrolments
                .Include(e => e.Class)
                .ThenInclude(c => c.Course)
                .Where(e => e.StudentId == studentId && e.Status == "Enroled")
                .ToListAsync();
        }

        // Helper: check if class fits in given day/time slot
        private bool IsClassInSlot(string schedule, string day, TimeSpan slotStart, TimeSpan slotEnd)
        {
            if (string.IsNullOrEmpty(schedule)) return false;

            var parts = schedule.Split(' ');
            if (parts.Length != 2) return false;

            var classDay = parts[0];
            if (!classDay.Equals(day, StringComparison.OrdinalIgnoreCase))
                return false;

            var times = parts[1].Split('-');
            if (times.Length != 2) return false;

            var start = TimeSpan.Parse(times[0]);
            var end = TimeSpan.Parse(times[1]);

            // Overlap check
            return start < slotEnd && end > slotStart;
        }
    }
}
