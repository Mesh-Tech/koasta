using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Shared.Database;
using Microsoft.AspNetCore.Identity;
using Koasta.Shared.Models;
using Amazon.S3;
using Koasta.Shared.Configuration;
using Amazon;

namespace Koasta.Service.Admin.Controllers
{
    [Route("/downloads/{downloadId}")]
    public class DownloadsController : Controller
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly DocumentRepository documents;
        private readonly ISettings settings;

        public DownloadsController(UserManager<Employee> userManager,
                                   RoleManager<EmployeeRole> roleManager,
                                   DocumentRepository documents,
                                   ISettings settings)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.documents = documents;
            this.settings = settings;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(int downloadId)
        {
            var user = await userManager.GetUserAsync(User).ConfigureAwait(false);
            if (user == null)
            {
                return Unauthorized();
            }

            var role = await roleManager.FindByIdAsync(user.RoleId.ToString()).ConfigureAwait(false);
            if (role == null)
            {
                return Unauthorized();
            }

            if (!role.CanAdministerCompany)
            {
                return Unauthorized();
            }

            var result = await documents.FetchDocument(downloadId)
                .Ensure(t => t.HasValue, "Document was found")
                .OnSuccess(t => t.Value)
                .ConfigureAwait(false);

            if (result.IsFailure)
            {
                return NotFound();
            }

            var doc = result.Value;
            if (user.CompanyId != doc.CompanyId)
            {
                return Unauthorized();
            }

            using var client = new AmazonS3Client(settings.Connection.AWSAccessKeyId, settings.Connection.AWSSecretAccessKey, RegionEndpoint.USEast1);
            var obj = await client.GetObjectAsync(settings.Connection.S3PrivateBucketName, $"documents/{doc.CompanyId}__{doc.DocumentKey}__doc").ConfigureAwait(false);

            return new FileStreamResult(obj.ResponseStream, "application/octet-stream")
            {
                FileDownloadName = doc.DocumentKey
            };
        }
    }
}
