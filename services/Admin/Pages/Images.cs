using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Koasta.Shared.Types;
using System.Linq;
using Koasta.Service.Admin.Models;
using System;
using Koasta.Shared.Configuration;

namespace Koasta.Service.Admin.Pages
{
    public class ImagesModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ImageRepository images;
        private readonly ISettings settings;

        public string Title { get; set; }
        public List<MediaImage> Images { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }

        public ImagesModel(UserManager<Employee> userManager, 
                           RoleManager<EmployeeRole> roleManager,
                           ImageRepository images,
                           ISettings settings)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.images = images;
            this.settings = settings;
        }

        public async Task<IActionResult> OnGetAsync(int? companyId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (companyId == null && !Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            if (Employee.CompanyId != companyId && !Role.CanAdministerCompany)
            {
                return RedirectToPage("/Index");
            }

            var task = companyId == null
                ? images.FetchCountedImages(PageNumber, 20)
                : images.FetchCountedCompanyImages(companyId.Value, PageNumber, 20);

            var results = (await task.ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Images found")
                .OnSuccess(e => e.Value)
                .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Image> { Data = new List<Image>(), Count = 0 });
            TotalResults = results.Count;
            Images = results.Data.Select(image => new MediaImage
            {
                ImageId = image.ImageId,
                ImageKey = image.ImageKey,
                CompanyId = image.CompanyId,
                ImageTitle = image.ImageTitle,
                ImageUrl = new Uri($"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{image.CompanyId}__{image.ImageKey}__img"),
            }).ToList();
            Title = $"Media ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);
            ViewData["SubnavCompanyId"] = companyId;

            return Page();
        }
    }
}
