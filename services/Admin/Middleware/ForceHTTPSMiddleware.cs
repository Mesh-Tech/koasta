using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Koasta.Service.Admin.Middleware
{
    internal class ForceHTTPSMiddleware
    {
        private readonly RequestDelegate next;

        public ForceHTTPSMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (context.Request == null)
            {
                return this.next(context);
            }

            var request = context.Request;

            // #1) Did this request start off as HTTP?
            string reqProtocol;
            if (request.Headers.ContainsKey("X-Forwarded-Proto"))
            {
                reqProtocol = request.Headers["X-Forwarded-Proto"][0];
            }
            else
            {
                reqProtocol = (request.IsHttps ? "https" : "http");
            }

            // #2) If so, redirect to HTTPS equivalent
            if (reqProtocol != "https")
            {
                var newUrl = new StringBuilder()
                    .Append("https://").Append(request.Host)
                    .Append(request.PathBase).Append(request.Path)
                    .Append(request.QueryString);

                context.Response.Redirect(newUrl.ToString(), true);
                return Task.CompletedTask;
            }

            return this.next(context);
        }
    }
}
