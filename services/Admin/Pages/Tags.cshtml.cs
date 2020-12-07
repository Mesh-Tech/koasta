using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Koasta.Shared.Types;

namespace Koasta.Service.Admin.Pages
{
    public class TagsModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly TagRepository venues;

        public string Title { get; set; }
        public List<Tag> Tags { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }

        public TagsModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              TagRepository venues)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.venues = venues;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            var results = (await venues.FetchCountedTags(PageNumber, 20).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Tags found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Tag> { Data = new List<Tag>(), Count = 0 });
            TotalResults = results.Count;
            Tags = results.Data;
            Title = $"Tags ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);

            return Page();
        }
    }
}
