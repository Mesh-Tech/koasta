using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Koasta.Shared.Types;
using Koasta.Service.UserService.Models;

namespace Koasta.Service.UserService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/users")]
    public class UserController : Controller
    {
        private readonly UserRepository users;
        private readonly ReviewRepository reviews;

        public UserController(UserRepository users, ReviewRepository reviews)
        {
            this.reviews = reviews;
            this.users = users;
        }

        [HttpGet]
        [ActionName("list_users")]
        [ProducesResponseType(typeof(List<User>), 200)]
        public async Task<IActionResult> GetUsers([FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await users.FetchUsers(page, count)
              .Ensure(u => u.HasValue, "Users were found")
              .OnBoth(u => u.IsFailure ? StatusCode(404, "") : StatusCode(200, u.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("me")]
        [ActionName("fetch_own_user")]
        [ProducesResponseType(typeof(DtoUserProfile), 200)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var authContext = this.GetAuthContext();
            if (authContext.UserType != UserType.User || !authContext.User.HasValue)
            {
                return Unauthorized();
            }

            var user = authContext.User.Value;
            var votes = await reviews.FetchUserVenueVotes(user.UserId)
                .Ensure(r => r.HasValue, "Reviews found")
                .OnSuccess(r => r.Value)
                .OnBoth(r => r.IsSuccess ? r.Value : new List<int>())
                .ConfigureAwait(false);

            return Ok(new DtoUserProfile
            {
                RegistrationId = user.RegistrationId,
                VotedVenueIds = votes,
                WantAdvertising = user.WantAdvertising,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        [HttpGet]
        [Route("{userId}")]
        [ActionName("fetch_user")]
        [ProducesResponseType(typeof(User), 200)]
        public async Task<IActionResult> GetSingleUser([FromRoute(Name = "userId")] int userId)
        {
            return await users.FetchUser(userId)
              .Ensure(u => u.HasValue, "User was found")
              .OnBoth(u => u.IsFailure ? StatusCode(404, "") : StatusCode(200, u.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [ActionName("create_user")]
        public async Task<IActionResult> CreateUser([FromBody] NewUser request)
        {
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Dob = request.Dob,
                IsVerified = request.IsVerified,
                WantAdvertising = request.WantAdvertising,
                RegistrationId = request.RegistrationId,
            };

            return await users.CreateUser(user)
              .Ensure(u => u.HasValue, "User created")
              .OnBoth(u => u.IsFailure ? StatusCode(500) : StatusCode(201))
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("{userId}")]
        [ActionName("delete_user")]
        public async Task<IActionResult> DropUser([FromRoute(Name = "userId")] int id)
        {
            return await users.DropUser(id)
              .OnBoth(u => u.IsFailure ? StatusCode(404) : StatusCode(200))
              .ConfigureAwait(false);
        }
    }
}
