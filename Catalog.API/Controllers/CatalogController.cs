using Catalog.API.EventBus;
using Catalog.API.Infrastructure;
using Catalog.API.IntegrationEvents;
using Catalog.API.Model;
using Microsoft.AspNetCore.Mvc;

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
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(context));
        }

        [Route("items")]
        [HttpPost]
        public async Task<ActionResult> CreateProductAsync([FromBody] CatalogItem product)
        {
            var priceChangedEvent = new ProductPriceChangedIntegrationEvent(product.Id, product.Price, 21);
            _eventBus.Send(priceChangedEvent);
            return NoContent();
        }

        [Route("items")]
        [HttpPut]
        public async Task<ActionResult> UpdateProductAsync([FromBody] CatalogItem product)
        {
            var priceChangedEvent = new ProductPriceChangedIntegrationEvent(product.Id, product.Price, 21);
            _eventBus.Send(priceChangedEvent);
            return NoContent();
        }
    }
}