
using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Basket.API.Basket.CheckoutBasket;

public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto) 
    : ICommand<CheckoutBasketResult>;
public record CheckoutBasketResult(bool IsSuccess);

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

public class CheckoutBasketCommandHandler
    (IBasketRepository repository, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        // get existing basket with total price
        // Set totalprice on basketcheckout event message
        // send basket checkout event to rabbitmq using masstransit
        // delete the basket

        var basket = await repository.GetBasket(command.BasketCheckoutDto.UserId, cancellationToken);
        if (basket == null)
        {
            return new CheckoutBasketResult(false);
        }

        var eventMessage = command.BasketCheckoutDto.Adapt<BasketCheckoutEvent>();
        eventMessage.TotalPrice = basket.TotalPrice;

        await publishEndpoint.Publish(eventMessage, cancellationToken);

        //Wait for OrderConfirmedEvent
        //await repository.DeleteBasket(command.BasketCheckoutDto.UserId, cancellationToken);

        return new CheckoutBasketResult(true);
    }
}
