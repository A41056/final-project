namespace Payment.API.Models
{
    public class OrderInfoPrePaymentModel
    {
        public Guid Id { get; set; }
        public string? OrderCode { get; set; }
        public decimal TotalPayment { get; set; }
        public DateTime? ApplyDiscountExpiredTime { get; set; }
        public int? ApplyDiscountTypeUse { get; set; }
        public string? ApplyDiscountCode { get; set; }
        public string? BillEmail { get; set; }
        public string? BillFullName { get; set; }
        public string? BillPhoneNumber { get; set; }
    }
}
