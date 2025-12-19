using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Wikimedia.Common.Kafka;
using Wikimedia.Consumer.Services;

namespace Wikimedia.Consumer.Workers
{
    internal class WikimediaStreamingConsumerWorker : BackgroundService
    {
        private readonly OpenSearchService _openSearchService;
        private readonly KafkaOptions _options;
        private readonly ILogger<WikimediaStreamingConsumerWorker> _logger;
        private readonly IConsumer<string, string> _consumer;

        public WikimediaStreamingConsumerWorker(
            OpenSearchService openSearchService,
            IOptions<KafkaOptions> kafkaOptions,
            ILogger<WikimediaStreamingConsumerWorker> logger)
        {
            _openSearchService = openSearchService;
            _options = kafkaOptions.Value;
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.Consumer.GroupId,
                AutoOffsetReset = _options.Consumer.AutoOffsetReset switch
                {
                    "earliest" => AutoOffsetReset.Earliest,
                    "latest" => AutoOffsetReset.Latest,
                    _ => AutoOffsetReset.Latest
                },
                EnableAutoCommit = _options.Consumer.EnableAutoCommit,
                PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky
            };

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, err) =>
                {
                    if (_logger.IsEnabled(logLevel: LogLevel.Error))
                    {
                        _logger.LogError("Kafka Consumer error: {Reason}", err.Reason);
                    }
                })
                .SetLogHandler((_, log) =>
                {
                    if (_logger.IsEnabled(logLevel: LogLevel.Information))
                    {
                        _logger.LogInformation("Kafka Consumer log: {Message}", log.Message);
                    }
                })
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    logger.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}]");
                })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    logger.LogWarning($"Revoked partitions: [{string.Join(", ", partitions)}]");
                })
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Kafka consumer: {_consumer.Name} started.");
            _consumer.Subscribe(_options.Consumer.ConsumerTopic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? result = null;

                    try
                    {
                        result = _consumer.Consume(stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Graceful shutdown in finally code block.
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while consuming Kafka message.");
                    }

                    if (result == null || result.IsPartitionEOF)
                        continue;

                    _logger.LogInformation($@"Message received for {_consumer.Name}:
                            Topic: {result.Topic}
                            Key: {result.Message.Key}
                            Value: {result.Message.Value}
                            Partition: {result.Partition.Value}
                            Offset: {result.Offset}
                            Timestamp: {result.Message.Timestamp.UtcDateTime}
                            ");

                    // Send data to opensearch
                    await _openSearchService.IndexDocumentAsync(
                        "wikimedia",
                        result.Message.Value,
                        stoppingToken);

                    if (!_options.Consumer.EnableAutoCommit)
                    {
                        try
                        {
                            _consumer.Commit(result);
                        }
                        catch (KafkaException ex)
                        {
                            _logger.LogError(ex, "Commit failed.");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer closing...");
            }
            finally
            {
                try
                {
                    _logger.LogInformation("Unsubscribing consumer...");
                    _consumer.Unsubscribe();

                    if (!_options.Consumer.EnableAutoCommit)
                    {
                        _logger.LogInformation("Committing offsets on shutdown...");
                        _consumer.Commit();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error during consumer shutdown operations.");
                }

                _logger.LogInformation("Closing consumer...");
                _consumer.Close(); // Leaves group gracefully
            }


            _logger.LogInformation("Kafka consumer stopped.");
        }
    }
}
