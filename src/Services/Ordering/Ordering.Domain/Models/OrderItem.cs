namespace Ordering.Domain.Models;

public class OrderItem : Entity<OrderItemId>
{
    private readonly List<VariantProperty> _variantProperties = new();
    public IReadOnlyList<VariantProperty> VariantProperties => _variantProperties.AsReadOnly();

    public OrderId OrderId { get; private set; } = default!;
    public ProductId ProductId { get; private set; } = default!;
    public int Quantity { get; private set; } = default!;
    public decimal Price { get; private set; } = default!;

    internal OrderItem(OrderId orderId, ProductId productId, int quantity, decimal price)
    {
        Id = OrderItemId.Of(Guid.NewGuid());
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }

    public void AddVariantProperty(VariantProperty variantProperty)
    {
        // Gán FK OrderItemId trước khi thêm
        variantProperty.SetOrderItemId(Id);
        _variantProperties.Add(variantProperty);
    }

    private OrderItem() { }
}

public class VariantProperty
{
    public Guid Id { get; private set; }
    public OrderItemId OrderItemId { get; private set; }
    public string Type { get; private set; }
    public string Value { get; private set; }
    public string? Image { get; private set; }

    public VariantProperty(string type, string value, string? image = null)
    {
        Id = Guid.NewGuid();
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Image = image;
    }

    internal void SetOrderItemId(OrderItemId orderItemId)
    {
        OrderItemId = orderItemId;
    }

    private VariantProperty() { }
}
