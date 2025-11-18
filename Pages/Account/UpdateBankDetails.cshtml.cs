using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnlineEnrolmentSystem.Pages.Account
{
    public class UpdateBankDetailsModel : PageModel
    {
        private readonly StudentDBContext _context;

        public UpdateBankDetailsModel(StudentDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public BankDetailInput Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existing = await _context.BankDetails
                .FirstOrDefaultAsync(b => b.StudentId == studentId);

            if (existing != null)
            {
                Input = new BankDetailInput
                {
                    BankName = existing.BankName,
                    BankAccountNumber = existing.BankAccountNumber,
                    BankHolderName = existing.BankHolderName
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existing = await _context.BankDetails
                .FirstOrDefaultAsync(b => b.StudentId == studentId);

            if (existing != null)
            {
                // Update
                existing.BankName = Input.BankName;
                existing.BankAccountNumber = Input.BankAccountNumber;
                existing.BankHolderName = Input.BankHolderName;
            }
            else
            {
                // Create new
                var newBankDetail = new BankDetail
                {
                    BankName = Input.BankName,
                    BankAccountNumber = Input.BankAccountNumber,
                    BankHolderName = Input.BankHolderName,
                    StudentId = studentId
                };
                _context.BankDetails.Add(newBankDetail);
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Bank details updated successfully!";
            return RedirectToPage();
        }

        public class BankDetailInput
        {
            [Required, StringLength(100)]
            [Display(Name = "Bank Name")]
            public string BankName { get; set; } = string.Empty;

            [Required, StringLength(50)]
            [Display(Name = "Bank Account Number")]
            public string BankAccountNumber { get; set; } = string.Empty;

            [Required, StringLength(100)]
            [Display(Name = "Account Holder Name")]
            public string BankHolderName { get; set; } = string.Empty;
        }
    }
}
