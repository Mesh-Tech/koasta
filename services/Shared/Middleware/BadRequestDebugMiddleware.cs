using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Koasta.Shared.Middleware
{
    internal class BadRequestDebugMiddleware : IAsyncActionFilter
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public BadRequestDebugMiddleware()
        {
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ModelState.IsValid)
            {
                await next().ConfigureAwait(false);
                return;
            }

            var allErrors = context.ModelState.Values.SelectMany(v => v.Errors)
              .Select(e => new ValidationError
              {
                  Error = e.ErrorMessage
              })
              .ToList();

            var response = JsonConvert.SerializeObject(new ValidationResponse { Errors = allErrors }, jsonSerializerSettings);

            context.HttpContext.Response.StatusCode = 400;
            await context.HttpContext.Response.WriteAsync(response).ConfigureAwait(false);
        }

        private class ValidationResponse
        {
            public List<ValidationError> Errors { get; set; }
        }

        private class ValidationError
        {
            public string Error { get; set; }
        }
    }
}
