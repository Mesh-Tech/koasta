using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using Koasta.Shared.PatchModels;
using Koasta.Service.Admin.Utils;
using System;
using Koasta.Shared.Configuration;

namespace Koasta.Service.Admin.Pages
{
    public class EditImageModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ImageRepository images;
        private readonly ISettings settings;

        public string Title { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public Uri ImageUrl { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [MaxLength(100)]
            [Display(Name = "Title")]
            public string ImageTitle { get; set; }
        }

        public EditImageModel(UserManager<Employee> userManager,
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
                return RedirectToPage("/Images");
            }
        }

        public async Task<IActionResult> OnPostAsync(int imageId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(imageId).ConfigureAwait(false);
                return this.TurboPage();
            }

            var result = await images.UpdateImage(new ImagePatch
            {
                ResourceId = imageId,
                ImageTitle = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.ImageTitle },
            })
            .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return this.RedirectToPage("/Image", new
                {
                    imageId
                });
            }
            else
            {
                return this.Page();
            }
        }

        private async Task<bool> FetchData(int imageId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (Role.CanAdministerVenue)
            {
                var image = (await images.FetchImage(imageId).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Image found")
                    .OnSuccess(e => e.Value);

                if (image.IsFailure)
                {
                    return false;
                }

                if (!Role.CanAdministerSystem && Employee.CompanyId != image.Value.CompanyId)
                {
                    return false;
                }

                Title = image.Value.ImageTitle;

                Input ??= new InputModel
                {
                    ImageTitle = image.Value.ImageTitle,
                };
                ImageUrl = new Uri($"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{image.Value.CompanyId}__{image.Value.ImageKey}__img");

                return true;
            }

            return false;
        }
    }
}
