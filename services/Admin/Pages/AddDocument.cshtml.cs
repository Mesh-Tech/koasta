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
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using Koasta.Shared.Configuration;
using Amazon;

namespace Koasta.Service.Admin.Pages
{
    public class DocumentValidationAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public DocumentValidationAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            var extension = Path.GetExtension(file.FileName);
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".odt", ".xls", ".xlsx", ".ods", ".ppt", ".pptx", ".txt", ".md", ".html", ".htm" };
            if (file != null)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage());
                }

                if (!allowedExtensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Maximum allowed file size is { _maxFileSize} bytes.";
        }
    }

    public class AddDocumentModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly DocumentRepository documents;
        private readonly CompanyRepository companies;
        private readonly ISettings settings;

        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public List<Company> Companies { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [DataType(DataType.Text)]
            [Display(Name = "CompanyId")]
            public string CompanyId { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(100)]
            [Display(Name = "Name")]
            public string DocumentName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(8000)]
            [Display(Name = "Description")]
            public string DocumentDescription { get; set; }

            [Required]
            [DataType(DataType.Upload)]
            [DocumentValidation(1024 * 1024 * 50)]
            [Display(Name = "Document")]
            public IFormFile DocumentData { get; set; }
        }

        public AddDocumentModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              CompanyRepository companies,
                              DocumentRepository documents,
                              ISettings settings)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.documents = documents;
            this.companies = companies;
            this.settings = settings;
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

            int documentId = -1;
            var result = await documents.CreateDocument(new Document {
                    CompanyId = selectedCompanyId,
                    DocumentKey = Guid.NewGuid().ToString(),
                    DocumentTitle = Input.DocumentName,
                    DocumentDescription = Input.DocumentDescription,
                })
                .Ensure(c => c.HasValue, "Document was created")
                .OnSuccess(c => {
                    documentId = c.Value;
                    return documents.FetchDocument(c.Value);
                })
                .Ensure(c => c.HasValue, "Document was found")
                .OnSuccess(c => UploadDocumentToS3(c.Value, Input.DocumentData))
                .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return RedirectToPage("/Document", new {
                    documentId
                });
            }
            else
            {
                return this.Page();
            }
        }

        private async Task<bool> FetchData(int? companyId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (companyId == null && !Role.CanAdministerSystem)
            {
                return false;
            }

            if (Employee.CompanyId != companyId && !Role.CanAdministerCompany)
            {
                return false;
            }

            if (Role.CanAdministerSystem)
            {
                Companies = await companies.FetchCompanies(0, int.MaxValue)
                    .Ensure(t => t.HasValue, "Companies were found")
                    .OnSuccess(t => t.Value)
                    .OnBoth(t => t.IsSuccess ? t.Value : new List<Company>())
                    .ConfigureAwait(false);
            }

            return Role.CanAdministerCompany;
        }

        private async Task<Result> UploadDocumentToS3(Document document, IFormFile file)
        {
            using var client = new AmazonS3Client(settings.Connection.AWSAccessKeyId, settings.Connection.AWSSecretAccessKey, RegionEndpoint.USEast1);
            using var s3 = new TransferUtility(client);

            try
            {
                await s3.UploadAsync(file.OpenReadStream(), settings.Connection.S3PrivateBucketName, $"documents/{document.CompanyId}__{document.DocumentKey}__doc").ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Fail("Failed to upload to S3");
            }
        }
    }
}
