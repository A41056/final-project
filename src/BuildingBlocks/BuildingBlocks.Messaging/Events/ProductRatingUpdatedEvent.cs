namespace BuildingBlocks.Messaging.Events
{
    public record ProductRatingUpdatedEvent : IntegrationEvent
    {
        public Guid ProductId { get; set; }
        public double NewAverageRating { get; set; }
    }
}
