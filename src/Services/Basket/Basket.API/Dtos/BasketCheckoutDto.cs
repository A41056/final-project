namespace Basket.API.Dtos;

public record BasketCheckoutDto
{
    public Guid UserId { get; init; }
    public Guid CustomerId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string EmailAddress { get; init; } = string.Empty;
    public string AddressLine { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string CardName { get; init; } = string.Empty;
    public string CardNumber { get; init; } = string.Empty;
    public string Expiration { get; init; } = string.Empty;
    public string CVV { get; init; } = string.Empty;
    public int PaymentMethod { get; init; }
    public List<BasketItemDto> Items { get; init; } = new();
}

public record BasketItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public List<VariantPropertyDto> VariantProperties { get; init; } = new();
}

public record VariantPropertyDto
{
    public string Type { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Image { get; init; }
}
