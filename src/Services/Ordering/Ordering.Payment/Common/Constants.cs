namespace Ordering.Payment.Common
{
    public static class Constants
    {
        public static class VnPayResponseCode
        {
            public const string TransactionSuccessfully = "00";
            public const string OrderNotFound = "01";
            public const string OrderAlreadyConfirmed = "02";
            public const string InvalidAmount = "04";
            public const string CancelPayment = "24";
            public const string BankIsUnderMaintenance = "75";
            public const string InvalidSignature = "97";
            public const string OtherErrors = "99";
        }
    }
}
