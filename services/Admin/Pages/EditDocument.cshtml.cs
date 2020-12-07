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
using Koasta.Shared.Configuration;

namespace Koasta.Service.Admin.Pages
{
    public class EditDocumentModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly DocumentRepository documents;
        private readonly ISettings settings;

        public string Title { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }

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
            public string DocumentTitle { get; set; }
            [Required]
            [DataType(DataType.Text)]
            [MaxLength(8000)]
            [Display(Name = "Description")]
            public string DocumentDescription { get; set; }
        }

        public EditDocumentModel(UserManager<Employee> userManager,
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
                return RedirectToPage("/Documents");
            }
        }

        public async Task<IActionResult> OnPostAsync(int documentId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(documentId).ConfigureAwait(false);
                return this.TurboPage();
            }

            var result = await documents.UpdateDocument(new DocumentPatch
            {
                ResourceId = documentId,
                DocumentTitle = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.DocumentTitle },
                DocumentDescription = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.DocumentDescription },
            })
            .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return this.RedirectToPage("/Document", new
                {
                    documentId
                });
            }
            else
            {
                return this.Page();
            }
        }

        private async Task<bool> FetchData(int documentId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (Role.CanAdministerVenue)
            {
                var document = (await documents.FetchDocument(documentId).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Document found")
                    .OnSuccess(e => e.Value);

                if (document.IsFailure)
                {
                    return false;
                }

                if (!Role.CanAdministerSystem && Employee.CompanyId != document.Value.CompanyId)
                {
                    return false;
                }

                Title = document.Value.DocumentTitle;

                Input ??= new InputModel
                {
                    DocumentTitle = document.Value.DocumentTitle,
                };

                return true;
            }

            return false;
        }
    }
}
