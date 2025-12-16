// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Wikimedia.Common.Kafka;
using Wikimedia.Producer.Clients;
using Wikimedia.Producer.Services;
using Wikimedia.Producer.Workers;

Console.WriteLine("Hello, World!");

await Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.Configure<KafkaOptions>(
            context.Configuration.GetSection("Kafka"));

        services.AddHttpClient<WikimediaSseClient>(client =>
        {
            client.BaseAddress = new Uri("https://stream.wikimedia.org");
            client.Timeout = TimeSpan.FromMinutes(5);

            client.DefaultRequestHeaders.UserAgent.ParseAdd("wikimedia-kafka-producer/1.0");
        });

        services.AddSingleton<IProducer<string, string>>(sp =>
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

        services.AddSingleton<WikimediaProducerService>();

        services.AddHostedService<WikimediaStreamingWorker>();
    })
    .RunConsoleAsync();