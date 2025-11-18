using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineEnrolmentSystem.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnlineEnrolmentSystem.Pages.Account
{
    public class UpdateProfileModel : PageModel
    {
        private readonly StudentDBContext _context;

        public UpdateProfileModel(StudentDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ProfileInput Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return NotFound();

            Input = new ProfileInput
            {
                Name = student.Name,
                PhoneNumber = student.PhoneNumber,
                Address = student.Address
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Update fields
            student.Name = Input.Name;
            student.PhoneNumber = Input.PhoneNumber;
            student.Address = Input.Address;

            await _context.SaveChangesAsync();

            // Claim new session info
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, student.StudentId.ToString()),
                new Claim(ClaimTypes.Name, student.Name),
                new Claim(ClaimTypes.Email, student.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });


            StatusMessage = "Profile updated successfully!";
            return RedirectToPage();
        }

        public class ProfileInput
        {
            [Required, StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required, Phone, StringLength(20)]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required, StringLength(200)]
            public string Address { get; set; } = string.Empty;
        }
    }
}
