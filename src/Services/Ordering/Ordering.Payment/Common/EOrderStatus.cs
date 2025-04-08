using System.ComponentModel;

namespace Ordering.Payment.Common
{
    public enum EOrderStatus
    {
        [Description("Chờ xác nhận")]
        PauseForConfirmation = 1,

        [Description("Chờ thanh toán")]
        PauseForPay = 2,

        [Description("Hoàn thành")]
        Done = 3,

        [Description("Thanh toán không thành công")]
        Failed = 4,

        [Description("Admin - Chờ duyệt")]
        AdminPauseForConfirmation = 5,

        [Description("Admin - Đang xử lý")]
        AdminProcessing = 6,

        [Description("Admin - Xử lý lỗi")]
        AdminProcessFailed = 7
    }
}
