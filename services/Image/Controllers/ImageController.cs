using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Koasta.Service.ImageService.Models;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Koasta.Service.ImageService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/images")]
    public class ImageController : Controller
    {
        private readonly ImageRepository images;
        private readonly ISettings settings;

        public ImageController(ImageRepository images, ISettings settings)
        {
            this.images = images;
            this.settings = settings;
        }

        [HttpGet]
        [Route("{companyId}")]
        [ActionName("fetch_images")]
        [ProducesResponseType(typeof(List<DtoImage>), 200)]
        public async Task<IActionResult> FetchCompanyImages([FromRoute(Name = "companyId")] int companyId, [FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await images.FetchCompanyImages(companyId, page, count)
              .Ensure(i => i.HasValue, "Images were found")
              .OnSuccess(i => i.Value.Select(image => new DtoImage
              {
                  ImageId = image.ImageId,
                  ImageKey = image.ImageKey,
                  CompanyId = image.CompanyId,
                  ImageTitle = image.ImageTitle,
                  ImageUrl = new Uri($"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{companyId}__{image.ImageKey}__img"),
              }).ToList())
              .OnSuccess(i => new { Images = i })
              .OnBoth(i => i.IsFailure ? StatusCode(404, "") : StatusCode(200, i.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{companyId}/{imageId}")]
        [ActionName("fetch_image")]
        [ProducesResponseType(typeof(DtoImage), 200)]
        public async Task<IActionResult> FetchCompanyImage([FromRoute(Name = "companyId")] int companyId, [FromRoute(Name = "imageId")] int imageId)
        {
            return await images.FetchCompanyImage(companyId, imageId)
              .Ensure(i => i.HasValue, "Image was found")
              .OnSuccess(image => new DtoImage
              {
                  ImageId = image.Value.ImageId,
                  ImageKey = image.Value.ImageKey,
                  CompanyId = image.Value.CompanyId,
                  ImageTitle = image.Value.ImageTitle,
                  ImageUrl = new Uri($"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{companyId}__{image.Value.ImageKey}__img"),
              })
              .OnBoth(i => i.IsFailure ? StatusCode(404, "") : StatusCode(200, i.Value))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [Route("{companyId}")]
        [ActionName("create_image")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> CreateCompanyImage([FromRoute(Name = "companyId")] int companyId, [FromBody] DtoNewImage request)
        {
            var image = new Image
            {
                CompanyId = companyId,
                ImageKey = request.ImageKey,
                ImageTitle = request.ImageTitle,
            };

            return await images.CreateImage(image)
              .Ensure(i => i.HasValue, "Image was found")
              .OnSuccess(i => new
              {
                  ImageId = i.Value,
              })
              .OnBoth(i => i.IsFailure ? StatusCode(500, "") : StatusCode(200, i.Value))
              .ConfigureAwait(false);
        }

        [HttpPut]
        [Route("{companyId}/{imageId}")]
        [ActionName("update_image")]
        public async Task<IActionResult> UpdateCompanyImage([FromRoute(Name = "companyId")] int companyId, [FromRoute(Name = "imageId")] int imageId, [FromBody] DtoImageImage request)
        {
            return await images.UpdateCompanyImage(companyId, imageId, request.ImageTitle)
              .OnBoth(i => i.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [Route("{companyId}/{imageId}")]
        [ActionName("upload_image")]
        public async Task<IActionResult> UploadImage([FromRoute(Name = "companyId")] int companyId, [FromRoute(Name = "imageId")] int imageId, IFormFile file)
        {
            return await images.FetchCompanyImage(companyId, imageId)
              .Ensure(i => i.HasValue, "Image was found")
              .OnSuccess(i => UploadImageToS3(i.Value, file))
              .OnBoth(i => i.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("{companyId}/{imageId}")]
        [ActionName("delete_image")]
        public async Task<IActionResult> DropImage([FromRoute(Name = "companyId")] int companyId, [FromRoute(Name = "imageId")] int imageId)
        {
            var imageResult = await images.FetchCompanyImage(companyId, imageId).ConfigureAwait(false);
            if (imageResult.IsFailure)
            {
                return StatusCode(500);
            }
            else if (imageResult.Value.HasNoValue)
            {
                return BadRequest();
            }

            var image = imageResult.Value.Value;

            return await images.DropImage(imageId)
              .OnSuccess(() => DeleteImageFromS3(image))
              .OnBoth(i => i.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }

        private async Task<Result> UploadImageToS3(Image image, IFormFile file)
        {
            using var client = new AmazonS3Client(settings.Connection.AWSAccessKeyId, settings.Connection.AWSSecretAccessKey, RegionEndpoint.EUWest1);
            using var s3 = new TransferUtility(client);

            try
            {
                await s3.UploadAsync(file.OpenReadStream(), settings.Connection.S3BucketName, $"images/{image.CompanyId}__{image.ImageKey}__img").ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Fail("Failed to upload to S3");
            }
        }

        private async Task<Result> DeleteImageFromS3(Image image)
        {
            using var client = new AmazonS3Client(settings.Connection.AWSAccessKeyId, settings.Connection.AWSSecretAccessKey, RegionEndpoint.EUWest1);

            try
            {
                var req = new DeleteObjectRequest
                {
                    BucketName = settings.Connection.S3BucketName,
                    Key = $"images/{image.CompanyId}__{image.ImageKey}__img",
                };
                await client.DeleteObjectAsync(req).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception)
            {
                return Result.Fail("Failed to upload to S3");
            }
        }
    }
}
