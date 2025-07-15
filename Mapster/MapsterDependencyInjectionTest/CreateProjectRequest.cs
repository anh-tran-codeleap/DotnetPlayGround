namespace MapsterDependencyInjectionTest;

public class CreateOrderCommand : IResourceIdAssociatedRequest
{
    public string Name { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
}

public class EndpointRequest
{
    public string Name { get; set; } = string.Empty;
}