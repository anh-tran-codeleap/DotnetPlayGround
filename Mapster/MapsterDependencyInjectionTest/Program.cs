using Microsoft.Extensions.DependencyInjection;
using Mapster;
using MapsterMapper;
using MapsterDependencyInjectionTest;
// Setup DI
var services = new ServiceCollection();

services.AddSingleton<IRequestContext, RequestContext>();

// Mapster config
var config = new TypeAdapterConfig();
config.Scan(typeof(MappingConfig).Assembly);

services.AddSingleton(config);
services.AddScoped<IMapper, ServiceMapper>();

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
var x = scope.ServiceProvider.GetRequiredService<IRequestContext>().GetResourceId();

// Create anonymous source
EndpointRequest source = new()
{
    Name = "Test Project"
};
