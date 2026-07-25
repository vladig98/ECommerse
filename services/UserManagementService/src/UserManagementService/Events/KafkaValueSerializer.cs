using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace UserManagementService.Events
{
    public class KafkaValueSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        }
    }
}
