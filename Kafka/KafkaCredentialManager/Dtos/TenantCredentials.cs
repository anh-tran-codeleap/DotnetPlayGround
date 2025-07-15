// The information you will return to the client
namespace KafkaCredentialManager.Dtos;

public class TenantCredentials
{
    required public string TenantId { get; set; }
    required public string Username { get; set; }
    required public string Password { get; set; }
    required public string TopicPrefix { get; set; }
    required public string GroupPrefix { get; set; }
    required public string ClientConfig { get; set; }
}