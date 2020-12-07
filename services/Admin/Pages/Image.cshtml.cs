using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System;
using Koasta.Shared.Configuration;

namespace Koasta.Service.Admin.Pages
{
    public class ImageModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ImageRepository images;
        private readonly ISettings settings;

        public string Title { get; set; }
        public Image Image { get; set; }
        public Uri ImageUrl { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }

        public ImageModel(UserManager<Employee> userManager,
                          RoleManager<EmployeeRole> roleManager,
                          ImageRepository images,
                          ISettings settings)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.images = images;
            this.settings = settings;
        }

        public async Task<IActionResult> OnGetAsync(int imageId)
        {
            if (await FetchData(imageId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int imageId)
        {
            if (!string.Equals(Action, "delete", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerCompany && !Role.CanAdministerVenue && !Role.CanWorkWithCompany)
            {
                return RedirectToPage("/Index");
            }

            var getResult = (await images.FetchImage(imageId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Image found")
                                    .OnSuccess(e => e.Value);

            if (!getResult.IsSuccess)
            {
                return RedirectToPage("/Index");
            }

            var result = await images.DropImage(imageId).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return RedirectToPage("/Media", new
                {
                    companyId = getResult.Value.CompanyId
                });
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int imageId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (Role.CanAdministerCompany || Role.CanWorkWithCompany || Role.CanAdministerVenue)
            {
                var result = (await images.FetchImage(imageId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Image found")
                                    .OnSuccess(e => e.Value);

                Image = result.IsSuccess ? result.Value : null;
                ViewData["SubnavCompanyId"] = Image?.CompanyId;

                if (!Role.CanAdministerSystem && Employee.CompanyId != Image?.CompanyId)
                {
                    return false;
                }

                Title = Image?.ImageTitle;
                ImageUrl = Image == null ? null : ImageUrl = new Uri($"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{Image.CompanyId}__{Image.ImageKey}__img");
                return result.IsSuccess;
            }

            return false;
        }
    }
}
