namespace Catalog.API.EventBus
{
    public interface IEventBus
    {
        void Send(IntegrationEvent @event);
        string Receive<T>() where T : IntegrationEvent;
    }
}
