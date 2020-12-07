using Microsoft.AspNetCore.Mvc;
using Koasta.Shared.Middleware;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using Koasta.Shared.Database;
using Newtonsoft.Json;
using Koasta.Service.FlagService.Models;
using System.Linq;
using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using Koasta.Shared.Models;
using Koasta.Service.Flags.Models;
using Newtonsoft.Json.Serialization;

namespace Koasta.Service.FlagService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/flags")]
    public class FlagController : Controller
    {
        private readonly IDistributedCache cache;
        private readonly FeatureFlagRepository flags;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private const string FeatureFlagsKey = "current_flags";

        public FlagController(IDistributedCache cache, FeatureFlagRepository flags)
        {
            this.cache = cache;
            this.flags = flags;
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<FeatureFlag>), 200)]
        [ProducesResponseType(404)]
        [ActionName("fetch_flags")]
        public async Task<IActionResult> GetFlags([FromQuery] int page = 0, [FromQuery] int count = 20)
        {
            return await flags.FetchFeatureFlags(page, count)
                .Ensure(f => f.HasValue, "Flags were found")
                .OnSuccess(f => f.Value)
                .OnBoth(f => f.IsSuccess ? StatusCode(200, f.Value) : StatusCode(404, null))
                .ConfigureAwait(false);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FeatureFlag), 200)]
        [ProducesResponseType(404)]
        [ActionName("fetch_flag")]
        public async Task<IActionResult> GetFlag(int id)
        {
            return await flags.FetchFeatureFlag(id)
                .Ensure(f => f.HasValue, "Flag was found")
                .OnSuccess(f => f.Value)
                .OnBoth(f => f.IsSuccess ? StatusCode(200, f.Value) : StatusCode(404, null))
                .ConfigureAwait(false);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ActionName("update_flag")]
        public async Task<IActionResult> UpdateFlag(int id, [FromBody] DtoFlagUpdate update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return await flags.ReplaceFeatureFlag(new FeatureFlag {
                FlagId = id,
                Name = update.Name,
                Description = update.Description,
                Value = update.Value
            })
                .OnBoth(f => f.IsSuccess ? StatusCode(200) : StatusCode(500))
                .ConfigureAwait(false);
        }

        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [ActionName("create_flag")]
        public async Task<IActionResult> CreateFlag([FromBody] DtoFlagUpdate update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return await flags.CreateFeatureFlag(new FeatureFlag
            {
                Name = update.Name,
                Description = update.Description,
                Value = update.Value
            })
                .Ensure(f => f.HasValue, "Flag created")
                .OnBoth(f => f.IsSuccess ? StatusCode(200) : StatusCode(500))
                .ConfigureAwait(false);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [ActionName("delete_flag")]
        public async Task<IActionResult> DeleteFlag(int id)
        {
            return await flags.DropFeatureFlag(id)
                .OnBoth(f => f.IsSuccess ? StatusCode(200) : StatusCode(500))
                .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("current")]
        [ProducesResponseType(typeof(DtoFlagsDescription), 200)]
        [ProducesResponseType(500)]
        [ActionName("fetch_current_flags")]
        public async Task<IActionResult> GetCurrentFlags()
        {
            var data = await cache.GetStringAsync(FeatureFlagsKey).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(data))
            {
                var allFlags = await flags.FetchFeatureFlags(0, int.MaxValue)
                .Ensure(f => f.HasValue, "Flags were found")
                .OnSuccess(f => f.Value)
                .ConfigureAwait(false);

                if (allFlags.IsFailure)
                {
                    return StatusCode(500);
                }

                var description = new DtoFlagsDescription {
                    Flags = allFlags.Value.ToDictionary(val => val.Name, val => val.Value)
                };

                var json = JsonConvert.SerializeObject(description, jsonSerializerSettings);

                await cache.SetStringAsync(
                    FeatureFlagsKey,
                    json,
                    new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                ).ConfigureAwait(false);

                return Content(json, "application/json");
            }
            else
            {
                return Content(data, "application/json");
            }
        }
    }
}
