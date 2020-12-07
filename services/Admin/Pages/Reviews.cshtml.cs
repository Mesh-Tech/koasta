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
    public class ReviewsModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ReviewRepository reviews;

        public string Title { get; set; }
        public List<Review> Reviews { get; set; }
        public List<AggregatedVenueVote> Votes { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }

        public ReviewsModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              ReviewRepository reviews)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.reviews = reviews;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            var voteResults = (await reviews.FetchAggregatedVenueVotes().ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Votes found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new List<AggregatedVenueVote>());

            var results = (await reviews.FetchCountedReviews(PageNumber, 20).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Reviews found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Review> { Data = new List<Review>(), Count = 0 });
            TotalResults = results.Count;
            Reviews = results.Data;
            Votes = voteResults;
            Title = $"Reviews ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);

            return Page();
        }
    }
}
