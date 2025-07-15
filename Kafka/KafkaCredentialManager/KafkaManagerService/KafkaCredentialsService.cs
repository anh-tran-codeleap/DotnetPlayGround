using System.Text;
using System.Text.Unicode;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using KafkaCredentialManager.Dtos;

namespace KafkaCredentialManager.KafkaManagerService;

public class KafkaCredentialsService : IMessageBrokerCredentialsService
{
    private readonly IAdminClient _adminClient;

    public KafkaCredentialsService(IConfiguration configuration)
    {
        var bootstrapServers = configuration["KafkaHost"];
        _adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();
    }

    public async Task CreateTopicForTenant(string tenantId, string topicName)
    {
        var createdTopicName = string.Concat(tenantId, topicName);
        // Example: topicName could be "tenant-acme.orders" which matches the "tenant-acme." prefix.
        Console.WriteLine($"\nAttempting to create topic '{createdTopicName}'...");
        var topicSpec = new TopicSpecification { Name = createdTopicName, NumPartitions = 3, ReplicationFactor = 1 };
        await _adminClient.CreateTopicsAsync(new[] { topicSpec });
        Console.WriteLine($"✅ Topic '{createdTopicName}' created successfully within its sandbox.");
    }

    public async Task<TenantCredentials> ProvisionTenantSandboxAsync(string tenantId)
    {
        var username = $"{tenantId}-user";
        var password = GenerateSecurePassword();
        var topicPrefix = $"{tenantId}.";
        var groupPrefix = $"{tenantId}."; // Often the same as the topic prefix for simplicity

        // === STEP 1: Create the User Principal and Credentials ===
        var userUpsert = new UserScramCredentialUpsertion()
        {
            Password = Encoding.UTF8.GetBytes(password),
            User = username,
            ScramCredentialInfo = new ScramCredentialInfo
            {
                Mechanism = ScramMechanism.ScramSha256,
                Iterations = 15000,
            },
        };
        try
        {
            await _adminClient.AlterUserScramCredentialsAsync(new[] { userUpsert }, new AlterUserScramCredentialsOptions() { RequestTimeout = TimeSpan.FromSeconds(30) });
            Console.WriteLine("All AlterUserScramCredentials operations completed successfully");
        }
        catch (AlterUserScramCredentialsException e)
        {
            Console.WriteLine($"An error occurred altering user SCRAM credentials" +
                               " for some users:");
            foreach (var result in e.Results)
            {
                Console.WriteLine($"  User: {result.User}");
                Console.WriteLine($"    Error: {result.Error}");
            }
        }
        catch (KafkaException e)
        {
            Console.WriteLine($"An error occurred altering user SCRAM credentials: {e}");
            Environment.ExitCode = 1;
        }
        // === STEP 2: Create the Prefix-Based ACLs (The Sandbox) ===
        var prefixAcls = new List<AclBinding>
        {
            // Rule 1: Allow producing to any topic starting with the prefix
            new AclBinding
            {
                Pattern = new ResourcePattern { Type = ResourceType.Topic, Name = topicPrefix, ResourcePatternType = ResourcePatternType.Prefixed },
                Entry = new AccessControlEntry { Principal = $"User:{username}", Host = "*", Operation = AclOperation.Write, PermissionType = AclPermissionType.Allow }
            },
            // Rule 2: Allow consuming from any topic starting with the prefix
            new AclBinding
            {
                Pattern = new ResourcePattern { Type = ResourceType.Topic, Name = topicPrefix, ResourcePatternType = ResourcePatternType.Prefixed },
                Entry = new AccessControlEntry { Principal = $"User:{username}", Host = "*", Operation = AclOperation.Read, PermissionType = AclPermissionType.Allow }
            },
            // Rule 3: Allow consumer group management for any group starting with the prefix
            new AclBinding
            {
                Pattern = new ResourcePattern { Type = ResourceType.Group, Name = groupPrefix, ResourcePatternType = ResourcePatternType.Prefixed },
                Entry = new AccessControlEntry { Principal = $"User:{username}", Host = "*", Operation = AclOperation.Read, PermissionType = AclPermissionType.Allow }
            }
        };

        await _adminClient.CreateAclsAsync(prefixAcls);
        Console.WriteLine($"✅ ACL Sandbox created for prefix '{topicPrefix}'.");

        // === STEP 4: Return the details to the tenant ===
        return new TenantCredentials
        {
            TenantId = tenantId,
            Username = username,
            Password = password,
            TopicPrefix = topicPrefix,
            GroupPrefix = groupPrefix,
            ClientConfig = $"sasl.username={username},sasl.password={password}" // Simplified example
        };
    }
    private static string GenerateSecurePassword(int length = 24) => Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(length));

}