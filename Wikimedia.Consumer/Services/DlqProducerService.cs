using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Wikimedia.Common.Kafka;

namespace Wikimedia.Consumer.Services
{
    internal class DlqProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly KafkaOptions _options;
        private readonly ILogger<DlqProducerService> _logger;

        public DlqProducerService(
            IProducer<string, string> producer,
            IOptions<KafkaOptions> kafkaOptions,
            ILogger<DlqProducerService> logger)
        {
            _producer = producer;
            _options = kafkaOptions.Value;
            _logger = logger;
        }

        public async Task ProduceAsync(string key, string value)
        {
            var message = new Message<string, string>
            {
                Key = key,
                Value = value
            };

            _logger.LogInformation("Producing message to DLQ");
            await _producer.ProduceAsync(_options.DLQ.Topic, message);
        }
    }
}
