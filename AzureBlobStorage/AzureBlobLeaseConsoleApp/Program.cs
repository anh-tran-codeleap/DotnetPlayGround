using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("AzureStorage");
        var containerName = configuration["BlobStorage:ContainerName"];
        var blobName = configuration["BlobStorage:BlobName"];

        // Create blob service client
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        BlobLeaseClient blobLeaseClient = blobClient.GetBlobLeaseClient();

        BlobLease blobLease = await blobLeaseClient.AcquireAsync(TimeSpan.FromSeconds(60));
        var uploadOptions = new BlobUploadOptions
        {
            Conditions = new BlobRequestConditions { LeaseId = blobLease.LeaseId }
        };

        
        await blobLeaseClient.ReleaseAsync();
    }
    private static async Task Main1(string[] args)
    {
        try
        {
            // Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("AzureStorage");
            var containerName = configuration["BlobStorage:ContainerName"];
            var blobName = configuration["BlobStorage:BlobName"];

            // Create blob service client
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Ensure container and blob exist
            await containerClient.CreateIfNotExistsAsync();

            // Create a sample blob if it doesn't exist
            if (!await blobClient.ExistsAsync())
            {
                string blobContents = "Sample content for lease testing";
                byte[] byteArray = Encoding.UTF8.GetBytes(blobContents);
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
                Console.WriteLine($"Created blob: {blobName}");
            }

            // Get blob lease client
            var leaseClient = blobClient.GetBlobLeaseClient();

            // Acquire lease
            Console.WriteLine("Acquiring blob lease...");
            var leaseResponse = await leaseClient.AcquireAsync(TimeSpan.FromMinutes(1));

            Console.WriteLine($"Lease acquired successfully!");
            Console.WriteLine($"Lease ID: {leaseResponse.Value.LeaseId}");
            Console.WriteLine($"ETag: {leaseResponse.Value.ETag}");

            // Demonstrate lease operations
            await DemonstrateLeasedBlobOperations(blobClient, leaseResponse.Value.LeaseId);

            // Renew lease
            Console.WriteLine("\nRenewing lease...");
            var renewResponse = await leaseClient.RenewAsync();
            Console.WriteLine($"Lease renewed. New ETag: {renewResponse.Value.ETag}");

            // Release lease
            Console.WriteLine("\nReleasing lease...");
            await leaseClient.ReleaseAsync();
            Console.WriteLine("Lease released successfully!");

            // Demonstrate different lease scenarios
            // await DemonstrateLeaseScenarios(leaseClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static async Task DemonstrateLeasedBlobOperations(BlobClient blobClient, string leaseId)
    {
        try
        {
            Console.WriteLine("\n--- Demonstrating operations on leased blob ---");

            // Try to modify blob without lease (should fail)
            Console.WriteLine("Attempting to modify blob without lease ID...");
            try
            {
                string blobContents = "Modified content";
                byte[] byteArray = Encoding.UTF8.GetBytes(blobContents);
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
                Console.WriteLine("❌ This should have failed!");
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 412)
            {
                Console.WriteLine("✅ Expected failure - blob is leased");
            }

            // Modify blob with lease (should succeed)
            Console.WriteLine("Modifying blob with lease ID...");
            var conditions = new BlobRequestConditions { LeaseId = leaseId };
            string blobContentsWithLease = "Modified content with lease";
            byte[] byteArrayWithLease = Encoding.UTF8.GetBytes(blobContentsWithLease);
            using (MemoryStream stream = new MemoryStream(byteArrayWithLease))
            {
                await blobClient.UploadAsync(stream, conditions: conditions);
            }
            Console.WriteLine("✅ Successfully modified leased blob");

            // Set metadata with lease
            Console.WriteLine("Setting metadata on leased blob...");
            var metadata = new Dictionary<string, string>
            {
                ["LastModified"] = DateTime.UtcNow.ToString(),
                ["ModifiedBy"] = "LeaseDemo"
            };
            await blobClient.SetMetadataAsync(metadata, conditions);
            Console.WriteLine("✅ Successfully set metadata on leased blob");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in leased blob operations: {ex.Message}");
        }
    }

    // private static async Task DemonstrateLeaseScenarios(BlobLeaseClient leaseClient)
    // {
    //     try
    //     {
    //         Console.WriteLine("\n--- Demonstrating different lease scenarios ---");

    //         // 1. Acquire with specific lease ID
    //         Console.WriteLine("1. Acquiring lease with specific ID...");
    //         var customLeaseId = Guid.NewGuid().ToString();
    //         var customLeaseClient = leaseClient.WithLeaseId(customLeaseId);
    //         var customLeaseResponse = await customLeaseClient.AcquireAsync(TimeSpan.FromSeconds(30));
    //         Console.WriteLine($"✅ Acquired lease with custom ID: {customLeaseId}");

    //         // 2. Try to acquire again (should fail)
    //         Console.WriteLine("2. Attempting to acquire lease again...");
    //         try
    //         {
    //             await leaseClient.AcquireAsync(TimeSpan.FromSeconds(30));
    //             Console.WriteLine("❌ This should have failed!");
    //         }
    //         catch (Azure.RequestFailedException ex) when (ex.Status == 409)
    //         {
    //             Console.WriteLine("✅ Expected failure - blob already leased");
    //         }

    //         // 3. Break the lease
    //         Console.WriteLine("3. Breaking the lease...");
    //         await leaseClient.BreakAsync();
    //         Console.WriteLine("✅ Lease broken successfully");

    //         // 4. Acquire infinite lease
    //         Console.WriteLine("4. Acquiring infinite lease...");
    //         var infiniteLeaseResponse = await leaseClient.AcquireAsync(TimeSpan.FromSeconds(-1));
    //         Console.WriteLine($"✅ Infinite lease acquired: {infiniteLeaseResponse.Value.LeaseId}");

    //         // 5. Release infinite lease
    //         Console.WriteLine("5. Releasing infinite lease...");
    //         await leaseClient.ReleaseAsync();
    //         Console.WriteLine("✅ Infinite lease released");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error in lease scenarios: {ex.Message}");
    //     }
    // }
}