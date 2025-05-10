namespace BuildingBlocks.Messaging.Events
{
    public record ProductRequestRatingEvent : IntegrationEvent
    {
        public List<Guid> ProductIds { get; init; } = new();
    }
}
