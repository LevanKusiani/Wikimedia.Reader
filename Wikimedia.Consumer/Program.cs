using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Wikimedia.Common.Kafka;
using Wikimedia.Consumer.Options;
using Wikimedia.Consumer.Services;
using Wikimedia.Consumer.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<OpenSearchOptions>(
            builder.Configuration.GetSection("OpenSearch"));

builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;

    var config = new ProducerConfig
    {
        BootstrapServers = opts.BootstrapServers,
        ClientId = opts.ClientId,
        Acks = opts.Producer.Acks switch
        {
            "-1" => Acks.All,
            "0" => Acks.None,
            "1" => Acks.Leader,
            _ => Acks.All
        },
        EnableIdempotence = opts.Producer.EnableIdempotence,
        CompressionType = opts.Producer.CompressionType switch
        {
            "gzip" => CompressionType.Gzip,
            "snappy" => CompressionType.Snappy,
            "lz4" => CompressionType.Lz4,
            "zstd" => CompressionType.Zstd,
            _ => CompressionType.None
        },
        LingerMs = opts.Producer.LingerMs,
        BatchSize = opts.Producer.BatchSize,
        MessageSendMaxRetries = opts.Producer.Retries
    };

    return new ProducerBuilder<string, string>(config).Build();
});

builder.Services.AddSingleton<DlqProducerService>();

builder.Services.Configure<KafkaOptions>(
            builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IOpenSearchClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<OpenSearchOptions>>().Value;

    var settings = new ConnectionSettings(new Uri(options.Uri))
        .DefaultIndex(options.DefaultIndexName);

    return new OpenSearchClient(settings);
});

builder.Services.AddHostedService<WikimediaStreamingConsumerWorker>();
builder.Services.AddSingleton<OpenSearchService>();

var host = builder.Build();
host.Run();
