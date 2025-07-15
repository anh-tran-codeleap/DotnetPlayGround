using Mapster;
using MapsterMapper;
namespace MapsterDependencyInjectionTest;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<EndpointRequest, CreateOrderCommand>()
            .Map(dest => dest.Name, src => "mapped Name") // Simulate user/resource context
            .Map(dest => dest.ResourceId, src => MapContext.Current.GetService<RequestContext>().GetResourceId()); // Simulate user/resource context
        config.NewConfig<EndpointRequest, IResourceIdAssociatedRequest>()
            .Map(dest => dest.ResourceId, src => "resource mapping"); // Simulate user/resource context
    }
}