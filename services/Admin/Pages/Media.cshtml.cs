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
using Koasta.Shared.Database;

namespace Koasta.Service.Admin.Pages
{
    public class MediaModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly MediaRepository mediaEntries;
        private readonly ISettings settings;
        private readonly ImageRepository images;
        private readonly DocumentRepository documents;

        public string Title { get; set; }
        public List<MediaFile> MediaEntries { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }
        [BindProperty(SupportsGet = true)]
        public string MediaType { get; set; }
        public int? CompanyId { get; set; }

        public MediaModel(UserManager<Employee> userManager,
                           RoleManager<EmployeeRole> roleManager,
                           MediaRepository mediaEntries,
                           ImageRepository images,
                           DocumentRepository documents,
                           ISettings settings)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.mediaEntries = mediaEntries;
            this.settings = settings;
            this.images = images;
            this.documents = documents;
        }

        public async Task<IActionResult> OnGetAsync(int? companyId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (companyId == null && !Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            if (string.Equals(MediaType, "image", StringComparison.OrdinalIgnoreCase))
            {
                if (Employee.CompanyId != companyId && !Role.CanAdministerVenue)
                {
                    return RedirectToPage("/Index");
                }

                await FetchImages(companyId).ConfigureAwait(false);
            }
            else if (string.Equals(MediaType, "document", StringComparison.OrdinalIgnoreCase))
            {
                if (Employee.CompanyId != companyId && !Role.CanAdministerCompany)
                {
                    return RedirectToPage("/Index");
                }

                await FetchDocuments(companyId).ConfigureAwait(false);
            }
            else
            {
                if (Role.CanAdministerSystem)
                {
                    await FetchMedia(null).ConfigureAwait(false);
                }
                else if (Employee.CompanyId == companyId && Role.CanAdministerCompany)
                {
                    await FetchMedia(companyId).ConfigureAwait(false);
                }
                else if (Employee.CompanyId == companyId && Role.CanAdministerVenue)
                {
                    await FetchImages(companyId).ConfigureAwait(false);
                }
                else
                {
                    return RedirectToPage("/Index");
                }
            }

            Title = $"Media ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);
            ViewData["SubnavCompanyId"] = companyId;
            CompanyId = companyId;

            return Page();
        }

        private async Task FetchMedia(int? companyId)
        {
            var task = companyId == null
                ? mediaEntries.FetchCountedMediaEntries(PageNumber, 20)
                : mediaEntries.FetchCountedCompanyMediaEntries(companyId.Value, PageNumber, 20);

            var results = (await task.ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Media entries found")
                .OnSuccess(e => e.Value)
                .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<MediaEntry> { Data = new List<MediaEntry>(), Count = 0 });

            TotalResults = results.Count;
            MediaEntries = results.Data.Select(entry => new MediaFile
            {
                Id = entry.Id,
                Key = entry.Key,
                CompanyId = entry.CompanyId,
                Title = entry.Title,
                ThumbnailUrl = entry.MediaType == 1 ? new Uri($"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{entry.CompanyId}__{entry.Key}__img") : null,
                Type = (MediaFileType)entry.MediaType,
            }).ToList();
        }

        private async Task FetchImages(int? companyId)
        {
            var task = companyId == null
                ? images.FetchCountedImages(PageNumber, 20)
                : images.FetchCountedCompanyImages(companyId.Value, PageNumber, 20);

            var results = (await task.ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Images found")
                .OnSuccess(e => e.Value)
                .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Image> { Data = new List<Image>(), Count = 0 });

            TotalResults = results.Count;
            MediaEntries = results.Data.Select(entry => new MediaFile
            {
                Id = entry.ImageId,
                Key = entry.ImageKey,
                CompanyId = entry.CompanyId,
                Title = entry.ImageTitle,
                ThumbnailUrl = new Uri($"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{entry.CompanyId}__{entry.ImageKey}__img"),
                Type = MediaFileType.Image,
            }).ToList();
        }

        private async Task FetchDocuments(int? companyId)
        {
            var task = companyId == null
                ? documents.FetchCountedDocuments(PageNumber, 20)
                : documents.FetchCountedCompanyDocuments(companyId.Value, PageNumber, 20);

            var results = (await task.ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Documents found")
                .OnSuccess(e => e.Value)
                .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Document> { Data = new List<Document>(), Count = 0 });

            TotalResults = results.Count;
            MediaEntries = results.Data.Select(entry => new MediaFile
            {
                Id = entry.DocumentId,
                Key = entry.DocumentKey,
                CompanyId = entry.CompanyId,
                Title = entry.DocumentTitle,
                Type = MediaFileType.Document,
            }).ToList();
        }
    }
}
