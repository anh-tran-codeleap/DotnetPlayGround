using MassTransit;
using System.Threading.Tasks;

namespace AzureServiceBusMassTransitConsumer;

public class ActivitySummaryConsumer(ActivitySummaryStore store, ILogger<ActivitySummaryConsumer> logger) : IConsumer<IActivity>
{
    private readonly ActivitySummaryStore _store = store;
    private readonly ILogger<ActivitySummaryConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<IActivity> context)
    {
        _logger.LogInformation("Received activity: {Title}", context.Message.Title);
        var activity = context.Message;
        var summary = $"Activity: {activity.Title} - {activity.Description} (Follow up: {activity.FollowUpDate:yyyy-MM-dd})";
        var response = new ActivitySummaryResponse
        {
            Summary = summary
        };
        _store.Add(response);
        await context.RespondAsync(response);
    }
}