using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ordering.Payment.Common;
using Ordering.Payment.Helpers;
using Ordering.Payment.Infrastructure.Impls;
using Ordering.Payment.Infrastructure.Models;
using Ordering.Payment.Models;
using Ordering.Payment.Models.VnPays;
using Ordering.Payment.VnPayLibraries;
using System.Net;

namespace Ordering.Payment.Services.Impls;

public class VnPayPaymentService : PaymentProvider<VnpayPaymentModel, VnpayTransactionModel, VnpayConfirmModel>
{
    private readonly VnpaySetting _vnpaySetting;
    private IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public VnPayPaymentService(VnpaySetting vnpaySetting, IHttpClientFactory httpClientFactory
        , ILogger logger, IHttpContextAccessor httpContextAccessor)
    {
        _vnpaySetting = vnpaySetting;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<string> PaymentGenerateUrlAsync(VnpayPaymentModel model)
    {
        VnPayLibrary vnpay = new();
        string fullName = model.BillFullName.Trim();
        string firstName = string.Empty;
        string lastName = string.Empty;

        if (!string.IsNullOrEmpty(fullName))
        {
            var indexof = fullName.IndexOf(' ');
            if (indexof == -1)
            {
                firstName = fullName;
            }
            else
            {
                firstName = fullName.Substring(0, indexof);
                lastName = fullName.Substring(indexof + 1, fullName.Length - indexof - 1);
            }
        }

        // fix timezone utc+7
        var vnDateTime = DateTime.UtcNow.AddHours(7);

        vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION)
            .AddRequestData("vnp_Command", "pay")
            .AddRequestData("vnp_TmnCode", _vnpaySetting.VnpTmnCode)
            .AddRequestData("vnp_Amount", (model.Amount * 100).ToString())
            .AddRequestData("vnp_CreateDate", vnDateTime.ToString("yyyyMMddHHmmss"))
            .AddRequestData("vnp_CurrCode", "VND")
            .AddRequestData("vnp_Locale", "vn")
            .AddRequestData("vnp_ReturnUrl", _vnpaySetting.VnpReturnUrl)
            .AddRequestData("vnp_OrderInfo", $"{model.TxnRef}")
            .AddRequestData("vnp_OrderType", "billpayment")
            .AddRequestData("vnp_ExpireDate", vnDateTime.AddMinutes(_vnpaySetting.VnExpiredMinute).ToString("yyyyMMddHHmmss"))
            .AddRequestData("vnp_IpAddr", GetClientIpAddress())
            .AddRequestData("vnp_TxnRef", model.TxnRef.ToString())
            .AddRequestData("vnp_Bill_Mobile", model.BillMobile)
            .AddRequestData("vnp_Bill_Email", model.BillEmail)
            .AddRequestData("vnp_Bill_FirstName", firstName?.ConvertNonVietNamChar())
            .AddRequestData("vnp_Bill_LastName", lastName?.ConvertNonVietNamChar())
            .AddRequestData("vnp_Bill_Country", "VN");

        await Task.CompletedTask;
        return vnpay.CreateRequestUrl(_vnpaySetting.VnpUrl, _vnpaySetting.VnpHashSecret);
    }

    protected override async Task<TransactionInfo> GetTransactionInfoAsync(VnpayTransactionModel model)
    {
        _logger.LogInformation("Started getting transaction info");
        var vnpay = new VnPayLibrary();

        vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION)
            .AddRequestData("vnp_Command", "querydr")
            .AddRequestData("vnp_TmnCode", _vnpaySetting.VnpTmnCode)
            .AddRequestData("vnp_TxnRef", model.OrderCode)
            .AddRequestData("vnp_OrderInfo", model.OrderInfo)
            .AddRequestData("vnp_TransDate", model.PayDate.ToString("yyyyMMdd"))
            .AddRequestData("vnp_CreateDate", model.PayDate.ToString("yyyyMMdd"))
            .AddRequestData("vnp_IpAddr", GetClientIpAddress());

        var queryDr = vnpay.CreateRequestUrl(_vnpaySetting.Querydr, _vnpaySetting.VnpHashSecret);

        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"{queryDr}");
            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            (TransactionInfo transactionInfo, _) = VnPayLibraryHelper.ParseQueryStringToTransactionInfo(json);
            return transactionInfo;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            return null;
        }
    }

    protected override async Task<ConfirmResponse> ConfirmPaymentAsync(VnpayConfirmModel model)
    {
        var result = new ConfirmResponse();

        (TransactionInfo transactionInfo, VnPayLibrary vnPay) = VnPayLibraryHelper.ParseQueryStringToTransactionInfo(model.DataQueryString);
        if (transactionInfo == null || vnPay == null)
        {
            result.RspCode = Constants.VnPayResponseCode.OtherErrors;
            result.Message = "Other errors";

            return result;
        }

        if (transactionInfo.ResponseCode == Constants.VnPayResponseCode.CancelPayment)
        {
            result.RspCode = Constants.VnPayResponseCode.CancelPayment;
            result.Message = "Cancel Payment";

            return result;
        }

        bool checkSignature = vnPay.ValidateSignature(transactionInfo.SecureHash, _vnpaySetting.VnpHashSecret);
        if (!checkSignature)
        {
            _logger.LogError("Invalid signature, InputData={0}", model.DataQueryString);

            result.RspCode = Constants.VnPayResponseCode.InvalidSignature;
            result.Message = "Invalid Checksum";

            return result;
        }

        result.TransactionId = transactionInfo.TxnRef;
        result.Amount = transactionInfo.Amount;
        result.TransactionNo = transactionInfo.TransactionNo;
        result.TransactionStatus = transactionInfo.TransactionStatus;
        result.RspCode = transactionInfo.ResponseCode;
        result.BankCode = transactionInfo.BankCode;
        result.PayDate = DateTime.ParseExact(transactionInfo.PayDate, "yyyyMMddHHmmss", null).AddHours(-7);
        result.PaymentContent = transactionInfo.OrderInfo;
        result.CardType = transactionInfo.CardType;

        await Task.CompletedTask;
        return result;
    }

    protected virtual string GetClientIpAddress()
    {
        try
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (ipAddress == null || ipAddress == "::1")
            {
                ipAddress = VnPayLibraryHelper.GetIpAddress();
            }

            return ipAddress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
            return null;
        }
    }
}