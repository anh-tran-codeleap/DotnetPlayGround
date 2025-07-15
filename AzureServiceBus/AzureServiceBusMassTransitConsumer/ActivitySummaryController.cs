using Microsoft.AspNetCore.Mvc;

namespace AzureServiceBusMassTransitConsumer;

[ApiController]
[Route("api/[controller]")]
public class ActivitySummaryController : ControllerBase
{
    private readonly ActivitySummaryStore _store;

    public ActivitySummaryController(ActivitySummaryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var summaries = _store.GetAll();
        return Ok(summaries);
    }
}