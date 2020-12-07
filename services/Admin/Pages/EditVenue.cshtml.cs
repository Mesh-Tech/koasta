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
using Koasta.Service.Admin.Services;
using System.Linq;
using Koasta.Shared.Configuration;
using System.Collections.Generic;
using Koasta.Service.Admin.Models;
using System;
using Koasta.Shared.Queueing;
using Koasta.Shared.Types;
using Koasta.Shared.Crypto;
using Koasta.Shared.Queueing.Models;

namespace Koasta.Service.Admin.Pages
{
    public class EditVenueModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly VenueRepository venues;
        private readonly ICoordinatesService coordinates;
        private readonly TagRepository tags;
        private readonly ImageRepository images;
        private readonly ISettings settings;
        private readonly DocumentRepository documents;
        private readonly VenueDocumentRepository venueDocuments;
        private readonly CompanyRepository companies;
        private readonly IMessagePublisher messagePublisher;
        private readonly Constants constants;
        private readonly ICryptoHelper cryptoHelper;
        private readonly VenueOpeningTimeRepository venueOpeningTimes;

        public List<MediaImage> Images { get; set; }
        public List<Document> Documents { get; set; }
        public Dictionary<int, Document> DocumentLookup { get; set; }

        public string Title { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public Venue Venue { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
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
            [Display(Name = "Tags")]
            public string Tags { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Documents")]
            public List<string> DocumentIds { get; set; }

            [Display(Name = "Progress")]
            public int? VenueProgress { get; set; }

            [Display(Name = "Serving Type")]
            public int ServingType { get; set; }

            [Display(Name = "Sunday open")]
            public bool SundayOpeningTimeEnabled { get; set; }

            [Display(Name = "Monday open")]
            public bool MondayOpeningTimeEnabled { get; set; }

            [Display(Name = "Tuesday open")]
            public bool TuesdayOpeningTimeEnabled { get; set; }

            [Display(Name = "Wednesday open")]
            public bool WednesdayOpeningTimeEnabled { get; set; }

            [Display(Name = "Thursday open")]
            public bool ThursdayOpeningTimeEnabled { get; set; }

            [Display(Name = "Friday open")]
            public bool FridayOpeningTimeEnabled { get; set; }

            [Display(Name = "Saturday open")]
            public bool SaturdayOpeningTimeEnabled { get; set; }

            [Display(Name = "SundayOpeningTimeStart")]
            public TimeSpan? SundayOpeningTimeStart { get; set; }
            [Display(Name = "SundayOpeningTimeEnd")]
            public TimeSpan? SundayOpeningTimeEnd { get; set; }

            [Display(Name = "MondayOpeningTimeStart")]
            public TimeSpan? MondayOpeningTimeStart { get; set; }
            [Display(Name = "MondayOpeningTimeEnd")]
            public TimeSpan? MondayOpeningTimeEnd { get; set; }

            [Display(Name = "TuesdayOpeningTimeStart")]
            public TimeSpan? TuesdayOpeningTimeStart { get; set; }
            [Display(Name = "TuesdayOpeningTimeEnd")]
            public TimeSpan? TuesdayOpeningTimeEnd { get; set; }

            [Display(Name = "WednesdayOpeningTimeStart")]
            public TimeSpan? WednesdayOpeningTimeStart { get; set; }
            [Display(Name = "WednesdayOpeningTimeEnd")]
            public TimeSpan? WednesdayOpeningTimeEnd { get; set; }

            [Display(Name = "ThursdayOpeningTimeStart")]
            public TimeSpan? ThursdayOpeningTimeStart { get; set; }
            [Display(Name = "ThursdayOpeningTimeEnd")]
            public TimeSpan? ThursdayOpeningTimeEnd { get; set; }

            [Display(Name = "FridayOpeningTimeStart")]
            public TimeSpan? FridayOpeningTimeStart { get; set; }
            [Display(Name = "FridayOpeningTimeEnd")]
            public TimeSpan? FridayOpeningTimeEnd { get; set; }

            [Display(Name = "SaturdayOpeningTimeStart")]
            public TimeSpan? SaturdayOpeningTimeStart { get; set; }
            [Display(Name = "SaturdayOpeningTimeEnd")]
            public TimeSpan? SaturdayOpeningTimeEnd { get; set; }
        }

        public EditVenueModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              ICoordinatesService coordinates,
                              CompanyRepository companies,
                              VenueRepository venues,
                              TagRepository tags,
                              ImageRepository images,
                              ISettings settings,
                              DocumentRepository documents,
                              VenueDocumentRepository venueDocuments,
                              IMessagePublisher messagePublisher,
                              Constants constants,
                              ICryptoHelper cryptoHelper,
                              VenueOpeningTimeRepository venueOpeningTimes)
        {
            this.coordinates = coordinates;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.venues = venues;
            this.tags = tags;
            this.images = images;
            this.settings = settings;
            this.documents = documents;
            this.venueDocuments = venueDocuments;
            this.companies = companies;
            this.messagePublisher = messagePublisher;
            this.constants = constants;
            this.cryptoHelper = cryptoHelper;
            this.venueOpeningTimes = venueOpeningTimes;
        }

        public async Task<IActionResult> OnGetAsync(int venueId)
        {
            if (await FetchData(venueId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Venues");
            }
        }

        public async Task<IActionResult> OnPostAsync(int venueId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(venueId).ConfigureAwait(false);
                return this.TurboPage();
            }

            if (!await FetchData(venueId).ConfigureAwait(false))
            {
                return this.RedirectToPage("/Index");
            }

            var coords = await coordinates.GetCoordinates(Input.VenuePostCode ?? "").ConfigureAwait(false);
            string latitude = null;
            string longitude = null;

            int? selectedImageId = Input.ImageId == null ? null : int.Parse(Input.ImageId) as int?;
            if (Input.ImageId == null || !Images.Exists(i => i.ImageId == selectedImageId))
            {
                selectedImageId = null;
            }

            if (coords.HasValue)
            {
                latitude = coords.Value.Latitude.ToString();
                longitude = coords.Value.Longitude.ToString();
            }

            var servingType = ServingTypeHelper.ValidateServingType(Input.ServingType) ? Input.ServingType : 0;

            var patch = new VenuePatch
            {
                ResourceId = venueId,
                VenueName = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.VenueName },
                VenueAddress = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.VenueAddress },
                VenueAddress2 = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.VenueAddress2 },
                VenueAddress3 = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.VenueAddress3 },
                VenueCounty = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.VenueCounty },
                VenuePostCode = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.VenuePostCode },
                VenuePhone = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.VenuePhone },
                VenueContact = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.VenueContact },
                VenueDescription = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.VenueDescription },
                VenueLatitude = new PatchOperation<string> { Operation = OperationKind.Update, Value = latitude },
                VenueLongitude = new PatchOperation<string> { Operation = OperationKind.Update, Value = longitude },
                ServingType = new PatchOperation<int> { Operation = OperationKind.Update, Value = servingType }
            };

            if (selectedImageId != null) {
                patch.ImageId = new PatchOperation<int> { Operation = OperationKind.Update, Value = selectedImageId.Value };
            }

            if (Input.VenueProgress != null && Role.CanAdministerSystem)
            {
                patch.Progress = new PatchOperation<int> { Operation = OperationKind.Update, Value = Input.VenueProgress.Value };
            }

            var result = await venues.UpdateVenue(patch)
                .OnSuccess(() => tags.ReplaceVenueTags(venueId, (Input.Tags ?? "").Trim().Split(',').ToList())
                .OnSuccess(() => venueDocuments.ReplaceVenueDocuments(venueId, (Input.DocumentIds ?? new List<string>()).Select(id => new VenueDocument { VenueId = venueId, DocumentId = int.Parse(id) }).ToList())))
                .OnSuccess(() => venues.UpdateVenueCoordinates(venueId))
                .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                await UpdateVenuesWithExternalPaymentProvider(Venue.CompanyId).ConfigureAwait(false);
                await UpdateVenueOpeningTimes().ConfigureAwait(false);
                return this.RedirectToPage("/Venue", new
                {
                    venueId
                });
            }
            else
            {
                return this.Page();
            }
        }

        private async Task<bool> FetchData(int venueId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem && Employee.VenueId != venueId)
            {
                return false;
            }

            if (Role.CanAdministerVenue || Role.CanWorkWithVenue)
            {
                var venue = (await venues.FetchVenue(venueId).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Venue found")
                    .OnSuccess(e => e.Value);

                if (venue.IsFailure)
                {
                    return false;
                }

                Venue = venue.Value;

                var venueTags = (await tags.FetchVenueTags(venue.Value.VenueId).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Venue tags found")
                    .OnSuccess(e => e.Value);

                if (venueTags.IsFailure)
                {
                    return false;
                }

                Title = venue.Value.VenueName;

                Images = await images.FetchCompanyImages(venue.Value.CompanyId, 0, int.MaxValue)
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

                Documents = await documents.FetchCompanyDocuments(venue.Value.CompanyId, 0, int.MaxValue)
                    .Ensure(i => i.HasValue, "Documents were found")
                    .OnSuccess(i => i.Value)
                    .OnBoth((Result<List<Document>> i) => i.IsSuccess ? i.Value : new List<Document>())
                    .ConfigureAwait(false);
                DocumentLookup = new Dictionary<int, Document>();
                Documents.ForEach(doc => DocumentLookup[doc.DocumentId] = doc);

                var venueDocs = await venueDocuments.FetchVenueVenueDocuments(venueId, 0, int.MaxValue)
                    .Ensure(i => i.HasValue, "Venue Documents were found")
                    .OnSuccess(i => i.Value)
                    .OnBoth((Result<List<VenueDocument>> i) => i.IsSuccess ? i.Value : new List<VenueDocument>())
                    .ConfigureAwait(false);

                var openingTimes = await venueOpeningTimes.FetchVenueVenueOpeningTimes(venue.Value.VenueId)
                    .Ensure(o => o.HasValue, "Opening times were found")
                    .OnSuccess(o => o.Value)
                    .OnBoth(o => o.IsSuccess ? o.Value : new List<VenueOpeningTime>())
                    .ConfigureAwait(false);

                VenueOpeningTime sundayTime = null;
                VenueOpeningTime mondayTime = null;
                VenueOpeningTime tuesdayTime = null;
                VenueOpeningTime wednesdayTime = null;
                VenueOpeningTime thursdayTime = null;
                VenueOpeningTime fridayTime = null;
                VenueOpeningTime saturdayTime = null;

                foreach (var openingTime in openingTimes)
                {
                    switch (openingTime.DayOfWeek)
                    {
                        case 0:
                            sundayTime = openingTime;
                            break;
                        case 1:
                            mondayTime = openingTime;
                            break;
                        case 2:
                            tuesdayTime = openingTime;
                            break;
                        case 3:
                            wednesdayTime = openingTime;
                            break;
                        case 4:
                            thursdayTime = openingTime;
                            break;
                        case 5:
                            fridayTime = openingTime;
                            break;
                        case 6:
                            saturdayTime = openingTime;
                            break;
                    }
                }

                Input ??= new InputModel
                {
                    VenueName = venue.Value.VenueName,
                    VenueAddress = venue.Value.VenueAddress,
                    VenueAddress2 = venue.Value.VenueAddress2,
                    VenueAddress3 = venue.Value.VenueAddress3,
                    VenueCounty = venue.Value.VenueCounty,
                    VenuePostCode = venue.Value.VenuePostCode,
                    VenueContact = venue.Value.VenueContact,
                    VenuePhone = venue.Value.VenuePhone,
                    VenueDescription = venue.Value.VenueDescription,
                    ImageId = venue.Value.ImageId == null ? null : venue.Value.ImageId.ToString(),
                    Tags = string.Join(',', venueTags.Value.Select(t => t.TagName).ToList()),
                    DocumentIds = venueDocs.Select(id => id.DocumentId.ToString()).ToList(),
                    VenueProgress = venue.Value.Progress,
                    ServingType = venue.Value.ServingType,
                    SundayOpeningTimeEnabled = sundayTime != null,
                    SundayOpeningTimeStart = sundayTime?.StartTime,
                    SundayOpeningTimeEnd = sundayTime?.EndTime,
                    MondayOpeningTimeEnabled = mondayTime != null,
                    MondayOpeningTimeStart = mondayTime?.StartTime,
                    MondayOpeningTimeEnd = mondayTime?.EndTime,
                    TuesdayOpeningTimeEnabled = tuesdayTime != null,
                    TuesdayOpeningTimeStart = tuesdayTime?.StartTime,
                    TuesdayOpeningTimeEnd = tuesdayTime?.EndTime,
                    WednesdayOpeningTimeEnabled = wednesdayTime != null,
                    WednesdayOpeningTimeStart = wednesdayTime?.StartTime,
                    WednesdayOpeningTimeEnd = wednesdayTime?.EndTime,
                    ThursdayOpeningTimeEnabled = thursdayTime != null,
                    ThursdayOpeningTimeStart = thursdayTime?.StartTime,
                    ThursdayOpeningTimeEnd = thursdayTime?.EndTime,
                    FridayOpeningTimeEnabled = fridayTime != null,
                    FridayOpeningTimeStart = fridayTime?.StartTime,
                    FridayOpeningTimeEnd = fridayTime?.EndTime,
                    SaturdayOpeningTimeEnabled = saturdayTime != null,
                    SaturdayOpeningTimeStart = saturdayTime?.StartTime,
                    SaturdayOpeningTimeEnd = saturdayTime?.EndTime,
                };

                return true;
            }

            return false;
        }

        private async Task UpdateVenuesWithExternalPaymentProvider(int companyId)
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
                                Data = new DtoVenueSyncMessage(Guid.NewGuid(), VenueSyncAction.Update, accessToken, company)
                            });
                    }
                }
            }
            catch (Exception e)
            {
                // TODO: Something here
            }
        }

        private async Task UpdateVenueOpeningTimes()
        {
            await venueOpeningTimes.UpsertVenueOpeningTimes(
                Venue.VenueId,
                Input.SundayOpeningTimeEnabled ? Input.SundayOpeningTimeStart : null,
                Input.SundayOpeningTimeEnabled ? Input.SundayOpeningTimeEnd : null,
                Input.MondayOpeningTimeEnabled ? Input.MondayOpeningTimeStart : null,
                Input.MondayOpeningTimeEnabled ? Input.MondayOpeningTimeEnd : null,
                Input.TuesdayOpeningTimeEnabled ? Input.TuesdayOpeningTimeStart : null,
                Input.TuesdayOpeningTimeEnabled ? Input.TuesdayOpeningTimeEnd : null,
                Input.WednesdayOpeningTimeEnabled ? Input.WednesdayOpeningTimeStart : null,
                Input.WednesdayOpeningTimeEnabled ? Input.WednesdayOpeningTimeEnd : null,
                Input.ThursdayOpeningTimeEnabled ? Input.ThursdayOpeningTimeStart : null,
                Input.ThursdayOpeningTimeEnabled ? Input.ThursdayOpeningTimeEnd : null,
                Input.FridayOpeningTimeEnabled ? Input.FridayOpeningTimeStart : null,
                Input.FridayOpeningTimeEnabled ? Input.FridayOpeningTimeEnd : null,
                Input.SaturdayOpeningTimeEnabled ? Input.SaturdayOpeningTimeStart : null,
                Input.SaturdayOpeningTimeEnabled ? Input.SaturdayOpeningTimeEnd : null
            ).ConfigureAwait(false);
        }
    }
}
