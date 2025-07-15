namespace AzureServiceBusMassTransitPublisher
{
    public class Activity
    {
        required public string Title { get; set; }
        required public string Description { get; set; }
        public DateTimeOffset FollowUpDate { get; set; }
    }

    public class ActivityEvent
    {
        required public string Title { get; set; }
        public DateTimeOffset FollowUpDate { get; set; }
    }
}