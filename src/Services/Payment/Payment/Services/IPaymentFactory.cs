using Payment.Enum;
using Payment.Infrastructure;

namespace Payment.Services;
public interface IPaymentFactory
{
    IPaymentProvider CreatePaymentProvider(EOrderPaymentMethod type);
}
