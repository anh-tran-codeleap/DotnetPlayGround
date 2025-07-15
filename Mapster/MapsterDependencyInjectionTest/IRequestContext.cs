namespace MapsterDependencyInjectionTest;

public interface IRequestContext
{
    string GetResourceId();
}

public class RequestContext : IRequestContext
{
    public string GetResourceId() => "resource-hello-id"; // Simulate user/resource context
}