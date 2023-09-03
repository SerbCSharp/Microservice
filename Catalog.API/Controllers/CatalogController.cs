using Catalog.API.Infrastructure;
using Catalog.API.IntegrationEvents;
using Catalog.API.Model;
using EventBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogContext _catalogContext;
        private readonly IEventBus _eventBus;

        public CatalogController(CatalogContext context, IEventBus eventBus)
        {
            _catalogContext = context ?? throw new ArgumentNullException(nameof(context));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        [Route("items")]
        [HttpPost]
        public async Task<ActionResult> CreateProductAsync([FromBody] CatalogItem product)
        {
            var item = new CatalogItem
            {
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Description = product.Description,
                Name = product.Name,
                PictureFileName = product.PictureFileName,
                Price = product.Price,
                CatalogType = product.CatalogType,
                CatalogBrand = product.CatalogBrand
            };

            _catalogContext.CatalogItems.Add(item);
            await _catalogContext.SaveChangesAsync();

            return NoContent();
        }

        [Route("items")]
        [HttpPut]
        public async Task<ActionResult> UpdateProductAsync([FromBody] CatalogItem product)
        {
            var catalogItem = await _catalogContext.CatalogItems.SingleOrDefaultAsync(i => i.Id == product.Id);

            if (catalogItem == null)
            {
                return NotFound(new { Message = $"Item with id {product.Id} not found." });
            }
            var oldPrice = catalogItem.Price;
            var raiseProductPriceChangedEvent = oldPrice != product.Price;

            catalogItem = product;
            _catalogContext.CatalogItems.Update(catalogItem);
            await _catalogContext.SaveChangesAsync();

            if (raiseProductPriceChangedEvent)
            {
                var priceChangedEvent = new ProductPriceChangedIntegrationEvent(catalogItem.Id, product.Price, oldPrice);
                _eventBus.Publish(priceChangedEvent);
            }
            return NoContent();
        }
    }
}