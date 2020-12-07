using Koasta.Service.Admin.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Koasta.Service.Admin.Services
{
    public class FeedService : IFeedService
    {
        private readonly string feedPath;
        private Feed cachedFeed;

        public FeedService(IWebHostEnvironment env)
        {
            feedPath = env.WebRootFileProvider.GetFileInfo("news.rss")?.PhysicalPath;
        }

        public async Task<Feed> GetFeed()
        {
            if (cachedFeed != null)
            {
                return cachedFeed;
            }

            if (string.IsNullOrWhiteSpace(feedPath))
            {
                return new Feed (
                    "Latest news from Koasta",
                    DateTime.Now,
                    new List<FeedItem>()
                );
            }

            var items = new List<FeedItem>();
            string description = "";
            DateTime pubDate = DateTime.Now;

            try
            {
                using var xmlReader = XmlReader.Create(feedPath, new XmlReaderSettings() { Async = true });
                var feedReader = new RssFeedReader(xmlReader);

                while (await feedReader.Read().ConfigureAwait(false))
                {
                    switch (feedReader.ElementType)
                    {
                        // Read Item
                        case SyndicationElementType.Item:
                            var item = await feedReader.ReadItem().ConfigureAwait(false);

                            items.Add(new FeedItem(
                                item.Title,
                                item.Links.FirstOrDefault()?.Uri?.ToString(),
                                item.Description,
                                item.Published.DateTime
                            ));
                            break;

                        default:
                            var content = await feedReader.ReadContent().ConfigureAwait(false);

                            if (content.Name.Equals("description", StringComparison.OrdinalIgnoreCase))
                            {
                                description = content.Value;
                            }
                            else if (content.Name.Equals("pubDate", StringComparison.OrdinalIgnoreCase))
                            {
                                pubDate = DateTime.Parse(content.Value);
                            }

                            break;
                    }
                }
            }
            catch (Exception)
            {
                return new Feed(
                    "Latest news from Koasta",
                    DateTime.Now,
                    new List<FeedItem>()
                );
            }

            var ret = new Feed(
                description,
                pubDate,
                items.OrderByDescending(i => i.PublishDate.Ticks).ToList()
            );
            cachedFeed = ret;

            return ret;
        }
    }
}
