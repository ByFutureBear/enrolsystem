using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineEnrolmentSystem.Data;
using OnlineEnrolmentSystem.Security;
using Microsoft.AspNetCore.Authorization;

namespace OnlineEnrolmentSystem.Pages
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly StudentDBContext _context;

        public ForgotPasswordModel(StudentDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ForgotPasswordInput Input { get; set; } = new();

        public string GeneratedPassword { get; set; } = string.Empty;
        public string InfoMessage { get; set; } = string.Empty;

        public class ForgotPasswordInput
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, Display(Name = "Phone Number")]
            [RegularExpression(@"^[0-9+\-()\s]{7,20}$", ErrorMessage = "Invalid phone number.")]
            public string PhoneNumber { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var email = Input.Email.Trim().ToLower();
            var phone = Input.PhoneNumber.Trim();

            var student = _context.Students.FirstOrDefault(s => s.Email.ToLower() == email && s.PhoneNumber == phone);
            if (student is null)
            {
                ModelState.AddModelError(string.Empty, "No matching account found. Check your email and phone number.");
                return Page();
            }

            GeneratedPassword = GenerateTempPassword();
            student.PasswordHash = PasswordHasher.Hash(GeneratedPassword);
            await _context.SaveChangesAsync();

            return Page();
        }

        private static string GenerateTempPassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string symbols = "!@#$%^&*()_-+=[]{}:;',.<>?/\\|`~";

            static string Pick(string chars, int n)
            {
                using var rng = RandomNumberGenerator.Create();
                var buf = new char[n];
                var bytes = new byte[n];
                rng.GetBytes(bytes);
                for (int i = 0; i < n; i++) buf[i] = chars[bytes[i] % chars.Length];
                return new string(buf);
            }

            var parts = (Pick(upper, 3) + Pick(lower, 3) + Pick(digits, 3) + Pick(symbols, 3)).ToCharArray();

            using var rng2 = RandomNumberGenerator.Create();
            var shuf = new byte[parts.Length];
            rng2.GetBytes(shuf);
            for (int i = 0; i < parts.Length; i++)
            {
                int j = shuf[i] % parts.Length;
                (parts[i], parts[j]) = (parts[j], parts[i]);
            }
            return new string(parts);
        }
    }
}
