namespace BuildingBlocks.Messaging.Events
{
    public record ProductUpdatedEvent : IntegrationEvent
    {
        public Guid ProductId { get; init; }
        public string Name { get; init; } = string.Empty;
        public List<ProductVariantDto> Variants { get; init; } = new();
    }

    public record ProductVariantDto
    {
        public decimal Price { get; init; }
        public int StockCount { get; init; }
        public List<VariantPropertyDto> Properties { get; init; } = new();
    }

    public record VariantPropertyDto
    {
        public string Type { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string? Image { get; init; }
    }
}
