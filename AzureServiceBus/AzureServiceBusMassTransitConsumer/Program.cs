using AzureServiceBusMassTransitConsumer;
using MassTransit;
using MassTransit.AzureServiceBusTransport.Topology;

var builder = WebApplication.CreateBuilder(args);

// Read Service Bus connection string from configuration
var serviceBusConnection = builder.Configuration.GetSection("AzureServiceBus")["ConnectionString"];

// Register MassTransit 
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ActivitySummaryConsumer>();

    // x.AddRequestClient<Activity>();
    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(serviceBusConnection);
        // cfg.ConfigureEndpoints(context);

        cfg.SubscriptionEndpoint(
            "ActivityConsumer",
            "azureservicebusmasstransitpublisher/activity",
            e =>
            {
                e.ConfigureConsumer<ActivitySummaryConsumer>(context);
            }
        );

    });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<ActivitySummaryStore>();

var app = builder.Build();

app.MapControllers();

app.Run();