using KafkaCredentialManager.Dtos;

namespace KafkaCredentialManager.KafkaManagerService;

public interface IMessageBrokerCredentialsService
{
    Task<TenantCredentials> ProvisionTenantSandboxAsync(string tenantId);
    Task CreateTopicForTenant(string tenantId, string topicName);
}