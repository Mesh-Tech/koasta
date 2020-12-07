using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using Koasta.Service.Admin.Utils;
using Koasta.Service.Admin.Services;
using System.Collections.Generic;
using Koasta.Shared.Configuration;
using Koasta.Service.Admin.Models;
using System.Linq;
using System;
using Koasta.Shared.Queueing;
using Koasta.Shared.Types;
using Koasta.Shared.Queueing.Models;
using Koasta.Shared.Crypto;

namespace Koasta.Service.Admin.Pages
{
    public class AddVenueModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly VenueRepository venues;
        private readonly ICoordinatesService coordinates;
        private readonly CompanyRepository companies;
        private readonly ImageRepository images;
        private readonly ISettings settings;
        private readonly DocumentRepository documents;
        private readonly VenueDocumentRepository venueDocuments;
        private readonly IMessagePublisher messagePublisher;
        private readonly Constants constants;
        private readonly ICryptoHelper cryptoHelper;

        public List<MediaImage> Images { get; set; }
        public List<Document> Documents { get; set; }
        public Dictionary<int, Document> DocumentLookup { get; set; }
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
            public string VenueName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(8000)]
            [Display(Name = "Address")]
            public string VenueAddress { get; set; }

            [DataType(DataType.Text)]
            [MaxLength(800)]
            [Display(Name = "Address2")]
            public string VenueAddress2 { get; set; }

            [DataType(DataType.Text)]
            [MaxLength(800)]
            [Display(Name = "Address3")]
            public string VenueAddress3 { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(200)]
            [Display(Name = "County")]
            public string VenueCounty { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(10)]
            [Display(Name = "Postcode")]
            public string VenuePostCode { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(15)]
            [Display(Name = "Phone")]
            public string VenuePhone { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(30)]
            [Display(Name = "Contact")]
            public string VenueContact { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(8000)]
            [Display(Name = "Description")]
            public string VenueDescription { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Image")]
            public string ImageId { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Documents")]
            public List<string> DocumentIds { get; set; }
        }

        public AddVenueModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              ICoordinatesService coordinates,
                              CompanyRepository companies,
                              VenueRepository venues,
                              ImageRepository images,
                              DocumentRepository documents,
                              ISettings settings,
                              VenueDocumentRepository venueDocuments,
                              IMessagePublisher messagePublisher,
                              Constants constants,
                              ICryptoHelper cryptoHelper)
        {
            this.coordinates = coordinates;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.venues = venues;
            this.companies = companies;
            this.images = images;
            this.settings = settings;
            this.documents = documents;
            this.venueDocuments = venueDocuments;
            this.messagePublisher = messagePublisher;
            this.constants = constants;
            this.cryptoHelper = cryptoHelper;
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

            var coords = await coordinates.GetCoordinates(Input.VenuePostCode ?? "").ConfigureAwait(false);
            string latitude = null;
            string longitude = null;

            if (coords.HasValue)
            {
                latitude = coords.Value.Latitude.ToString();
                longitude = coords.Value.Longitude.ToString();
            }

            int? selectedImageId = null;
            if (Input.ImageId != null)
            {
                int.Parse(Input.ImageId);
            }

            var realImage = Images.Find(i => i.ImageId == selectedImageId);
            if (realImage == null || realImage.CompanyId != selectedCompanyId)
            {
                selectedImageId = null;
            }

            var result = coords.HasValue
                ? await venues.CreateVenueWithCoordinates(new Venue
                {
                    CompanyId = selectedCompanyId,
                    VenueName = Input.VenueName,
                    VenueCode = "",
                    VenueAddress = Input.VenueAddress,
                    VenueAddress2 = Input.VenueAddress2,
                    VenueAddress3 = Input.VenueAddress3,
                    VenueCounty = Input.VenueCounty,
                    VenuePostCode = Input.VenuePostCode,
                    VenueContact = Input.VenueContact,
                    VenuePhone = Input.VenuePhone,
                    VenueDescription = Input.VenueDescription,
                    VenueNotes = "",
                    VenueLatitude = latitude,
                    VenueLongitude = longitude,
                    ImageId = selectedImageId,
                    ReferenceCode = Guid.NewGuid().ToString()
                }).Ensure(c => c.HasValue, "Venue was created")
                .OnSuccess(c => c.Value)
                .ConfigureAwait(false)
                : await venues.CreateVenue(new Venue
                {
                    CompanyId = selectedCompanyId,
                    VenueName = Input.VenueName,
                    VenueCode = "",
                    VenueAddress = Input.VenueAddress,
                    VenueAddress2 = Input.VenueAddress2,
                    VenueAddress3 = Input.VenueAddress3,
                    VenueCounty = Input.VenueCounty,
                    VenuePostCode = Input.VenuePostCode,
                    VenueContact = Input.VenueContact,
                    VenuePhone = Input.VenuePhone,
                    VenueDescription = Input.VenueDescription,
                    VenueNotes = "",
                    VenueLatitude = latitude,
                    VenueLongitude = longitude,
                    ImageId = selectedImageId,
                    ReferenceCode = Guid.NewGuid().ToString()
                }).Ensure(c => c.HasValue, "Venue was created")
                .OnSuccess(c => c.Value)
                .ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                return this.Page();
            }

            await AddVenuesWithExternalPaymentProvider(selectedCompanyId).ConfigureAwait(false);

            var selectedDocumentIds = new List<VenueDocument>();
            if (Input.DocumentIds?.Count > 0)
            {
                selectedDocumentIds = Input.DocumentIds
                    .GetRange(0, Math.Min(Input.DocumentIds.Count, 100))
                    .Select(id => int.Parse(id))
                    .Where(id => DocumentLookup.ContainsKey(id) && DocumentLookup[id].CompanyId == selectedCompanyId)
                    .Select(id => new VenueDocument
                    {
                        VenueId = result.Value,
                        DocumentId = id
                    })
                    .ToList();
            }

            return (await venueDocuments.CreateVenueDocuments(selectedDocumentIds)
                .OnSuccess(() => this.RedirectToPage("/Venue", new
                {
                    venueId = result.Value
                }))
                .OnFailure(() => this.Page())
                .ConfigureAwait(false)).Value;
        }

        private async Task<bool> FetchData(int? companyId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (Employee.CompanyId != companyId && !Role.CanAdministerSystem)
            {
                return false;
            }

            var task = companyId == null
                ? images.FetchImages(0, int.MaxValue)
                : images.FetchCompanyImages(companyId.Value, 0, int.MaxValue);

            Images = await task
                .Ensure(i => i.HasValue, "Images were found")
                .OnSuccess(i => (List<MediaImage>)i.Value.Select(image => new MediaImage
                {
                    ImageId = image.ImageId,
                    ImageKey = image.ImageKey,
                    CompanyId = image.CompanyId,
                    ImageTitle = image.ImageTitle,
                    ImageUrl = new Uri($"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{image.CompanyId}__{image.ImageKey}__img"),
                }).ToList())
                .OnBoth((Result<List<MediaImage>> i) => i.IsSuccess ? i.Value : new List<MediaImage>())
                .ConfigureAwait(false);

            var task2 = companyId == null
                ? documents.FetchDocuments(0, int.MaxValue)
                : documents.FetchCompanyDocuments(companyId.Value, 0, int.MaxValue);

            Documents = await task2
                .Ensure(i => i.HasValue, "Documents were found")
                .OnSuccess(i => i.Value)
                .OnBoth((Result<List<Document>> i) => i.IsSuccess ? i.Value : new List<Document>())
                .ConfigureAwait(false);
            DocumentLookup = new Dictionary<int, Document>();
            Documents.ForEach(doc => DocumentLookup[doc.DocumentId] = doc);

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

        private async Task AddVenuesWithExternalPaymentProvider(int companyId)
        {
            try
            {
                var companyResult = await companies.FetchCompany(companyId).ConfigureAwait(false);

                if (companyResult.IsSuccess)
                {
                    var company = companyResult.Value.Value;

                    var accessToken = await cryptoHelper.DecryptString(company.ExternalAccessToken).ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        messagePublisher.DirectPublish(
                            constants.VenueSyncQueueName,
                            constants.VenueSyncExchangeRoutingKey,
                            constants.VenueSyncExchangeName,
                            constants.VenueSyncExchangeQueueName,
                            new Message<DtoVenueSyncMessage>
                            {
                                Type = constants.VenueSynch,
                                Data = new DtoVenueSyncMessage(Guid.NewGuid(), VenueSyncAction.Create, accessToken, company)
                            });
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: Something here
            }
        }
    }
}
