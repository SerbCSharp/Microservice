using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Catalog.API.EventBus.RabbitMQ
{
    public class RabbitMQBus : IEventBus
    {
        const string BROKER_NAME = "eshop_event_bus";
        private readonly RabbitMqConfiguration _rabbitMqConfiguration;
        private readonly ILogger<RabbitMQBus> _logger;
        private IConnection _connection;
        private readonly string _queueName;

        public RabbitMQBus(IOptions<RabbitMqConfiguration> rabbitMqConfiguration, ILogger<RabbitMQBus> logger)
        {
            _rabbitMqConfiguration = rabbitMqConfiguration.Value ?? throw new ArgumentNullException(nameof(rabbitMqConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queueName = _rabbitMqConfiguration.SubscriptionClientName;
        }

        public string Receive<T>() where T : IntegrationEvent
        {
            var eventName = typeof(T).Name;
            string message = string.Empty;

            if (ConnectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: BROKER_NAME, type: ExchangeType.Direct);
                    channel.QueueDeclare(queue: _queueName, autoDelete: false);
                    channel.QueueBind(queue: _queueName,
                                      exchange: BROKER_NAME,
                                      routingKey: eventName);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        message = Encoding.UTF8.GetString(body);
                    };
                    channel.BasicConsume(queue: _queueName,
                                        autoAck: true,
                                        consumer: consumer);
                }
            }
            return message;
        }

        public void Send(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;
            if (ConnectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: BROKER_NAME, type: ExchangeType.Direct);
                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message);                 
                    channel.BasicPublish(exchange: BROKER_NAME,
                                            routingKey: eventName,
                                            basicProperties: null,
                                            body: body);
                }
            }
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _rabbitMqConfiguration.Host };
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not create connection: {exMessage}", ex.Message);
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
                return true;

            CreateConnection();
            return _connection != null;
        }
    }
}
