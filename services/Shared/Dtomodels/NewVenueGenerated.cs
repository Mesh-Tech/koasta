using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class NewVenue
    {
        [Column("companyId")]
        public int CompanyId { get; set; }
        [Column("venueName")]
        public string VenueName { get; set; }
        [Column("venueCode")]
        public string VenueCode { get; set; }
        [Column("venueAddress")]
        public string VenueAddress { get; set; }
        [Column("venueAddress2")]
        public string VenueAddress2 { get; set; }
        [Column("venueAddress3")]
        public string VenueAddress3 { get; set; }
        [Column("venueCounty")]
        public string VenueCounty { get; set; }
        [Column("venuePostCode")]
        public string VenuePostCode { get; set; }
        [Column("venueContact")]
        public string VenueContact { get; set; }
        [Column("venueDescription")]
        public string VenueDescription { get; set; }
        [Column("venueNotes")]
        public string VenueNotes { get; set; }
        [Column("venueLatitude")]
        public string VenueLatitude { get; set; }
        [Column("venueLongitude")]
        public string VenueLongitude { get; set; }
        [Column("imageId")]
        public int? ImageId { get; set; }
        [Column("venuePhone")]
        public string VenuePhone { get; set; }
    }
}
