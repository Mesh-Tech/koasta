using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class VenuePatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> VenueId { get; set; }
        public PatchOperation<int> CompanyId { get; set; }
        public PatchOperation<string> VenueCode { get; set; }
        public PatchOperation<string> VenueName { get; set; }
        public PatchOperation<string> VenueAddress { get; set; }
        public PatchOperation<string> VenueAddress2 { get; set; }
        public PatchOperation<string> VenueAddress3 { get; set; }
        public PatchOperation<string> VenueCounty { get; set; }
        public PatchOperation<string> VenuePostCode { get; set; }
        public PatchOperation<string> VenuePhone { get; set; }
        public PatchOperation<string> VenueContact { get; set; }
        public PatchOperation<string> VenueDescription { get; set; }
        public PatchOperation<string> VenueNotes { get; set; }
        public PatchOperation<int> ImageId { get; set; }
        public PatchOperation<string> VenueLatitude { get; set; }
        public PatchOperation<string> VenueLongitude { get; set; }
        public PatchOperation<string> ImageUrl { get; set; }
        public PatchOperation<bool> IsOpen { get; set; }
        public PatchOperation<string> ExternalLocationId { get; set; }
        public PatchOperation<int> VerificationStatus { get; set; }
        public PatchOperation<string> ReferenceCode { get; set; }
        public PatchOperation<int> Progress { get; set; }
        public PatchOperation<int> ServingType { get; set; }
    }
}
