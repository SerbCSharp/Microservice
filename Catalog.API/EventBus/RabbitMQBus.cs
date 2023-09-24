using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Catalog.API.EventBus
{
    public class RabbitMQBus : IEventBus
    {
        const string BROKER_NAME = "eshop_event_bus";
        private readonly RabbitMqConfiguration _rabbitMqConfiguration;
        private readonly ILogger<RabbitMQBus> _logger;
        private IConnection _connection;

        public RabbitMQBus(IOptions<RabbitMqConfiguration> rabbitMqConfiguration, ILogger<RabbitMQBus> logger)
        {
            _rabbitMqConfiguration = rabbitMqConfiguration.Value ?? throw new ArgumentNullException(nameof(rabbitMqConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Receive()
        {
            var factory = new ConnectionFactory() { HostName = _rabbitMqConfiguration.Host };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (sender, e) =>
                {

                    var body = e.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    var json = JsonConvert.DeserializeObject<ConversationMessage>(message);
                    SystemMessageModel systemMessage = JsonConvert.DeserializeObject<SystemMessageModel>(json.Message);
                    Console.WriteLine($"{systemMessage.UI.Message.value}");
                };
                channel.BasicConsume(queue: "Abdt.Babbai.RabbitContracts.Messages.Aimee.ConversationMessage, Abdt.Babbai.RabbitContracts_Abdt.Aimee",
                                    autoAck: true,
                                    consumer: consumer);
                Console.ReadLine();
            }
        }

        public void Send(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;
            if (ConnectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: BROKER_NAME, type: "direct");
                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent
                    properties.Type = "Abdt.Babbai.RabbitContracts.Messages.Aimee.ConversationMessage, Abdt.Babbai.RabbitContracts";

                    channel.BasicPublish(exchange: BROKER_NAME,
                                            routingKey: eventName,
                                            basicProperties: properties,
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
