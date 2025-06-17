using Basket.API.EventHandler;
using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Basket.API.Basket.CheckoutBasket;

public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto)
    : ICommand<CheckoutBasketResult>;
public record CheckoutBasketResult(bool IsSuccess, string? PaymentUrl);

public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketCommandValidator()
    {
        RuleFor(x => x.BasketCheckoutDto).NotNull().WithMessage("BasketCheckoutDto can't be null");
        RuleFor(x => x.BasketCheckoutDto.UserId).NotEmpty().WithMessage("UserId is required");
        RuleFor(x => x.BasketCheckoutDto.CustomerId).NotEmpty().WithMessage("CustomerId is required");
        RuleFor(x => x.BasketCheckoutDto.Items).NotEmpty().WithMessage("Basket items cannot be empty");
        RuleForEach(x => x.BasketCheckoutDto.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId is required");
            item.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("UnitPrice must be non-negative");
        });
    }
}

public class CheckoutBasketCommandHandler : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    private readonly IBasketRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CheckoutBasketCommandHandler(IBasketRepository repository, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var basket = await _repository.GetBasket(command.BasketCheckoutDto.UserId, cancellationToken);
        if (basket == null)
        {
            return new CheckoutBasketResult(false, null);
        }

        var eventMessage = command.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();
        eventMessage.TotalPrice = basket.TotalPrice;

        var userId = command.BasketCheckoutDto.UserId.ToString();

        Console.WriteLine($"[Register] Register PaymentUrlCreatedEvent for user {userId}");
        var paymentUrlTcs = PaymentUrlCreatedEventConsumer.RegisterPaymentUrlTask(userId);

        Console.WriteLine($"[Publish] Publish BasketCheckoutEvent for user {userId}");
        await _publishEndpoint.Publish(eventMessage, cancellationToken);

        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        var completedTask = await Task.WhenAny(paymentUrlTcs.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            return new CheckoutBasketResult(false, null);
        }

        var paymentUrl = await paymentUrlTcs.Task;

        await _repository.DeleteBasket(command.BasketCheckoutDto.UserId, cancellationToken);

        return new CheckoutBasketResult(true, paymentUrl);
    }
}
