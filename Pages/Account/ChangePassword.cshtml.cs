using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Security;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnlineEnrolmentSystem.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly StudentDBContext _context;

        public ChangePasswordModel(StudentDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ChangePasswordInput Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return NotFound();

            // Verify old password
            if (!PasswordHasher.Verify(student.PasswordHash, Input.OldPassword))
            {
                ModelState.AddModelError("Input.OldPassword", "Old password is incorrect.");
                return Page();
            }

            // Confirm new password match
            if (Input.NewPassword != Input.ConfirmPassword)
            {
                ModelState.AddModelError("Input.ConfirmPassword", "Passwords do not match.");
                return Page();
            }

            // Update with new hash
            student.PasswordHash = PasswordHasher.Hash(Input.NewPassword);
            await _context.SaveChangesAsync();

            // Force logout after password change
            await HttpContext.SignOutAsync();

            // Redirect to login with message
            TempData["StatusMessage"] = "Password changed successfully. Please log in again.";
            return RedirectToPage("/Login");
        }

        public class ChangePasswordInput
        {
            [Required, DataType(DataType.Password)]
            [Display(Name = "Old Password")]
            public string OldPassword { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
                ErrorMessage = "Password doesn't meet the requirements.")]
            [Display(Name = "New Password")]
            public string NewPassword { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            [Display(Name = "Confirm New Password")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
