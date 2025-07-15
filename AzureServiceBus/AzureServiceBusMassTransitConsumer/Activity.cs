namespace AzureServiceBusMassTransitConsumer
{
    public interface IActivity
    {
        public string Title { get; }
        public string Description { get; }
        public DateTimeOffset FollowUpDate { get; }
    }
}