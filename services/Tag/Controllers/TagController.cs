using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Service.TagService.Models;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Koasta.Service.TagService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/tags")]
    public class TagController : Controller
    {
        private readonly TagRepository tags;

        public TagController(TagRepository tags)
        {
            this.tags = tags;
        }

        [HttpGet]
        [ActionName("fetch_tags")]
        [ProducesResponseType(typeof(List<Tag>), 200)]
        public async Task<IActionResult> GetTags([FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await tags.FetchTags(page, count)
              .Ensure(tags => tags.HasValue, "Tags were found")
              .OnBoth(tags => tags.IsFailure ? StatusCode(404, "") : StatusCode(200, tags.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_tag")]
        [Route("{tagId}")]
        [ProducesResponseType(typeof(Tag), 200)]
        public async Task<IActionResult> GetTag([FromRoute(Name = "tagId")] int tagId)
        {
            return await tags.FetchTag(tagId)
              .Ensure(tag => tag.HasValue, "Tag was found")
              .OnBoth(tag => tag.IsFailure ? StatusCode(404, "") : StatusCode(200, tag.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [ActionName("create_tag")]
        [ProducesResponseType(typeof(List<Tag>), 200)]
        public async Task<IActionResult> CreateTag([FromBody] DtoNewTag newTag)
        {
            var tag = new Tag
            {
                TagName = newTag.TagName
            };
            return await tags.CreateTag(tag)
              .Ensure(t => t.HasValue, "Tag successfully created")
              .OnSuccess(t => new Tag { TagId = t.Value, TagName = tag.TagName })
              .OnBoth(t => t.IsFailure ? StatusCode(404, "") : StatusCode(200, t.Value))
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [ActionName("delete_tag")]
        [Route("{tagId}")]
        public async Task<IActionResult> DropTag([FromRoute(Name = "tagId")] int tagId)
        {
            return await tags.DropTag(tagId)
              .OnBoth(tag => tag.IsFailure ? StatusCode(404) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpPut]
        [ActionName("create_tag")]
        [Route("{tagId}")]
        public async Task<IActionResult> ReplaceTag([FromRoute(Name = "tagId")] int tagId, [FromBody] DtoNewTag newTag)
        {
            var tag = new Tag
            {
                TagId = tagId,
                TagName = newTag.TagName
            };
            return await tags.ReplaceTag(tag)
              .OnBoth(t => t.IsFailure ? StatusCode(404) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_venue_tags")]
        [Route("venue/{venueId}")]
        [ProducesResponseType(typeof(List<Tag>), 200)]
        public async Task<IActionResult> GetVenueTags([FromRoute(Name = "venueId")] int venueId)
        {
            return await tags.FetchVenueTags(venueId)
              .Ensure(tags => tags.HasValue, "Tags were found")
              .OnBoth(tags => tags.IsFailure ? StatusCode(404, "") : StatusCode(200, tags.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpPatch]
        [ActionName("add_venue_tags")]
        [Route("venue/{venueId}")]
        public async Task<IActionResult> AddVenueTags([FromRoute(Name = "venueId")] int venueId, [FromBody] DtoTagsReq request)
        {
            return await tags.CreateVenueTags(venueId, request.Tags)
              .OnBoth(tags => tags.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [ActionName("remove_venue_tags")]
        [Route("venue/{venueId}")]
        public async Task<IActionResult> RemoveVenueTags([FromRoute(Name = "venueId")] int venueId, [FromBody] DtoTagsReq request)
        {
            return await tags.DropVenueTags(venueId, request.Tags)
              .OnBoth(tags => tags.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }
    }
}
