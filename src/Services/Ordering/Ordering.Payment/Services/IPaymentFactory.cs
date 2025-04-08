using Ordering.Payment.Common;
using Ordering.Payment.Infrastructure;

namespace Ordering.Payment.Services;
public interface IPaymentFactory
{
    IPaymentProvider CreatePaymentProvider(EOrderPaymentMethod type);
}
