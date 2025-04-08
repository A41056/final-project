using Ordering.Payment.Infrastructure.Models;

namespace Ordering.Payment.Models.VnPays
{
    public class VnpayConfirmModel : IConfirmModel
    {
        public string DataQueryString { get; set; }
    }
}
