using AzureServiceBusMassTransitPublisher;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Read Service Bus connection string from config
var serviceBusConnection = builder.Configuration.GetSection("AzureServiceBus")["ConnectionString"];

// Configure MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(serviceBusConnection);

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapControllers();

app.Run();