using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public class MediaEntry
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("companyId")]
        public int CompanyId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("key")]
        public string Key { get; set; }

        [Column("mediatype")]
        public int MediaType { get; set; }
    }
}
