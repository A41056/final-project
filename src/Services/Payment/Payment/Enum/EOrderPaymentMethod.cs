using System.ComponentModel;

namespace Payment.Enum
{
    public enum EOrderPaymentMethod
    {
        [Description("VNPay")]
        VNPay = 1,

        [Description("Momo")]
        MoMo = 2,

        [Description("Tiền mặt")]
        Cash = 3,

        [Description("Chuyển khoản")]
        MoneyTransfer = 4
    }
}
