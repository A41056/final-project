using Microsoft.Extensions.Logging;
using Ordering.Payment.Infrastructure.Impls;
using Ordering.Payment.Infrastructure.Models;
using Ordering.Payment.Models;
using Ordering.Payment.Models.Momo;
using System.Net.Http;

namespace Ordering.Payment.Services.Impls;
public class MoMoPaymentService : PaymentProvider<MomoPaymentModel, MomoTransactionModel, MomoConfirmModel>
{
    private readonly MomoSetting _momoSetting;
    private IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    public MoMoPaymentService(MomoSetting momoSetting, IHttpClientFactory httpClientFactory
        , ILogger logger)
    {
        _momoSetting = momoSetting;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override Task<ConfirmResponse> ConfirmPaymentAsync(MomoConfirmModel model)
    {
        throw new NotImplementedException();
    }

    protected override Task<TransactionInfo> GetTransactionInfoAsync(MomoTransactionModel model)
    {
        throw new NotImplementedException();
    }

    protected override Task<string> PaymentGenerateUrlAsync(MomoPaymentModel model)
    {
        throw new NotImplementedException();
    }
}
