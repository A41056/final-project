namespace Basket.API.Basket.DeleteBasket;

public record DeleteBasketCommand(Guid UserId) : ICommand<DeleteBasketResult>;
public record DeleteBasketResult(bool IsSuccess);

public class DeleteBasketCommandValidator : AbstractValidator<DeleteBasketCommand>
{
    public DeleteBasketCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserName is required");
    }
}

public class DeleteBasketCommandHandler(IBasketRepository repository) 
    : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
{
    public async Task<DeleteBasketResult> Handle(DeleteBasketCommand command, CancellationToken cancellationToken)
    {
        // TODO: delete basket from database and cache       
        await repository.DeleteBasket(command.UserId, cancellationToken);

        return new DeleteBasketResult(true);
    }
}
