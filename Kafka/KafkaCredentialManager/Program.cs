using KafkaCredentialManager.Dtos;
using KafkaCredentialManager.KafkaManagerService;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMessageBrokerCredentialsService, KafkaCredentialsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/api/v1/kafka/create-credentials", async (
    IMessageBrokerCredentialsService credentialsManager,
    [FromBody] TenantIdRequest request) =>
{
    var credentials = await credentialsManager.ProvisionTenantSandboxAsync(request.TenantId);
    return Results.Ok(credentials);
})
.WithName("CreateKafkaCredentials")
.WithOpenApi();

app.MapPost("/api/v1/kafka/create-topic", async (
    IMessageBrokerCredentialsService credentialsManager,
    [FromBody] CreateTopicForTenant request) =>
{
    await credentialsManager.CreateTopicForTenant(request.TenantId, request.TopicName);
    return Results.Ok();
})
.WithName("CreateTopic")
.WithOpenApi();
app.Run();