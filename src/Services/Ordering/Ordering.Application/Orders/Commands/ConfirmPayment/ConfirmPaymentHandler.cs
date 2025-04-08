using Marten;
using Ordering.Payment.Common;
using Ordering.Payment.Infrastructure.Models;
using Ordering.Payment.Models.VnPays;
using Ordering.Payment.Services;

namespace Ordering.Application.Orders.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, ConfirmPaymentResult>
{
    private readonly IPaymentFactory _paymentFactory;
    private readonly IDocumentSession _session;
    private readonly ILogger<ConfirmPaymentCommandHandler> _logger;

    public ConfirmPaymentCommandHandler(
        IPaymentFactory paymentFactory,
        IDocumentSession session,
        ILogger<ConfirmPaymentCommandHandler> logger)
    {
        _paymentFactory = paymentFactory;
        _session = session;
        _logger = logger;
    }

    public async Task<ConfirmPaymentResult> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentProvider = _paymentFactory.CreatePaymentProvider(EOrderPaymentMethod.VNPay);
        var vnpayConfirmModel = new VnpayConfirmModel { DataQueryString = request.QueryString };
        var confirmResponse = await paymentProvider.ConfirmPaymentAsync(vnpayConfirmModel);

        if (confirmResponse == null)
        {
            _logger.LogInformation("Payment confirmation failed: Input data required");
            return new ConfirmPaymentResult(
                Constants.VnPayResponseCode.OtherErrors,
                "Input data required",
                null, 0, null, null, DateTime.UtcNow, null);
        }

        switch (confirmResponse.RspCode)
        {
            case Constants.VnPayResponseCode.InvalidSignature:
                return new ConfirmPaymentResult(
                    Constants.VnPayResponseCode.InvalidSignature,
                    "Invalid Checksum",
                    confirmResponse.TransactionId,
                    confirmResponse.Amount,
                    confirmResponse.TransactionNo,
                    confirmResponse.TransactionStatus,
                    confirmResponse.PayDate.GetValueOrDefault(),
                    confirmResponse.PaymentContent);

            case Constants.VnPayResponseCode.CancelPayment:
                return new ConfirmPaymentResult(
                    Constants.VnPayResponseCode.CancelPayment,
                    "Cancel Payment",
                    confirmResponse.TransactionId,
                    confirmResponse.Amount,
                    confirmResponse.TransactionNo,
                    confirmResponse.TransactionStatus,
                    confirmResponse.PayDate.GetValueOrDefault(),
                    confirmResponse.PaymentContent);

            case Constants.VnPayResponseCode.OtherErrors:
                return new ConfirmPaymentResult(
                    Constants.VnPayResponseCode.TransactionSuccessfully,
                    "Confirm Success",
                    confirmResponse.TransactionId,
                    confirmResponse.Amount,
                    confirmResponse.TransactionNo,
                    confirmResponse.TransactionStatus,
                    confirmResponse.PayDate.GetValueOrDefault(),
                    confirmResponse.PaymentContent);
        }

        if (!Guid.TryParse(confirmResponse.TransactionId?.Replace("_", "-"), out Guid transactionId))
        {
            _logger.LogInformation("Payment failed: Invalid TransactionId format");
            return new ConfirmPaymentResult(
                Constants.VnPayResponseCode.OrderNotFound,
                "Order Not Found",
                confirmResponse.TransactionId,
                confirmResponse.Amount,
                confirmResponse.TransactionNo,
                confirmResponse.TransactionStatus,
                confirmResponse.PayDate.GetValueOrDefault(),
                confirmResponse.PaymentContent);
        }

        var order = await _session.Query<Order>()
            .FirstOrDefaultAsync(o => o.TransactionId == transactionId, cancellationToken);
        if (order == null)
        {
            _logger.LogInformation("Payment failed: Order not found for TransactionId={TransactionId}", transactionId);
            return new ConfirmPaymentResult(
                Constants.VnPayResponseCode.OrderNotFound,
                "Order not found",
                confirmResponse.TransactionId,
                confirmResponse.Amount,
                confirmResponse.TransactionNo,
                confirmResponse.TransactionStatus,
                confirmResponse.PayDate.GetValueOrDefault(),
                confirmResponse.PaymentContent);
        }

