namespace UserManagementService.Events.Interfaces
{
    public interface IKafkaEventProducer<TKey, TValue>
    {
        void SendEvent(string topic, TKey key, TValue value);
        Task SendEventAsync(string topic, TKey key, TValue value, CancellationToken cancellationToken);
    }
}
