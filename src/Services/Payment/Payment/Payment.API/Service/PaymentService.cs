using Payment.Common;
using Payment.Enum;
using Payment.Models.VnPays;
using Payment.Services;

namespace Payment.API.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentFactory _paymentFactory;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentFactory paymentFactory, ILogger<PaymentService> logger) 
        {
            _paymentFactory = paymentFactory;
            _logger = logger;
        }

        public async Task<ConfirmPaymentResponse> ConfirmPaymentAsync(string vnpDataQueryString)
        {
            var result = new ConfirmPaymentResponse();

            var res = await _paymentFactory
                .CreatePaymentProvider(EOrderPaymentMethod.VNPay)
                .ConfirmPaymentAsync(new VnpayConfirmModel
                {
                    DataQueryString = vnpDataQueryString
                });

            // implement bussiness
            if (res == null)
            {
                _logger.LogInformation("Thanh toan khong thanh cong");

                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.OtherErrors,
                    Message = "Input data required"
                };
            }

            if (res.RspCode == Constants.VnPayResponseCode.OtherErrors)
            {
                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.TransactionSuccessfully,
                    Message = "Confirm Success"
                };
            }

            if (res.RspCode == Constants.VnPayResponseCode.InvalidSignature)
            {
                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.InvalidSignature,
                    Message = "Invalid Checksum"
                };
            }

            if (res.RspCode == Constants.VnPayResponseCode.CancelPayment)
            {
                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.CancelPayment,
                    Message = "Cancel Payment"
                };
            }

            bool isValid = Guid.TryParse(res.TransactionId.Replace("_", "-"), out Guid transactionId);
            if (!isValid)
            {
                _logger.LogInformation("Thanh toan khong thanh cong");

                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.OrderNotFound,
                    Message = "Order Not Found"
                };
            }
            var paymentTransactionInfo =
                await _shoppingCartRepository
                    .GetPaymentTransactionAsync(transactionId);
            if (paymentTransactionInfo == null)
            {
                _logger.LogInformation("Thanh toan khong thanh cong, TransactionId={0}", transactionId);

                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.OrderNotFound,
                    Message = "Order not found"
                };
            }

            // get order by orderCode
            var orderInfo = await _shoppingCartRepository
                .GetOrderInfoForConfirmPayment(paymentTransactionInfo.OrderCode);
            // check order is null
            if (orderInfo == null)
            {
                _logger.LogInformation("Thanh toan khong thanh cong, TransactionId={0}", res.TransactionId);

                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.OrderNotFound,
                    Message = "Order not found"
                };
            }

            // check the same amount
            if (orderInfo.TotalPayment != res.Amount)
            {
                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.InvalidAmount,
                    Message = "Invalid amount"
                };
            }

            // check order status
            if (paymentTransactionInfo.Status == Constants.VnPayResponseCode.TransactionSuccessfully &&
                (orderInfo.Status == EOrderStatus.Done
                 || orderInfo.Status == EOrderStatus.PauseForConfirmation))
            {
                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.OrderAlreadyConfirmed,
                    Message = "Order already confirmed"
                };
            }

            if (paymentTransactionInfo.Status != Constants.VnPayResponseCode.TransactionSuccessfully &&
                (orderInfo.Status == EOrderStatus.Done
                 || orderInfo.Status == EOrderStatus.PauseForConfirmation))
            {
                return new ConfirmPaymentResponse
                {
                    RspCode = Constants.VnPayResponseCode.TransactionSuccessfully,
                    Message = "Confirm Success"
                };
            }

            // vnp_ResponseCode = 75 => Loi ngan hang bao tri
            if (res.RspCode == Constants.VnPayResponseCode.BankIsUnderMaintenance)
            {
                // Update Order status = PauseForConfirmation
                _logger.LogInformation("The bank is under maintenance, TransactionId={0}, OrderCode={1}",
                    paymentTransactionInfo.Id, orderInfo.OrderCode);
                _logger.LogInformation("Change Order Status, OrderCode={0}, Old Status={1}, New Status={2}",
                    orderInfo.OrderCode, orderInfo.Status.EnumGetDescription(),
                    EOrderStatus.PauseForConfirmation.EnumGetDescription());
                await _shoppingCartRepository.ChangeOrderStatusAsync(orderInfo.OrderCode, EOrderStatus.PauseForConfirmation);
            }
            else
            {
                // check vnp_ResponseCode && vnp_TransactionStatus
                if (res.RspCode == Constants.VnPayResponseCode.TransactionSuccessfully &&
                    res.TransactionStatus == Constants.VnPayResponseCode.TransactionSuccessfully)
                {
                    //Thanh toan thanh cong
                    _logger.LogInformation(
                        "Payment Successfully, OrderCode={0}, TransactionId={1}, VNPAY TranId={2}",
                        orderInfo.OrderCode, res.TransactionId,
                        res.TransactionNo);
                    // Update Order status = Done
                    _logger.LogInformation("Change Order Status, OrderCode={0}, Old Status={1}, New Status={2}",
                        orderInfo.OrderCode, orderInfo.Status.EnumGetDescription(),
                        EOrderStatus.Done.EnumGetDescription());
                    await _shoppingCartRepository.ChangeOrderStatusAsync(orderInfo.OrderCode, EOrderStatus.Done);

                    // Update Receipt code
                    var newReceiptCode = await _shoppingCartRepository.GetNewReceiptCodeInDBAsync(orderInfo.Id) ?? Constants.Common.DefaultReceiptCode;

                    await _shoppingCartRepository.UpdateReceiptCodeAsync(orderInfo.OrderCode, newReceiptCode);
                    await _invoiceRepository.UpdateReceiptCodeAsync(orderInfo.OrderCode, newReceiptCode);

                    //Auto add store course from order detail
                    _logger.LogInformation("Auto add store course from order detail, OrderCode={0}, Payment date={1}",
                        orderInfo.OrderCode, res.PayDate);
                    await _shoppingCartRepository.AutoInsertStoreCourseByOrderCodeAsync(orderInfo.OrderCode,
                        res.PayDate ?? DateTime.UtcNow);

                    // Update discount_web_user
                    await _shoppingCartRepository.UpdateDiscountWebUserAsync(orderInfo.OrderCode);

                    var currentUserId = await _shoppingCartRepository.GetWebUserIdFromOrderId(orderInfo.Id);
                    var coursePrices = await _storeCourseRepository.GetCourseListInOrderAsync(orderInfo.Id);
                    var grpCoursePrices = coursePrices.GroupBy(x => x.ProductType);
                    var coursePricesExamTool = grpCoursePrices.FirstOrDefault(x => x.Key == (int)EProductType.ExamTool);
                    var coursePricesLearnWithTeacher = grpCoursePrices.FirstOrDefault(x => x.Key == (int)EProductType.LearnWithTeacher);

                    // ProcessExamTool
                    await ProcessExamTool(orderInfo, currentUserId, coursePricesExamTool, coursePrices);

                    // LearnWithTeacher
                    if (coursePricesLearnWithTeacher != null)
                    {
                        // create classin user
                        //await CreateClassinUser(currentUserId);

                        // generate live class booking
                        //await GenerateLiveClassBooking(currentUserId, coursePricesLearnWithTeacher);
                    }

                    // get list course courseId by order code
                    var lstCourseId = await _orderRepository.GetListCourseIdByOrderCodeAsync(orderInfo.OrderCode);

                    // remove cache key
                    foreach (var courseId in lstCourseId)
                    {
                        // string redisKey = string.Format(WebConstants.RedisUserAccessCourseKey, currentUserId, courseId);
                        // await _distributedCacheProvider
                        //     .Cache<string>()
                        //     .Remove(redisKey);
                    }

                    // move all draft order detail to new order
                    await MoveDraftOrderDetailToNewOrderAfterPaymentSuccess(orderInfo.Id, currentUserId);

                    // update order date = payment date
                    await _orderRepository
                        .UpdateOrderPayDate(orderInfo.Id, res.PayDate.Value);
                }
                // Cac loi khac
                // Update Order status = Failed
                else
                {
                    _logger.LogInformation("Other Payment Errors, TransactionId={0}, OrderCode={1}",
                        paymentTransactionInfo.Id, orderInfo.OrderCode);
                    _logger.LogInformation("Change Order Status, OrderCode={0}, Old Status={1}, New Status={2}",
                        orderInfo.OrderCode, orderInfo.Status.EnumGetDescription(),
                        EOrderStatus.Failed.EnumGetDescription());
                    await _shoppingCartRepository.ChangeOrderStatusAsync(orderInfo.OrderCode, EOrderStatus.Failed);
                }

                result.RspCode = Constants.VnPayResponseCode.TransactionSuccessfully;
                result.Message = "Confirm Success";
            }

            // Update transaction into payment_transaction table
            _logger.LogInformation(
                "Update Transaction Payment, TransactionId={0}, TransactionCode={1}, Transaction Status={2}, OrderCode={3}",
                paymentTransactionInfo.Id, res.TransactionNo, res.TransactionStatus, orderInfo.OrderCode);
            var transactionPaymentModel = new PaymentTransactionInsertOrUpdateModel
            {
                Id = paymentTransactionInfo.Id,
                OrderCode = orderInfo.OrderCode,
                TransactionCode = res.TransactionNo,
                BankCode = res.BankCode,
                TraceId = res.TransactionNo,
                PaymentContent = res.PaymentContent,
                Status = res.TransactionStatus,
                PayDate = res.PayDate,
                CardType = res.CardType
            };

            await _shoppingCartRepository.UpdatePaymentTransactionAsync(transactionPaymentModel);
            _logger.LogInformation(
                "Completed Confirm Payment, TransactionId={0}, TransactionCode={1}, Transaction Status={2}",
                orderInfo.OrderCode, res.TransactionNo, res.TransactionStatus);

            return result;
        }

        public async Task<string> PaymentGenerateUrlAsync(PaymentGenerateUrlRequest request)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(Constants.ClaimTypes.UserId).Value.ToGuid();
            // get order info pre payment process
            var orderInfoPrePayment = await _shoppingCartRepository
                .GetOrderInfoPrePayment(request.OrderCode, currentUserId);

            if (orderInfoPrePayment == null)
                throw new ApiValidationException(WebConstants.ValidationMessages.ShoppingCartMessage.OrderNotFound, null);

            if (!string.IsNullOrEmpty(orderInfoPrePayment.ApplyDiscountCode))
            {
                await ReCheckDiscountStillValid(orderInfoPrePayment);
            }

            var paymentTransactionId = Guid.NewGuid();
            string paymentUrlRedirect = await _paymentFactory
                .CreatePaymentProvider(request.PaymentMethod)
                .PaymentGenerateUrlAsync(new VnpayPaymentModel
                {
                    Amount = Convert.ToInt32(orderInfoPrePayment.TotalPayment),
                    BillEmail = orderInfoPrePayment.BillEmail,
                    BillFullName = orderInfoPrePayment.BillFullName,
                    BillMobile = orderInfoPrePayment.BillPhoneNumber,
                    OrderInfo = request.OrderCode,
                    TxnRef = paymentTransactionId.ToString().Replace("-", "_")
                });

            // insert transaction into payment_transaction table
            _logger.LogInformation(
                "START: Insert Transaction Payment, TransactionId={0}", orderInfoPrePayment.OrderCode);
            var transactionPaymentModel = new PaymentTransactionInsertOrUpdateModel
            {
                Id = paymentTransactionId,
                OrderCode = orderInfoPrePayment.OrderCode
            };

            await _shoppingCartRepository.InsertPaymentTransactionAsync(transactionPaymentModel);
            _logger.LogInformation(
                "END: Insert Transaction Payment, TransactionId={0}", orderInfoPrePayment.OrderCode);

            // Update payment method
            await _orderRepository.UpdatePaymentMethodAsync(orderInfoPrePayment.Id, request.PaymentMethod);

            return paymentUrlRedirect;
        }

        public async Task<IEnumerable<TransactionInfo>> TransactionInfoAsync(TransactionInfoRequest request)
        {
            var result = new List<TransactionInfo>();
            var transactionPayments = await _shoppingCartRepository.GetPaymentTransactionByOrderCodeAsync(request.OrderCode);
            var models = transactionPayments.ToList();
            if (transactionPayments == null || !models.Any())
                throw new ApiValidationException(WebConstants.ValidationMessages.ShoppingCartMessage.TransactionPaymentNotFound, null);
            foreach (var transactionPayment in models)
            {
                var transactionInfo = await _paymentFactory
                    .CreatePaymentProvider(transactionPayment.PaymentMethod)
                    .GetTransactionInfoAsync(new VnpayTransactionModel
                    {
                        OrderCode = transactionPayment.Id.ToString(),
                        PayDate = transactionPayment.PayDate,
                        OrderInfo = $"queryDr OrderId:  {request.OrderCode}"
                    });

                result.Add(transactionInfo);
            }

            return result;
        }

        private async Task ReCheckDiscountStillValid(OrderInfoPrePaymentModel order)
        {
            var discount = await _discountRepository
                .GetDiscountByCodeAsync(order.ApplyDiscountCode);

            if (discount == null)
            {
                await _shoppingCartRepository.ResetDiscount(order.Id);
                return;
            }

            // check discount status
            var discountStatus = discount.Status;
            if (discountStatus != EDiscountStatus.Active)
            {
                await _shoppingCartRepository.ResetDiscount(order.Id);
                return;
            }

            // check valid date
            var discountValidFrom = discount.ValidFrom;
            var discountValidTo = discount.ValidTo;
            var validDate = discountValidFrom <= DateTime.UtcNow && DateTime.UtcNow <= discountValidTo;
            if (!validDate)
            {
                await _shoppingCartRepository.ResetDiscount(order.Id);
                return;
            }

            // user id
            var currentUserId = _httpContextAccessor.HttpContext.User.FindFirst(Constants.ClaimTypes.UserId).Value.ToGuid();

            // get list discount web user : no use + date use by discount code
            var discountWebUser = await _discountRepository
                .GetDiscountWebUserByCodeAsync(discount.Code);
            var discountNoUse = discountWebUser.Select(x => x.NoUse).Sum(); // number of discount already in use

            // discount web user info
            var discountWebUserInfo = discountWebUser.FirstOrDefault(x => x.WebUserId == currentUserId);

            // check if web user can use the code
            if (discountWebUserInfo == null)
            {
                await _shoppingCartRepository.ResetDiscount(order.Id);
                return;
            }

            var discountWebUserNoUse = discountWebUserInfo.NoUse.GetValueOrDefault(); // number user already use

            // discount period number by type use
            // all check for limit use
            // no limited -> no other check
            // on code -> check for period use for code
            // on day -> check for period use in a day
            var discountPeriodNo = discount.PeriodNo; // limit use for code / in a day

            // check discount type use
            var discountTypeUse = discount.TypeUse;
            var discountLimitedUse = discount.LimitedUse; // limit discount number

            // check theo số lượng đã có người dùng ~ tổng số lượng ban đầu
            if (discountNoUse >= discountLimitedUse)
            {
                await _shoppingCartRepository.ResetDiscount(order.Id);
                return;
            }

            switch (discountTypeUse)
            {
                case EDiscountTypeUse.NoLimited:
                    // no other check

                    break;

                case EDiscountTypeUse.OnCode:
                    // check theo số lượng giới hạn cho một người dùng theo code
                    if (discountWebUserNoUse > discountPeriodNo)
                    {
                        await _shoppingCartRepository.ResetDiscount(order.Id);
                        return;
                    }

                    break;

                case EDiscountTypeUse.OnDay:
                    if (discountWebUserInfo.DateUse.Date == DateTime.UtcNow.Date)
                    {
                        // check theo số lượng giới hạn cho một người dùng trong 1 ngày
                        if (discountWebUserNoUse > discountPeriodNo)
                        {
                            await _shoppingCartRepository.ResetDiscount(order.Id);
                            return;
                        }
                    }
                    else
                    {
                        await _discountRepository
                            .ResetNoUseDiscountWebUserAsync(discount.Code, currentUserId);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(WebConstants.ValidationMessages.DiscountMessage.DiscountTypeUseIsNotValid);
            }
        }

        public async Task PushQueueConfirmPaymentAsync(string vnpDataQueryString)
        {
            _rabitMQProducer.SendMessage(vnpDataQueryString, Constants.RabbitMQ.TopicPaymentOnline,
                  string.Format(Constants.RabbitMQ.PushMessagePaymentOnlineRoutingkey, "ConfirmPayment"));

            await Task.CompletedTask;
        }
    }
}
