using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using Koasta.Service.Admin.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Koasta.Service.Admin.Pages
{
    public class AddEmployeeModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly VenueRepository venues;
        private readonly EmployeeRoleRepository roles;
        private readonly CompanyRepository companies;

        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public List<Company> Companies { get; set; }
        public List<VenueItem> Venues { get; set; }
        public List<EmployeeRole> Roles { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [MaxLength(200)]
            [Display(Name = "Name")]
            public string EmployeeName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(200)]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [MaxLength(200)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "CompanyId")]
            [MaxLength(800)]
            public string CompanyId { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "VenueId")]
            public string VenueId { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "RoleId")]
            public string RoleId { get; set; }
        }

        public AddEmployeeModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              VenueRepository venues,
                              CompanyRepository companies,
                              EmployeeRoleRepository roles)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.venues = venues;
            this.roles = roles;
            this.companies = companies;
        }

        public async Task<IActionResult> OnGetAsync(int? companyId)
        {
            if (await FetchData(companyId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int? companyId)
        {
            if (!await FetchData(companyId).ConfigureAwait(false))
            {
                return RedirectToPage("/Index");
            }

            if (!ModelState.IsValid)
            {
                return this.TurboPage();
            }

            var selectedVenueId = int.Parse(Input.VenueId);
            var selectedVenue = Venues.Find(v => v.VenueId == selectedVenueId);
            if (selectedVenue == null) {
                return this.TurboPage();
            }

            var selectedRoleId = int.Parse(Input.RoleId);
            var selectedRole = Roles.Find(r => r.RoleId == selectedRoleId);

            if (selectedRole?.IsMorePrivilegedThanRole(Role) != false)
            {
                return this.TurboPage();
            }

            var selectedCompanyId = 0;
            if (Role.CanAdministerSystem)
            {
                if (string.IsNullOrWhiteSpace(Input.CompanyId))
                {
                    return this.TurboPage();
                }

                selectedCompanyId = int.Parse(Input.CompanyId);
            }
            else
            {
                selectedCompanyId = companyId.Value;
            }

            if (selectedVenue.CompanyId != selectedCompanyId)
            {
                return this.TurboPage();
            }

            var user = new Employee
            {
                CompanyId = selectedCompanyId,
                VenueId = int.Parse(Input.VenueId),
                RoleId = selectedRoleId,
                EmployeeName = Input.EmployeeName,
                Username = Input.Username,
                Confirmed = true,
            };

            var result = await userManager.CreateAsync(user, Input.Password).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return this.Page();
            }

            result = await userManager.UpdateSecurityStampAsync(user).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return this.Page();
            }

            return RedirectToPage("/Employee", new
            {
                employeeId = user.EmployeeId
            });
        }

        private async Task<bool> FetchData(int? companyId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (Employee.CompanyId != companyId && !Role.CanAdministerSystem)
            {
                return false;
            }

            Venues = await venues.FetchVenueItems(Role.CanAdministerSystem ? null : companyId)
                .Ensure(t => t.HasValue, "Venue items were found")
                .OnSuccess(t => t.Value)
                .OnBoth(t => t.IsSuccess ? t.Value : new List<VenueItem>())
                .ConfigureAwait(false);

            if (Role.CanAdministerSystem)
            {
                Companies = await companies.FetchCompanies(0, int.MaxValue)
                    .Ensure(t => t.HasValue, "Companies were found")
                    .OnSuccess(t => t.Value)
                    .OnBoth(t => t.IsSuccess ? t.Value : new List<Company>())
                    .ConfigureAwait(false);
            }

            var allRoles = await roles.FetchEmployeeRoles(0, int.MaxValue)
                .Ensure(t => t.HasValue, "Roles were found")
                .OnSuccess(t => t.Value)
                .OnBoth(t => t.IsSuccess ? t.Value : new List<EmployeeRole>())
                .ConfigureAwait(false);
            Roles = allRoles.Where(r => !r.IsMorePrivilegedThanRole(Role)).ToList();

            return Role.CanAdministerCompany;
        }
    }
}
