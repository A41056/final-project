namespace Ordering.Domain.Models;

public class OrderItem : Entity<OrderItemId>
{
    private readonly List<VariantProperty> _variantProperties = new();
    public IReadOnlyList<VariantProperty> VariantProperties => _variantProperties.AsReadOnly();

    internal OrderItem(OrderId orderId, ProductId productId, int quantity, decimal price, List<VariantProperty>? variantProperties = null)
    {
        Id = OrderItemId.Of(Guid.NewGuid());
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        _variantProperties = variantProperties ?? new List<VariantProperty>();
    }

    public OrderId OrderId { get; private set; } = default!;
    public ProductId ProductId { get; private set; } = default!;
    public int Quantity { get; private set; } = default!;
    public decimal Price { get; private set; } = default!;
}

public class VariantProperty
{
    public string Type { get; private set; }
    public string Value { get; private set; }
    public string? Image { get; private set; }

    public VariantProperty(string type, string value, string? image = null)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Image = image;
    }
}