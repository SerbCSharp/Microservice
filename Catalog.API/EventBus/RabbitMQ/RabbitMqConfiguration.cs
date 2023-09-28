namespace Catalog.API.EventBus.RabbitMQ
{
    public class RabbitMqConfiguration
    {
        public const string Section = "RabbitMQ";
        public string Host { get; set; }
        public string SubscriptionClientName { get; set; }
    }
}
