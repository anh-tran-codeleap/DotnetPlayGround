using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace AzureServiceBusMassTransitPublisher
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController(IBus publishEndpoint) : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] Activity message)
        {
            await _publishEndpoint.Publish(message);
            return Ok("Message sent to Azure Service Bus topic.");
        }

        [HttpPost("event")]
        public async Task<IActionResult> SendEvent([FromBody] ActivityEvent message)
        {
            await _publishEndpoint.Publish(message);
            return Ok("Event sent to Azure Service Bus topic.");
        }
    }
}