using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Wikimedia.Common.Kafka;
using Wikimedia.Consumer.Options;
using Wikimedia.Consumer.Services;
using Wikimedia.Consumer.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<OpenSearchOptions>(
            builder.Configuration.GetSection("OpenSearch"));

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
