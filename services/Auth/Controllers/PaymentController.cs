using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Koasta.Service.Auth.Models;
using Koasta.Shared.Billing;
using Koasta.Shared.Middleware;

namespace Koasta.Service.Auth.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/auth/payment-key")]
    public class PaymentController : Controller
    {
        private readonly IBillingManager billingManager;

        public PaymentController(IBillingManager billingManager)
        {
            this.billingManager = billingManager;
        }

        [HttpPost]
        [ActionName("create_ephemeral_key")]
        public async Task<IActionResult> CreateEphemeralKey()
        {
            try
            {
                var key = await billingManager.CreateEphemeralKey(this.GetAuthContext().User.Value.ExternalPaymentProcessorId).ConfigureAwait(false);
                return Ok(new DtoOrderKey { Key = Newtonsoft.Json.Linq.JObject.Parse(key) });
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