        var totalPayment = order.OrderItems.Sum(i => i.UnitPrice * i.Quantity);
        if (totalPayment != confirmResponse.Amount)
        {
            _logger.LogInformation("Payment failed: Amount mismatch for OrderCode={OrderCode}, Expected={Expected}, Received={Received}",
                order.OrderCode, totalPayment, confirmResponse.Amount);
            return new ConfirmPaymentResult(
                Constants.VnPayResponseCode.InvalidAmount,
                "Invalid amount",
                confirmResponse.TransactionId,
                confirmResponse.Amount,
                confirmResponse.TransactionNo,
                confirmResponse.TransactionStatus,
                confirmResponse.PayDate.GetValueOrDefault(),
                confirmResponse.PaymentContent);
        }

        if (order.Status == EOrderStatus.Done || order.Status == EOrderStatus.PauseForConfirmation)
        {
            return new ConfirmPaymentResult(
                order.Status == EOrderStatus.Done ? Constants.VnPayResponseCode.OrderAlreadyConfirmed : Constants.VnPayResponseCode.TransactionSuccessfully,
                order.Status == EOrderStatus.Done ? "Order already confirmed" : "Confirm Success",
                confirmResponse.TransactionId,
                confirmResponse.Amount,
                confirmResponse.TransactionNo,
                confirmResponse.TransactionStatus,
                confirmResponse.PayDate.GetValueOrDefault(),
                confirmResponse.PaymentContent);
        }

        if (confirmResponse.RspCode == Constants.VnPayResponseCode.BankIsUnderMaintenance)
        {
            _logger.LogInformation("Bank under maintenance: TransactionId={TransactionId}, OrderCode={OrderCode}",
                transactionId, order.OrderCode);
            await UpdateOrderStatus(order, EOrderStatus.PauseForConfirmation);
        }
        else if (confirmResponse.RspCode == Constants.VnPayResponseCode.TransactionSuccessfully &&
                 confirmResponse.TransactionStatus == Constants.VnPayResponseCode.TransactionSuccessfully)
        {
            await HandleSuccessfulPayment(order, confirmResponse);
        }
        else
        {
            _logger.LogInformation("Payment failed: TransactionId={TransactionId}, OrderCode={OrderCode}",
                transactionId, order.OrderCode);
            await UpdateOrderStatus(order, EOrderStatus.Failed);
        }

        await _session.SaveChangesAsync(cancellationToken);

        return new ConfirmPaymentResult(
            confirmResponse.RspCode == Constants.VnPayResponseCode.TransactionSuccessfully ? Constants.VnPayResponseCode.TransactionSuccessfully : confirmResponse.RspCode,
            confirmResponse.RspCode == Constants.VnPayResponseCode.TransactionSuccessfully ? "Confirm Success" : confirmResponse.Message,
            confirmResponse.TransactionId,
            confirmResponse.Amount,
            confirmResponse.TransactionNo,
            confirmResponse.TransactionStatus,
            confirmResponse.PayDate.GetValueOrDefault(),
            confirmResponse.PaymentContent);
    }

    private async Task HandleSuccessfulPayment(Order order, ConfirmResponse confirmResponse)
    {
        _logger.LogInformation("Payment successful: OrderCode={OrderCode}, TransactionId={TransactionId}, VNPayTranId={TransactionNo}",
            order.OrderCode, confirmResponse.TransactionId, confirmResponse.TransactionNo);

        await UpdateOrderStatus(order, EOrderStatus.Done);
        order.PayDate = confirmResponse.PayDate ?? DateTime.UtcNow;
        order.TransactionId = Guid.Parse(confirmResponse.TransactionId);

        await NotifyExternalServices(order);
    }

    private async Task UpdateOrderStatus(Order order, EOrderStatus newStatus)
    {
        _logger.LogInformation("Changing Order Status: OrderCode={OrderCode}, OldStatus={OldStatus}, NewStatus={NewStatus}",
            order.OrderCode, order.Status.ToString(), newStatus.ToString());
        order.Status = newStatus;
        _session.Store(order);
        await Task.CompletedTask;
    }

    private async Task NotifyExternalServices(Order order)
    {
        _logger.LogInformation("Notifying external services for OrderCode={OrderCode}", order.OrderCode);
        // Placeholder: Publish events for Catalog (stock), Invoice, etc.
        // Example:
        // await _eventBus.Publish(new OrderConfirmedEvent(order.OrderCode, order.OrderItems));
        await Task.CompletedTask; // Replace with actual event publishing
    }
}

public partial class Order
{
    public string OrderCode { get; set; } // Add if not present
    public Guid? TransactionId { get; set; } // Store VNPay TransactionId
    public DateTime? PayDate { get; set; } // Store payment date
    public List<OrderItem> OrderItems { get; set; } // Already has this
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Price from BasketCheckoutEvent
}