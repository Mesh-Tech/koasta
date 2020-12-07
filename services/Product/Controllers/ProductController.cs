using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Service.ProductService.Models;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using System.Collections.Generic;

namespace Koasta.Service.ProductService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/product")]
    public class ProductController : Controller
    {
        private readonly ProductRepository products;
        private readonly ProductTypeRepository productTypes;

        public ProductController(ProductRepository products, ProductTypeRepository productTypes)
        {
            this.products = products;
            this.productTypes = productTypes;
        }

        [HttpGet]
        [Route("{venueId}")]
        [ActionName("fetch_products")]
        [ProducesResponseType(typeof(List<Product>), 200)]
        public async Task<IActionResult> GetVenueProducts([FromRoute(Name = "venueId")] int venueId, [FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await products.FetchVenueProducts(venueId, page, count)
              .Ensure(p => p.HasValue, "Products were found")
              .OnBoth(p => p.IsFailure ? StatusCode(404, "") : StatusCode(200, p.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("productTypes")]
        [Route("productTypes/{productId}")]
        [ActionName("fetch_product_types")]
        [ProducesResponseType(typeof(List<ProductType>), 200)]
        public async Task<IActionResult> GetProductTypes()
        {
            return await productTypes.FetchProductTypes(0, 2000)
              .Ensure(p => p.HasValue, "Product types were found")
              .OnBoth(p => p.IsFailure ? StatusCode(404, "") : StatusCode(200, p.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{venueId}/{productId}")]
        [ActionName("fetch_product")]
        [ProducesResponseType(typeof(Product), 200)]
        public async Task<IActionResult> GetVenueProduct([FromRoute(Name = "venueId")] int venueId, [FromRoute(Name = "productId")] int productId)
        {
            return await products.FetchVenueProduct(venueId, productId)
              .Ensure(p => p.HasValue, "Product was found")
              .OnBoth(p => p.IsFailure ? StatusCode(404, "") : StatusCode(200, p.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [Route("{venueId}")]
        [ActionName("create_product")]
        public async Task<IActionResult> CreateVenueProduct([FromRoute(Name = "venueId")] int venueId, [FromBody] DtoNewProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var product = new NewProduct
            {
                VenueId = venueId,
                ProductTypeId = request.ProductTypeId,
                ProductName = request.ProductName,
                ProductDescription = request.ProductDescription,
                Price = request.Price,
                Image = request.Image,
                AgeRestricted = request.AgeRestricted,
                ParentProductId = request.ParentProductId,
            };

            return await products.CreateVenueProduct(product)
              .Ensure(p => p.HasValue, "Product was created")
              .OnBoth(p => p.IsFailure ? StatusCode(500) : StatusCode(201))
              .ConfigureAwait(false);
        }

        [HttpPut]
        [Route("{venueId}")]
        [ActionName("update_product")]
        public async Task<IActionResult> ReplaceVenueProduct([FromRoute(Name = "venueId")] int venueId, [FromBody] DtoReplaceProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var product = new UpdatedProduct
            {
                VenueId = venueId,
                ProductId = request.ProductId,
                ProductTypeId = request.ProductTypeId,
                ProductName = request.ProductName,
                ProductDescription = request.ProductDescription,
                Price = request.Price,
                Image = request.Image,
                AgeRestricted = request.AgeRestricted,
                ParentProductId = request.ParentProductId,
            };

            return await products.ReplaceVenueProduct(product)
              .OnBoth(p => p.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("{venueId}/{productId}")]
        [ActionName("delete_product")]
        public async Task<IActionResult> DropVenueProduct([FromRoute(Name = "venueId")] int venueId, [FromRoute(Name = "productId")] int productId)
        {
            return await products.DropVenueProduct(venueId, productId)
              .OnBoth(p => p.IsFailure ? StatusCode(404) : StatusCode(200))
              .ConfigureAwait(false);
        }
    }
}
