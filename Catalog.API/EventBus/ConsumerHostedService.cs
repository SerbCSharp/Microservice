using Catalog.API.IntegrationEvents;

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
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var productPriceChanged = await _eventBus.Receive<ProductPriceChangedIntegrationEvent>();
                _logger.LogInformation("ProductPriceChangedIntegrationEvent: {productPriceChanged}", productPriceChanged);
                if (productPriceChanged != null)
                {
                }
                await Task.Delay(5000);
            }
        }
    }
}
