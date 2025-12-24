using Microsoft.Extensions.Logging;

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

        public async Task StartAsync(Func<WikiRecentChange, Task> onMessage, CancellationToken cancellationToken)
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
                    var wikiRecentChange = System.Text.Json.JsonSerializer.Deserialize<WikiRecentChange>(rawData);

                    if (wikiRecentChange == null)
                        _logger.LogWarning("Failed to deserialize. Raw data: {RawData}", rawData);
                    
                    await onMessage(wikiRecentChange!);
                }
            }
        }
    }
}
