using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Koasta.Service.UserService.Models;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using System;
using System.Threading.Tasks;

namespace Koasta.Service.UserService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/users/me/devices")]
    public class DeviceController : Controller
    {
        private readonly DeviceRepository devices;
        private readonly ILogger logger;

        public DeviceController(DeviceRepository devices, ILoggerFactory logger)
        {
            this.devices = devices;
            this.logger = logger.CreateLogger("DeviceController");
        }

        [HttpPost]
        [ActionName("create_device")]
        public async Task<IActionResult> CreateDevice([FromBody] DtoNewDeviceModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var device = new Device
            {
                UserId = this.GetAuthContext().User.Value.UserId,
                Token = request.Token,
                Platform = request.Platform,
                UpdateTimestamp = DateTime.Now,
            };

            return await devices.CreateDevice(device)
              .OnFailure(_ => logger.LogError($"Failed to create device with token: {request.Token} platform: {request.Platform} for user: {this.GetAuthContext().User.Value.UserId}"))
              .OnBoth(_ => StatusCode(201))
              .ConfigureAwait(false);
        }
    }
}
