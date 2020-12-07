using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Koasta.Shared.Database;
using CSharpFunctionalExtensions;
using System;
using System.Linq;

namespace Koasta.Service.Admin.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Employee> _signInManager;
        private readonly UserManager<Employee> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly CompanyRepository companies;
        private readonly VenueRepository venues;

        public RegisterModel(
            UserManager<Employee> userManager,
            SignInManager<Employee> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            CompanyRepository companies,
            VenueRepository venues)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            this.companies = companies;
            this.venues = venues;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Name")]
            public string EmployeeName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string EmployeeEmail { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string EmployeePassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("EmployeePassword", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmEmployeePassword { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Company Name")]
            public string CompanyName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Company Address")]
            public string CompanyAddress { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Company Postcode")]
            public string CompanyPostcode { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Company Contact")]
            public string CompanyContact { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Company Email address")]
            public string CompanyEmail { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Company Phone Number")]
            public string CompanyPhone { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Venue name")]
            public string VenueName { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ViewData["HideNavigation"] = true;
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ViewData["HideNavigation"] = true;

            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var company = await companies.CreateCompany(new Company
            {
                CompanyName = Input.CompanyName,
                CompanyPhone = Input.CompanyPhone,
                CompanyPostcode = Input.CompanyPostcode,
                CompanyContact = Input.CompanyContact,
                CompanyEmail = Input.CompanyEmail,
                CompanyAddress = Input.CompanyAddress
            }).Ensure(c => c.HasValue, "Company created")
              .OnSuccess(c => c.Value)
              .ConfigureAwait(false);

            if (company.IsFailure)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong when confirming your details, sorry about that. Please reach out to our support team to finish your setup.");
                return Page();
            }

            var venue = await venues.CreateVenue(new Venue {
                CompanyId = company.Value,
                VenueCode = "",
                VenueName = Input.VenueName,
                VenueAddress = "",
                VenuePostCode = "",
                VenuePhone = "",
                VenueContact = "",
                VenueDescription = "",
                VenueNotes = "Created via onboarding",
            }).Ensure(c => c.HasValue, "Venue created")
              .OnSuccess(c => c.Value)
              .ConfigureAwait(false);

            if (venue.IsFailure)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong when confirming your details, sorry about that. Please reach out to our support team to finish your setup.");
                return Page();
            }

            var user = new Employee { Username = Input.EmployeeEmail, EmployeeName = Input.EmployeeName, CompanyId = company.Value, VenueId = venue.Value, RoleId = 5 };
            var result = await _userManager.CreateAsync(user, Input.EmployeePassword).ConfigureAwait(false);
            if (result.Succeeded)
            {
                result = await _userManager.UpdateSecurityStampAsync(user).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    _logger.LogInformation("User created a new account with password.");

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.EmployeeId, code, returnUrl },
                        protocol: Request.Scheme
                    );

                    await _emailSender.SendEmailAsync(Input.EmployeeEmail, "Welcome to Koasta!",
                        $@"
                            <p>Thanks for choosing Koasta. You're on your way to accepting digital payments with ease.</p>
                            <p>Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>. After confirming, you'll have access to your Koasta dashboard and can begin your onboarding process.</p>
                            <p>Further information about how to get started with Koasta can be found <a href='https://support.koasta.com'>here</a>.</p>
                        ").ConfigureAwait(false);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.EmployeeEmail, returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(false);
                        return LocalRedirect(returnUrl);
                    }
                }
            }

            try
            {
                await venues.DropVenue(venue.Value).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            try
            {
                await companies.DropCompany(company.Value).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            if (!result.Errors.Any(e => e.Code.Equals("TOOCOMMON", StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError(string.Empty, "Something went wrong when confirming your details, sorry about that. Please reach out to our support team to finish your setup.");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
