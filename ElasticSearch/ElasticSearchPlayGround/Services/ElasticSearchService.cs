using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Transport.Products.Elasticsearch;
using Microsoft.Extensions.Configuration;

namespace ElasticSearchPlayGround.Services;

public class ElasticSearchService(IConfiguration configuration, ElasticsearchClient elasticsearchClient) : IElasticSearchService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ElasticsearchClient _elasticsearchClient = elasticsearchClient;

    private int MaxBatchSize => _configuration.GetValue("ElasticSearchConfiguration:MaxBatchSize", 1000);
    private int MaxRetries => _configuration.GetValue("ElasticSearchConfiguration:MaxRetries", 5);

    public async Task<bool> PingAsync()
    {
        var response = await _elasticsearchClient.PingAsync();

        return response != null && response.IsSuccess();
    }

    public string GetIndexName<T>(string prefix)
    {
        return $"{prefix}_{typeof(T).Name}".ToLower();
    }

    public async Task CreateIndexAsync(string indexName)
    {
        var response = await _elasticsearchClient.Indices.CreateAsync(
            index: indexName);
    }

    public async Task CreateDocumentAsync<T>(T document, string id, string indexName, string? refresh = null)
    {
        var response = await _elasticsearchClient.IndexAsync(
        document: document,
        index: indexName,
        id: id);

        HandleResponse(response);
    }

    public async Task<ElasticSearchResponseModel<T>> SearchAsync<T>(Action<SearchRequestDescriptor<T>> searchRequest)
    {
        var response = await _elasticsearchClient.SearchAsync(searchRequest);
        HandleResponse(response);

        return new ElasticSearchResponseModel<T>
        {
            Count = response.Total,
            Hits = response.Hits.Select(x => x.Source!).ToList()
        };
    }

    public async Task<long> CountAsync<T>(Action<CountRequestDescriptor<T>> countRequest)
    {
        var response = await _elasticsearchClient.CountAsync(countRequest);
        HandleResponse(response);

        return response.Count;
    }

    public async Task BulkCreateDocumentsAsync<T>(List<T> documents, string indexName)
    {
        // Http request has 100mb size limit so we should split the records to avoid oversize
        foreach (var chunkedDocuments in documents.Chunk(MaxBatchSize))
        {
            var updates = chunkedDocuments.Select(item => new BulkCreateOperation<T>(item, indexName));
            var request = new BulkRequest() { Operations = new BulkOperationsCollection(updates) };
            var response = await _elasticsearchClient.BulkAsync(request);
            HandleResponse(response);
        }
    }

    private static void HandleResponse(ElasticsearchResponse? response)
    {
        if (response == null || !response.IsSuccess() || !response.IsValidResponse)
        {
            throw new InvalidOperationException();
        }
    }
}