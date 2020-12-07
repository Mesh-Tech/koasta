using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System;
using Koasta.Shared.PatchModels;
using Microsoft.AspNetCore.Http.Extensions;

namespace Koasta.Service.Admin.Pages
{
    public class VenueModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly VenueRepository venues;

        public string Title { get; set; }
        public Venue Venue { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }

        public VenueModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              VenueRepository venues)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.venues = venues;
        }

        public async Task<IActionResult> OnGetAsync(int venueId)
        {
            if (await FetchData(venueId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int venueId)
        {
            if (string.Equals(Action, "delete", StringComparison.OrdinalIgnoreCase))
            {
                return await Delete(venueId).ConfigureAwait(false);
            }

            if (string.Equals(Action, "approve", StringComparison.OrdinalIgnoreCase))
            {
                return await UpdateVerification(venueId, 1).ConfigureAwait(false);
            }

            if (string.Equals(Action, "reject", StringComparison.OrdinalIgnoreCase))
            {
                return await UpdateVerification(venueId, 2).ConfigureAwait(false);
            }

            return RedirectToPage("/Index");
        }

        private async Task<IActionResult> Delete(int venueId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerCompany)
            {
                return RedirectToPage("/Index");
            }

            var getResult = (await venues.FetchVenue(venueId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Venue found")
                                    .OnSuccess(e => e.Value);

            if (!getResult.IsSuccess)
            {
                return RedirectToPage("/Index");
            }

            var result = await venues.DropVenue(venueId).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return RedirectToPage("/CompanyVenues", new
                {
                    companyId = getResult.Value.CompanyId
                });
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<IActionResult> UpdateVerification(int venueId, int status)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            var getResult = (await venues.FetchVenue(venueId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Venue found")
                                    .OnSuccess(e => e.Value);

            if (!getResult.IsSuccess)
            {
                return RedirectToPage("/Index");
            }

            var result = await venues.UpdateVenue(new VenuePatch {
                ResourceId = venueId,
                VerificationStatus = new PatchOperation<int> { Operation = OperationKind.Update, Value = status }
            }).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return Redirect(Request.GetEncodedUrl());
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int venueId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);
            ViewData["SubnavVenueId"] = venueId;

            if (Role.CanAdministerVenue || Role.CanWorkWithVenue)
            {
                var result = (await venues.FetchVenue(venueId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Venue found")
                                    .OnSuccess(e => e.Value);

                Venue = result.IsSuccess ? result.Value : null;

                if (!Role.CanAdministerSystem)
                {
                    if (Role.CanWorkWithCompany && Employee.CompanyId != Venue?.CompanyId)
                    {
                        return false;
                    }
                    else if (!Role.CanWorkWithCompany && Role.CanWorkWithVenue && Employee.VenueId != Venue?.VenueId)
                    {
                        return false;
                    }
                }

                Title = Venue.VenueName;
                return result.IsSuccess;
            }

            return false;
        }
    }
}
