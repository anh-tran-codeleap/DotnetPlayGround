using Elastic.Clients.Elasticsearch;

namespace ElasticSearchPlayGround.Services;

public interface IElasticSearchService
{
    Task<bool> PingAsync();

    string GetIndexName<T>(string prefix);

    Task CreateIndexAsync(string indexName);

    Task CreateDocumentAsync<T>(T document, string id, string indexName, string? refresh = null);

    Task<ElasticSearchResponseModel<T>> SearchAsync<T>(Action<SearchRequestDescriptor<T>> searchRequest);

    Task<long> CountAsync<T>(Action<CountRequestDescriptor<T>> countRequest);

    Task BulkCreateDocumentsAsync<T>(List<T> documents, string indexName);
}