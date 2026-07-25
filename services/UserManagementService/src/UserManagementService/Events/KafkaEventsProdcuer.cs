using Confluent.Kafka;

namespace UserManagementService.Events
{
    public class KafkaEventsProdcuer<TKey, TValue> : IKafkaEventProducer<TKey, TValue>
    {
        private readonly IProducer<TKey, TValue> _producer;
        private readonly ILogger<KafkaEventsProdcuer<TKey, TValue>> _logger;

        public KafkaEventsProdcuer(IProducer<TKey, TValue> producer, ILogger<Events.KafkaEventsProdcuer<TKey, TValue>> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        public void SendEvent(string topic, TKey key, TValue value)
        {
            _producer.Produce(topic, new Message<TKey, TValue>() { Key = key, Value = value }, LogEventOutcome);
        }

        public async Task SendEventAsync(string topic, TKey key, TValue value, CancellationToken cancellationToken)
        {
            DeliveryResult<TKey, TValue> result = await _producer.ProduceAsync(topic, new Message<TKey, TValue>() { Key = key, Value = value }, cancellationToken);

            LogEventOutcome(result);
        }

        private void LogEventOutcome(DeliveryResult<TKey, TValue> result)
        {
            switch (result.Status)
            {
                case PersistenceStatus.NotPersisted:
                    _logger.LogError(GlobalConstants.LogError(GlobalConstants.KafkaHeader, string.Format(GlobalConstants.KafkaEventFailure, result.Topic, result.Key, result.Value, string.Empty)));
                    break;
                case PersistenceStatus.PossiblyPersisted:
                    _logger.LogWarning(GlobalConstants.LogWarning(GlobalConstants.KafkaHeader, string.Format(GlobalConstants.KafkaEventDeliveredButNotAcknowledged, result.Topic, result.Key, result.Value)));
                    break;
                case PersistenceStatus.Persisted:
                    _logger.LogInformation(GlobalConstants.LogInfo(GlobalConstants.KafkaHeader, string.Format(GlobalConstants.KafkaEventDelivered, result.Topic, result.Key, result.Value)));
                    break;
                default:
                    break;
            }

            if (result is not DeliveryReport<TKey, TValue>)
            {
                return;
            }

            DeliveryReport<TKey, TValue> report = (DeliveryReport<TKey, TValue>)result;

            if (!report.Error.IsError)
            {
                return;
            }

            _logger.LogError(GlobalConstants.LogError(GlobalConstants.KafkaHeader, string.Format(GlobalConstants.KafkaEventFailure, report.Topic, report.Key, report.Value, report.Error.Reason)));
        }
    }
}
