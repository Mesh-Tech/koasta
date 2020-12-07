using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Koasta.Service.Admin.Utils
{
    public static class PageModelExtensions
    {
        public static IActionResult TurboRedirectToPage(this PageModel pm, string page)
        {
            if (pm.Request.Headers.ContainsKey("x-request-source") && pm.Request.Headers["x-request-source"].Equals("turbolinks"))
            {
                var route = pm.Url.Page(page);
                return pm.Content($"Turbolinks.visit('{route}')");
            }

            return pm.RedirectToPage(page);
        }

        public static IActionResult TurboPage(this PageModel pm)
        {
            if (pm.Request.Headers.ContainsKey("x-request-source") && pm.Request.Headers["x-request-source"].Equals("turbolinks"))
            {
                pm.Response.StatusCode = 400;
                return pm.Page();
            }

            return pm.Page();
        }
    }
}
