using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Wikimedia.Producer.Services
{
    internal class WikimediaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<WikimediaProducerService> _logger;

        public WikimediaProducerService(IProducer<string, string> producer, ILogger<WikimediaProducerService> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        public async Task ProduceAsync(string topic, string key, string value)
        {
            var message = new Message<string, string>
            {
                Key = key,
                Value = value
            };

            _logger.LogInformation("Producing message");
            await _producer.ProduceAsync(topic, message);
        }
    }
}
