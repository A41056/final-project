public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; 
    public string Description { get; set; } = string.Empty; 
    public string ImageFile { get; set; } = string.Empty; 
    public decimal Price { get; set; }
    public bool IsHot { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Modified { get; set; } = DateTime.UtcNow;
    public List<Guid> CategoryIds { get; set; } = new();
}