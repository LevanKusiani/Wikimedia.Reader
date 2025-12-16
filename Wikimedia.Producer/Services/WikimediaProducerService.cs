using Confluent.Kafka;

namespace Wikimedia.Producer.Services
{
    internal class WikimediaProducerService
    {
        private readonly IProducer<string, string> _producer;

        public WikimediaProducerService(IProducer<string, string> producer)
        {
            _producer = producer;
        }

        public async Task ProduceAsync(string topic, string key, string value)
        {
            var message = new Message<string, string>
            {
                Key = key,
                Value = value
            };

            await _producer.ProduceAsync(topic, message);
        }
    }
}
