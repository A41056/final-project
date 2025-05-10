namespace BuildingBlocks.Messaging.Events
{
    public record ProductRatingResponseEvent : IntegrationEvent
    {
        public Guid CorrelationId { get; init; }
        public List<ProductRatingData> Ratings { get; init; } = new();
    }
    public record ProductRatingData
    {
        public Guid ProductId { get; init; }
        public double AverageRating { get; init; }
    }
}
