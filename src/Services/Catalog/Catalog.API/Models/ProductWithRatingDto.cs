namespace Catalog.API.Models;

public class ProductWithRatingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ImageFiles { get; set; } = new();
    public bool IsHot { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public List<Guid> CategoryIds { get; set; } = new();
    public List<ProductVariant> Variants { get; set; } = new();
    public double AverageRating { get; set; }
}
