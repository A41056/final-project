public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ImageFiles { get; set; } = new();
    public bool IsHot { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public DateTime Modified { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    public List<Guid> CategoryIds { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<ProductVariant> Variants { get; set; } = new();
    public double AverageRating { get; set; } = 0;
}

public class ProductVariant
{
    public List<VariantProperty> Properties { get; set; } = new();
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockCount { get; set; }
}

public class VariantProperty
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Image { get; set; }
}