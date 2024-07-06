namespace UserManagementService.Events.Contracts
{
    public interface IEventBus
    {
        void Publish<T>(T @event) where T : class;
    }
}
