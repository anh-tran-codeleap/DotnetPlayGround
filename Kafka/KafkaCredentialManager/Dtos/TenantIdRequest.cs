namespace KafkaCredentialManager.Dtos;

public class TenantIdRequest
{
    required public string TenantId { get; set; }
}

public class CreateTopicForTenant
{
    required public string TenantId { get; set; }
    required public string TopicName { get; set; }
}