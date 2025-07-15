using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AzureServiceBusMassTransitConsumer;

public class ActivitySummaryStore
{
    private readonly ConcurrentBag<ActivitySummaryResponse> _summaries = new();

    public void Add(ActivitySummaryResponse summary) => _summaries.Add(summary);

    public IEnumerable<ActivitySummaryResponse> GetAll() => _summaries.ToArray();
}