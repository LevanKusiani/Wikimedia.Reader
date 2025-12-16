using Wikimedia.Consumer.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<WikimediaStreamingConsumerWorker>();

var host = builder.Build();
host.Run();
