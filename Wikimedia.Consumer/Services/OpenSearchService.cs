using OpenSearch.Client;
using OpenSearch.Net;

namespace Wikimedia.Consumer.Services
{
    internal class OpenSearchService
    {
        private readonly IOpenSearchClient _client;
        private readonly ILogger<OpenSearchService> _logger;

        public OpenSearchService(IOpenSearchClient client, ILogger<OpenSearchService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task IndexDocumentAsync(string indexName, string documentJson, CancellationToken cancellationToken)
        {
            CheckIndex(indexName);

            var response = await _client.LowLevel.IndexAsync<StringResponse>(
                "wikimedia",
                documentJson,
                null,
                cancellationToken
            );

            if (!response.Success)
                _logger.LogError("Failed to index document: {Error}", response.Body);

            _logger.LogInformation("Document indexed successfully in {IndexName}", indexName);
        }

        private async void CheckIndex(string indexName)
        {
            var existsResponse = await _client.Indices.ExistsAsync(indexName);
            if (!existsResponse.Exists)
            {
                var createIndexResponse = await _client.Indices.CreateAsync(indexName);
                if (!createIndexResponse.IsValid)
                {
                    _logger.LogError("Failed to create index {IndexName}: {Error}", indexName, createIndexResponse.ServerError);
                }
                else
                {
                    _logger.LogInformation("Index {IndexName} created successfully", indexName);
                }
            }
        }
    }
}
