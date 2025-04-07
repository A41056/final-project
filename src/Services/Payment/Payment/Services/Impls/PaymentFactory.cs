using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.Infrastructure;
using Payment.Models;
using Payment.Services;
using StudyForce.Core.Enums;

namespace Payment.Services.Impls;
public class PaymentFactory : IPaymentFactory
{
    private readonly BillingSettingOptions _billingSetting;
    private IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PaymentFactory> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentFactory(IOptions<BillingSettingOptions> billingSettingOptions,
        IHttpClientFactory httpClientFactory, ILogger<PaymentFactory> logger, IHttpContextAccessor httpContextAccessor)
    {
        _billingSetting = billingSettingOptions.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }


    public IPaymentProvider CreatePaymentProvider(EOrderPaymentMethod type)
    {
        return type switch
        {
            EOrderPaymentMethod.VNPay => new VnPayPaymentService(_billingSetting.VnpaySetting, _httpClientFactory, _logger, _httpContextAccessor),
            EOrderPaymentMethod.MoMo => new MoMoPaymentService(_billingSetting.MomoSetting, _httpClientFactory, _logger),
            _ => new VnPayPaymentService(_billingSetting.VnpaySetting, _httpClientFactory, _logger, _httpContextAccessor)
        };
    }
}
