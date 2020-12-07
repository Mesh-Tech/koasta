using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Koasta.Shared.Models;
using Koasta.Shared.Database;
using CSharpFunctionalExtensions;
using Koasta.Service.Admin.Utils;

namespace Koasta.Service.Admin.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<Employee> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly CompanyRepository companies;

        public ForgotPasswordModel(UserManager<Employee> userManager, IEmailSender emailSender, CompanyRepository companies)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            this.companies = companies;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string Username { get; set; }
        }

        public void OnGet() {
            ViewData["HideNavigation"] = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["HideNavigation"] = true;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(Input.Username).ConfigureAwait(false);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var company = await companies.FetchCompany(user.CompanyId)
                    .Ensure(c => c.HasValue, "Company was found")
                    .OnSuccess(c => c.Value)
                    .ConfigureAwait(false);

                if (!company.IsSuccess)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var email = user.Username;
                if (!email.IsValidEmail()) {
                    email = company.Value.CompanyEmail;
                }

                if (!email.IsValidEmail()) {
                    ModelState.AddModelError("", "There's no email address on file for you or your company, so we're unable to send you a link to reset your password. Please get in touch for further assistance.");
                    return Page();
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    company.Value.CompanyEmail,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.").ConfigureAwait(false);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
