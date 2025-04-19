namespace Ordering.Application.Dtos
{
    public record CatalogProductDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public List<ProductVariantDto> Variants { get; init; } = new();
    }

    public record ProductVariantDto
    {
        public List<VariantPropertyDto> Properties { get; init; } = new();
        public decimal Price { get; init; }
        public int StockCount { get; init; }
    }
}
