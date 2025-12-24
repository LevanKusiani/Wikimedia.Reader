using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wikimedia.Producer.Clients;
using Wikimedia.Producer.Services;
using Wikimedia.Common.Kafka;

namespace Wikimedia.Producer.Workers
{
    internal class WikimediaStreamingWorker : BackgroundService
    {
        // ToDo: Move JsonSerializerOptions to a common location
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly WikimediaSseClient _wikimediaSseClient;
        private readonly WikimediaProducerService _wikimediaProducerService;
        private readonly KafkaOptions _kafkaOptions;
        private readonly ILogger<WikimediaStreamingWorker> _logger;

        public WikimediaStreamingWorker(
            WikimediaSseClient wikimediaSseClient,
            WikimediaProducerService wikimediaProducerService,
            IOptions<KafkaOptions> kafkaOptions,
            ILogger<WikimediaStreamingWorker> logger)
        {
            _wikimediaSseClient = wikimediaSseClient;
            _wikimediaProducerService = wikimediaProducerService;
            _kafkaOptions = kafkaOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Wikimedia stream");

            await _wikimediaSseClient.StartAsync(
                // onMessage
                async ev =>
                {
                    var json = JsonSerializer.Serialize(ev, _jsonSerializerOptions);

                    if (ev is not null)
                    {
                        await _wikimediaProducerService.ProduceAsync("wikimedia_recentchange", $"{ev.ServerName}:{ev.Title}", json);
                    }
                    else
                    {
                        _logger.LogWarning("Received null WikiRecentChange from JSON: {Json}", json);
                    }
                },
                // onError
                ProduceToDqlAsync,
                stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Wikimedia stream");
            return base.StopAsync(cancellationToken);
        }

        private async Task ProduceToDqlAsync(string rawJson, Exception ex)
        {
            try
            {
                var dlqTopic = _kafkaOptions?.DLQ?.Topic;

                if (string.IsNullOrWhiteSpace(dlqTopic))
                {
                    _logger.LogWarning(ex, "Deserialization failed but no DLQ is configured. Raw event: {Raw}", rawJson);
                    return;
                }

                _logger.LogInformation("Sending failed event to DLQ topic '{Topic}'", dlqTopic);

                var dlqMessage = new DlqMessage
                {
                    Source = "Wikimedia.Producer",
                    Reason = "Deserialization / Validation failure",
                    Error = ex.Message,
                    OriginalPayload = rawJson,
                    FailedAtUtc = DateTime.UtcNow,
                    Topic = "wikimedia_recentchange"
                };

                var dlqJson = JsonSerializer.Serialize(dlqMessage, _jsonSerializerOptions);

                await _wikimediaProducerService.ProduceAsync(dlqTopic, "deserialize_error", dlqJson);
            }
            catch (Exception produceEx)
            {
                _logger.LogError(produceEx, "Failed to produce message to DLQ.");
            }
        }
    }
}
