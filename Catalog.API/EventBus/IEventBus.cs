namespace Catalog.API.EventBus
{
    public interface IEventBus
    {
        void Send(IntegrationEvent @event);
        void Receive();
    }
}
