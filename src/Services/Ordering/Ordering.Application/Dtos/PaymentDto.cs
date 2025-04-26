namespace Ordering.Application.Dtos;

public record PaymentDto(Guid Id, string CardName, string CardNumber, string Expiration, string Cvv, int PaymentMethod);
