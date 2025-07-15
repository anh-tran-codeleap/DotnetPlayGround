namespace MapsterDependencyInjectionUnitTest;

using System;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class InjectionTest
{
    [Fact]
    public void Injection()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<Poco, Dto>()
            .Map(dto => dto.Name, _ => MapContext.Current.GetService<IMockService>().GetName());
        config.NewConfig<Poco, IDtoWithTenantId>()
            .Map(dto => dto.TenantId, _ => MapContext.Current.GetService<IRequestContext>().GetTenantId());

        IServiceCollection sc = new ServiceCollection();
        sc.AddScoped<IMockService, MockService>();
        sc.AddScoped<IRequestContext, RequestContext>();
        sc.AddSingleton(config);
        sc.AddScoped<IMapper, ServiceMapper>();

        var sp = sc.BuildServiceProvider();
        using (var scope = sp.CreateScope())
        {
            var mapper = scope.ServiceProvider.GetService<IMapper>();
            var poco = new Poco { Id = "bar" };
            var dto = mapper.Map<Poco, Dto>(poco);
            Assert.Equal("foo", dto.Name);
            Assert.Null(dto.TenantId); // TenantId is not set in Dto, but can be set in IDtoWithTenantId

            var pocoWithTenant = new Poco { Id = "baz" };
            var dtoWithTenant = mapper.Map<Poco, IDtoWithTenantId>(pocoWithTenant);
            Assert.Equal("tenant-hello-id", dtoWithTenant.TenantId);
            // Assert.Equal("baz", dtoWithTenant.Id); // Can't map Id directly, but can map TenantId because of the interface
        }
    }

    [Fact]
    public void NoServiceAdapter_InjectionError()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<Poco, Dto>()
            .Map(dto => dto.Name, _ => MapContext.Current.GetService<IMockService>().GetName());

        IServiceCollection sc = new ServiceCollection();
        sc.AddScoped<IMockService, MockService>();
        sc.AddSingleton(config);
        sc.AddScoped<IMapper, Mapper>();

        var sp = sc.BuildServiceProvider();
        using (var scope = sp.CreateScope())
        {
            var mapper = scope.ServiceProvider.GetService<IMapper>();
            var poco = new Poco { Id = "bar" };
            Assert.Throws<InvalidOperationException>(() =>
            {
                var dto = mapper.Map<Poco, Dto>(poco);
                // Should throw before this line
            });
        }
    }
}

public interface IMockService
{
    string GetName();
}

public interface IRequestContext
{
    string GetTenantId();
}

public class MockService : IMockService
{
    public string GetName()
    {
        return "foo";
    }
}

public class RequestContext : IRequestContext
{
    public string GetTenantId()
    {
        return "tenant-hello-id"; // Simulate user/resource context
    }
}

public class Poco
{
    public string Id { get; set; }
}
public class Dto : IDtoWithTenantId
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string TenantId { get; set; }
}

public interface IDtoWithTenantId
{
    string TenantId { get; set; }
}