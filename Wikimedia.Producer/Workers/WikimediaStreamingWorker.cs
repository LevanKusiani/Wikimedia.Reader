using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wikimedia.Producer.Clients;
using Wikimedia.Producer.Services;

namespace Wikimedia.Producer.Workers
{
    internal class WikimediaStreamingWorker : BackgroundService
    {
        private readonly WikimediaSseClient _wikimediaSseClient;
        private readonly WikimediaProducerService _wikimediaProducerService;
        private readonly ILogger<WikimediaStreamingWorker> _logger;

        public WikimediaStreamingWorker(
            WikimediaSseClient wikimediaSseClient,
            WikimediaProducerService wikimediaProducerService,
            ILogger<WikimediaStreamingWorker> logger)
        {
            _wikimediaSseClient = wikimediaSseClient;
            _wikimediaProducerService = wikimediaProducerService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Wikimedia stream");

            await _wikimediaSseClient.StartAsync(
                async json =>
                {
                    await _wikimediaProducerService.ProduceAsync("wikimedia_recentchange", null!, json);
                },
                stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Wikimedia stream");
            return base.StopAsync(cancellationToken);
        }
    }
}
