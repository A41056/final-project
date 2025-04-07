namespace Payment.Models
{
    public class BillingSettingOptions
    {
        public const string BillingSetting = "BillingSetting";

        public VnpaySetting VnpaySetting { get; set; }
        public MomoSetting MomoSetting { get; set; }
    }

    public class VnpaySetting
    {
        public string VnpUrl { get; set; }
        public string VnpHashSecret { get; set; }
        public string VnpTmnCode { get; set; }
        public string Querydr { get; set; }
        public string VnpReturnUrl { get; set; }
        public int VnExpiredMinute { get; set; }
    }

    public class MomoSetting
    {
    }
}
