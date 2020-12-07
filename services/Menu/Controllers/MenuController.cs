using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Koasta.Service.MenuService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/menu")]
    public class MenuController : Controller
    {
        private readonly MenuRepository menus;

        public MenuController(MenuRepository menus)
        {
            this.menus = menus;
        }

        [HttpGet]
        [Route("{venueId}")]
        [ActionName("fetch_full_menu")]
        [ProducesResponseType(typeof(List<FullMenu>), 200)]
        public async Task<IActionResult> FetchVenueMenus([FromRoute(Name = "venueId")] int venueId)
        {
            return await menus.FetchMenusForVenue(venueId)
              .Ensure(m => m.HasValue, "Menus are present")
              .OnBoth(m => m.IsFailure ? StatusCode(404, "") : StatusCode(200, m.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{venueId}/{menuId}")]
        [ActionName("fetch_menu")]
        [ProducesResponseType(typeof(FullMenu), 200)]
        public async Task<IActionResult> FetchFullMenu([FromRoute(Name = "venueId")] int venueId, [FromRoute(Name = "menuId")] int menuId)
        {
            return await menus.FetchFullMenu(venueId, menuId)
              .Ensure(m => m.HasValue, "Menus are present")
              .OnBoth(m => m.IsFailure ? StatusCode(404, "") : StatusCode(200, m.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [Route("{venueId}")]
        [ActionName("create_menu")]
        public async Task<IActionResult> CreateMenu([FromRoute(Name = "venueId")] int venueId, [FromBody] NewMenu newMenu)
        {
            if (newMenu.MenuName == null || newMenu.Products == null)
            {
                return BadRequest();
            }

            newMenu.VenueId = venueId;

            return await menus.CreateFullMenu(newMenu)
              .OnBoth(m => m.IsFailure ? StatusCode(500) : StatusCode(201))
              .ConfigureAwait(false);
        }

        [HttpPut]
        [Route("{venueId}")]
        [ActionName("update_menu")]
        public async Task<IActionResult> ReplaceMenu([FromRoute(Name = "venueId")] int venueId, [FromBody] UpdatedMenu updatedMenu)
        {
            if (updatedMenu.MenuDescription == null || updatedMenu.MenuName == null || updatedMenu.Products == null)
            {
                return BadRequest();
            }

            return await menus.ReplaceFullMenu(venueId, updatedMenu)
              .OnBoth(m => m.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("{venueId}/{menuId}")]
        [ActionName("delete_menu")]
        public async Task<IActionResult> DropMenu([FromRoute(Name = "venueId")] int venueId, [FromRoute(Name = "menuId")] int menuId)
        {
            return await menus.DropVenueMenu(venueId, menuId)
              .OnBoth(m => m.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }
    }
}
