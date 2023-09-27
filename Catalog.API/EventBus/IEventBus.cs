namespace Catalog.API.EventBus
{
    public interface IEventBus
    {
        void Send(IntegrationEvent @event);
        Task<T> Receive<T>() where T : IntegrationEvent;
    }
}
