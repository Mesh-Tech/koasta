using System;

namespace Koasta.Service.Admin.Models
{
    public enum MediaFileType
    {
        None,
        Image = 1,
        Document = 2
    }

    public class MediaFile
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public Uri ThumbnailUrl { get; set; }
        public MediaFileType Type { get; set; }
    }
}
