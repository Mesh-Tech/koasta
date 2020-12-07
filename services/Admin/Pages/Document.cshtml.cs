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
    public class DocumentModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly DocumentRepository documents;
        private readonly ISettings settings;

        public string Title { get; set; }
        public Document Document { get; set; }
        public Uri DocumentUrl { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }

        public DocumentModel(UserManager<Employee> userManager,
                          RoleManager<EmployeeRole> roleManager,
                          DocumentRepository documents,
                          ISettings settings)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.documents = documents;
            this.settings = settings;
        }

        public async Task<IActionResult> OnGetAsync(int documentId)
        {
            if (await FetchData(documentId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int documentId)
        {
            if (!string.Equals(Action, "delete", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerCompany)
            {
                return RedirectToPage("/Index");
            }

            var getResult = (await documents.FetchDocument(documentId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Document found")
                                    .OnSuccess(e => e.Value);

            if (!getResult.IsSuccess)
            {
                return RedirectToPage("/Index");
            }

            var result = await documents.DropDocument(documentId).ConfigureAwait(false);
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

        private async Task<bool> FetchData(int documentId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (Role.CanAdministerCompany || Role.CanWorkWithCompany)
            {
                var result = (await documents.FetchDocument(documentId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Document found")
                                    .OnSuccess(e => e.Value);

                Document = result.IsSuccess ? result.Value : null;
                ViewData["SubnavCompanyId"] = Document?.CompanyId;

                if (!Role.CanAdministerSystem && Employee.CompanyId != Document?.CompanyId)
                {
                    return false;
                }

                Title = Document?.DocumentTitle;
                DocumentUrl = Document == null ? null : DocumentUrl = new Uri($"{settings.Connection.CfPrivateRootEndpoint}/documents/{Document.CompanyId}__{Document.DocumentKey}__doc");
                return result.IsSuccess;
            }

            return false;
        }
    }
}
