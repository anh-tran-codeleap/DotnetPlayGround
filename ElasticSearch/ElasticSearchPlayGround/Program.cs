using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ElasticSearchPlayGround.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Service provider
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddElasticsearch(configuration)
            .AddScoped<IElasticSearchService, ElasticSearchService>()
            .BuildServiceProvider();

        var elasticSearchService = serviceProvider.GetRequiredService<IElasticSearchService>();

        // Test connection
        var isConnected = await elasticSearchService.PingAsync();
        Console.WriteLine($"Elasticsearch connection: {(isConnected ? "Success" : "Failed")}");

        if (!isConnected)
        {
            Console.WriteLine("Cannot connect to Elasticsearch. Please check your configuration.");
            return;
        }

        // Generate 25,000 test records to replicate the issue
        Console.WriteLine("Generating 25,000 test records...");
        var testRecords = GenerateTestRecords(25000);

        // Create index
        var indexName = elasticSearchService.GetIndexName<TestDocument>("test");
        Console.WriteLine($"Creating index: {indexName}");
        await elasticSearchService.CreateIndexAsync(indexName);

        // Bulk insert records
        Console.WriteLine("Starting bulk insert...");
        var startTime = DateTime.UtcNow;

        try
        {
            await elasticSearchService.BulkCreateDocumentsAsync(testRecords, indexName);
            var endTime = DateTime.UtcNow;
            Console.WriteLine($"Bulk insert completed in {(endTime - startTime).TotalSeconds:F2} seconds");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during bulk insert: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        // Count total documents
        var totalCount = await elasticSearchService.CountAsync<TestDocument>(c => c.Indices(indexName));
        Console.WriteLine($"Total documents in index: {totalCount}");

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static List<TestDocument> GenerateTestRecords(int count)
    {
        var random = new Random();
        var records = new List<TestDocument>();

        for (int i = 1; i <= count; i++)
        {
            records.Add(new TestDocument
            {
                Id = i,
                Name = $"TestDocument_{i}",
                Description = $"This is a test document with ID {i}. Generated for testing purposes with some additional text to make it more realistic.",
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 365)),
                Price = Math.Round(random.NextDouble() * 1000, 2),
                Category = $"Category_{random.Next(1, 10)}",
                IsActive = random.Next(0, 2) == 1,
                Tags = new List<string> { $"tag_{random.Next(1, 100)}", $"tag_{random.Next(1, 100)}" }
            });
        }

        return records;
    }
}
// Extension method for Elasticsearch configuration
public static class ElasticsearchExtensions
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var host = configuration.GetValue<string>("ElasticSearchConfiguration:Host") ?? "http://localhost:9200";

        var settings = new ElasticsearchClientSettings(new Uri(host))
            .DefaultIndex(configuration.GetValue<string>("ElasticSearchConfiguration:DefaultIndex") ?? "default");

        var client = new ElasticsearchClient(settings);
        services.AddSingleton(client);

        return services;
    }
}
// Test document model
public class TestDocument
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public double Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; } = new();
}