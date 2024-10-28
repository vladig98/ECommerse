using Confluent.Kafka;

namespace UserManagementService.Events
{
    public class KafkaEventsProdcuer<TKey, TValue>(IProducer<TKey, TValue> producer, LoggingFactory<KafkaEventsProdcuer<TKey, TValue>> logger) : IKafkaEventProducer<TKey, TValue>
    {
        private readonly IProducer<TKey, TValue> _producer = producer;
        private readonly LoggingFactory<KafkaEventsProdcuer<TKey, TValue>> _logger = logger;

        private const string KafkaHeader = "Kafka";
        private const string KafkaEventFailure = "Event was not delivered! Topic: {0}, Key: {1}, Value: {2}, Reason: '{3}'";
        private const string KafkaEventDelivered = "Event was successfully delivered! Topic: {0}, Key: {1}, Value: {2}";
        private const string KafkaEventDeliveredButNotAcknowledged = "Event was delivered but not acknowledged! Topic: {0}, Key: {1}, Value: {2}";

        public void SendEvent(string topic, TKey key, TValue value)
        {
            Message<TKey, TValue> message = GenerateMessage(key, value);

            _producer.Produce(topic, message, LogEventOutcome);
        }

        public async Task SendEventAsync(string topic, TKey key, TValue value, CancellationToken cancellationToken)
        {
            Message<TKey, TValue> message = GenerateMessage(key, value);
            DeliveryResult<TKey, TValue> result = await _producer.ProduceAsync(topic, message, cancellationToken);

            LogEventOutcome(result);
        }

        private Message<TKey, TValue> GenerateMessage(TKey key, TValue value)
        {
            return new Message<TKey, TValue>
            {
                Key = key,
                Value = value,
                Timestamp = new Timestamp(DateTime.UtcNow)
            };
        }

        private void LogEventOutcome(DeliveryResult<TKey, TValue> result)
        {
            string key = result.Key?.ToString() ?? string.Empty;
            string value = result.Value?.ToString() ?? string.Empty;

            switch (result.Status)
            {
                case PersistenceStatus.NotPersisted:
                    _logger.LogError(KafkaHeader, KafkaEventFailure, result.Topic, key, value, string.Empty);
                    break;
                case PersistenceStatus.PossiblyPersisted:
                    _logger.LogWarning(KafkaHeader, KafkaEventDeliveredButNotAcknowledged, result.Topic, key, value);
                    break;
                case PersistenceStatus.Persisted:
                    _logger.LogInfo(KafkaHeader, KafkaEventDelivered, result.Topic, key, value);
                    break;
            }

            LogErrorsIfAny(result, key, value);
        }

        private void LogErrorsIfAny(DeliveryResult<TKey, TValue> result, string key, string value)
        {
            if (result is not DeliveryReport<TKey, TValue> report)
            {
                return;
            }

            if (!report.Error.IsError)
            {
                return;
            }

            _logger.LogError(KafkaHeader, KafkaEventFailure, report.Topic, key, value, report.Error.Reason);
        }
    }
}
