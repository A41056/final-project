using Payment.Infrastructure.Models;

namespace Payment.Models.VnPays
{
    public class VnpayConfirmModel : IConfirmModel
    {
        public string DataQueryString { get; set; }
    }
}
