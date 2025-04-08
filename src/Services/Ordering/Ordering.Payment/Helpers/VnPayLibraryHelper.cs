using Ordering.Payment.Infrastructure.Models;
using Ordering.Payment.VnPayLibraries;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Ordering.Payment.Helpers
{
    public static class VnPayLibraryHelper
    {
        public static VnPayLibrary AddRequestData(this VnPayLibrary vnPayLibrary, string key, string value)
        {
            if (vnPayLibrary == null)
            {
                throw new ArgumentNullException(nameof(VnPayLibrary));
            }

            if (!string.IsNullOrEmpty(value))
            {
                vnPayLibrary._requestData.Add(key, value);
            }

            return vnPayLibrary;
        }
        public static VnPayLibrary AddResponseData(this VnPayLibrary vnPayLibrary, string key, string value)
        {
            if (vnPayLibrary == null)
            {
                throw new ArgumentNullException(nameof(VnPayLibrary));
            }

            if (!string.IsNullOrEmpty(value))
            {
                vnPayLibrary._responseData.Add(key, value);
            }

            return vnPayLibrary;
        }

        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        public static string GetIpAddress()
        {
            var addlistDNS = Dns.GetHostEntry(Dns.GetHostName());
            var strIPV4 = addlistDNS != null && addlistDNS.AddressList != null && addlistDNS.AddressList.Length > 1
                ? addlistDNS.AddressList[1].ToString() : string.Empty;

            return strIPV4;
        }

        public class VnPayCompare : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                var vnpCompare = CompareInfo.GetCompareInfo("en-US");
                return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
            }
        }

        public static (TransactionInfo, VnPayLibrary) ParseQueryStringToTransactionInfo(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (null, null);

            var vnpay = new VnPayLibrary();
            NameValueCollection vnpayData = HttpUtility.ParseQueryString(path);
            foreach (string s in vnpayData)
            {
                if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(s, vnpayData[s]);
                }
            }

            return (new TransactionInfo
            {
                TxnRef = vnpay.GetResponseData("vnp_TxnRef"),
                Amount = Convert.ToInt64(string.IsNullOrWhiteSpace(vnpay.GetResponseData("vnp_Amount"))
                    ? null
                    : vnpay.GetResponseData("vnp_Amount")) / 100,
                BankCode = vnpay.GetResponseData("vnp_BankCode"),
                OrderInfo = vnpay.GetResponseData("vnp_OrderInfo"),
                PayDate = vnpay.GetResponseData("vnp_PayDate"),
                ResponseCode = vnpay.GetResponseData("vnp_ResponseCode"),
                Message = vnpay.GetResponseData("vnp_Message"),
                SecureHash = vnpay.GetResponseData("vnp_SecureHash"),
                Trace = vnpay.GetResponseData("vnp_Trace"),
                TransactionNo = vnpay.GetResponseData("vnp_TransactionNo"),
                TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus"),
                TransactionType = vnpay.GetResponseData("vnp_TransactionType"),
                CardType = vnpay.GetResponseData("vnp_CardType")
            }, vnpay);
        }
    }
}
