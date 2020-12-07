using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Shared.Database;
using System.Linq;
using System.Collections.Generic;

namespace Koasta.Service.Admin.Controllers
{
    [Route("/tags/suggestions")]
    [ProducesResponseType(typeof(List<string>), 200)]
    public class TagSuggestionsController : Controller
    {
        private readonly TagRepository tags;
        public TagSuggestionsController(TagRepository tags)
        {
            this.tags = tags;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestions()
        {
            return await tags.FetchTags(0, int.MaxValue)
                .Ensure(t => t.HasValue, "Tags were found")
                .OnSuccess(t => t.Value.Select(t => t.TagName).ToList())
                .OnBoth(t => t.IsSuccess ? StatusCode(200, t.Value) : StatusCode(500, null))
                .ConfigureAwait(false);
        }
    }
}
