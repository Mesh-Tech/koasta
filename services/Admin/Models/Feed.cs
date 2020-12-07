using System;
using System.Collections.Generic;

namespace Koasta.Service.Admin.Models
{
    public class Feed
    {
        public Feed(string description, DateTime lastUpdated, List<FeedItem> items)
        {
            Description = description;
            LastUpdated = lastUpdated;
            Items = items;
        }

        public string Description { get; }
        public DateTime LastUpdated { get; }
        public List<FeedItem> Items { get; }
    }

    public class FeedItem
    {
        public FeedItem(string title, string link, string description, DateTime publishDate)
        {
            this.Title = title;
            this.Link = link;
            this.Description = description;
            this.PublishDate = publishDate;
        }

        public string Title { get; }
        public string Link { get; }
        public string Description { get; }
        public DateTime PublishDate { get; }
    }
}
