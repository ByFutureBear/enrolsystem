using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnlineEnrolmentSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace OnlineEnrolmentSystem.Pages.Enquiry
{
    public class ContactUsModel : PageModel
    {
        private readonly SmtpSettings _smtpSettings;

        public ContactUsModel(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required, StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required, EmailAddress, StringLength(100)]
            public string Email { get; set; } = string.Empty;

            [Required, StringLength(150)]
            public string Subject { get; set; } = string.Empty;

            [Required, StringLength(1000)]
            public string Message { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                Input.Email = User.FindFirstValue(ClaimTypes.Email) ?? "";
                Input.Name = User.Identity.Name ?? "";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                // **Testing purpose, no actual email will be sent
                // SMTP Main server info
                using (var smtp = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port))
                {
                    // Account to send email
                    smtp.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                    smtp.EnableSsl = true;

                    var mail = new MailMessage
                    {
                        From = new MailAddress(Input.Email, Input.Name),
                        Subject = Input.Subject,
                        Body = $"Message from {Input.Name} ({Input.Email}):\n\n{Input.Message}",
                        IsBodyHtml = false
                    };

                    // School’s contact email
                    mail.To.Add(_smtpSettings.ReceiverEmail);

                    await smtp.SendMailAsync(mail);
                }

                SuccessMessage = "Thank you! Your enquiry has been sent.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error sending email: Please try again later.");
                return Page();
            }
        }
    }
}
