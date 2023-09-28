using Catalog.API.IntegrationEvents;
using Newtonsoft.Json;

namespace Catalog.API.EventBus
{
    public class ConsumerHostedService : BackgroundService
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<ConsumerHostedService> _logger;

        public ConsumerHostedService(IEventBus eventBus, ILogger<ConsumerHostedService> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _eventBus.Receive<ProductPriceChangedIntegrationEvent>();
                if (!string.IsNullOrEmpty(message))
                {
                    var productPriceChanged = JsonConvert.DeserializeObject<ProductPriceChangedIntegrationEvent>(message);
                    _logger.LogInformation("NewPrice: {newPrice}", productPriceChanged?.NewPrice);
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
