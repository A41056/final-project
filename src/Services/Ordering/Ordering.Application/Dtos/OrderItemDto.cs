namespace Ordering.Application.Dtos;

public record OrderItemDto
(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    List<VariantPropertyDto> VariantProperties
);

public record VariantPropertyDto
{
    public string Type { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Image { get; init; }
};
