namespace BuildingBlocks.Messaging.Events
{
    public record PaymentUrlCreatedEvent : IntegrationEvent
    {
        public string OrderCode { get; init; }
        public Guid UserId { get; init; }
        public string PaymentUrl { get; init; } = string.Empty;
    }
}
