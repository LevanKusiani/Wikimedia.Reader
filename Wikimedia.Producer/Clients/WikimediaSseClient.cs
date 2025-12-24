using Microsoft.Extensions.Logging;
using Wikimedia.Common.Extensions;

namespace Wikimedia.Producer.Clients
{
    internal class WikimediaSseClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<WikimediaSseClient> _logger;

        public WikimediaSseClient(HttpClient client, ILogger<WikimediaSseClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task StartAsync(Func<WikiRecentChange, Task> onMessage, Func<string, Exception, Task>? onError, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "v2/stream/recentchange");

            request.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

            using var response = await _client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);

                if (line == null)
                    break;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("data: "))
                {
                    var rawData = line.Substring("data: ".Length);

                    try
                    {
                        var wikiRecentChange = WikiRecentChangeExtensions.Deserialize(rawData);

                        await onMessage(wikiRecentChange!);
                    }
                    catch (InvalidOperationException e)
                    {
                        _logger.LogWarning("Failed to deserialize Wikimedia event: {Message}", e.Message);

                        if (onError != null)
                        {
                            await onError(rawData, e);
                        }
                    }
                }
            }
        }
    }
}
