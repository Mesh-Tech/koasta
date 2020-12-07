using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Koasta.Service.Admin.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
            ViewData["HideNavigation"] = true;
        }
    }
}

