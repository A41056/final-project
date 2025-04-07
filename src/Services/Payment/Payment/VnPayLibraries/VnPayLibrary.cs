using System.Net;
using System.Text;
using static Payment.Helpers.VnPayLibraryHelper;

namespace Payment.VnPayLibraries
{
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";
        public readonly SortedList<string, string> _requestData = new(new VnPayCompare());
        public readonly SortedList<string, string> _responseData = new(new VnPayCompare());

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out string retValue) ? retValue : string.Empty;
        }
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            StringBuilder data = new();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string queryString = data.ToString();

            baseUrl += "?" + queryString;
            string signData = queryString;
            if (signData.Length > 0)
            {

                signData = signData.Remove(data.Length - 1, 1);
            }
            string vnp_SecureHash = HmacSHA512(vnpHashSecret, signData);
            baseUrl += $"{VnPayParameter.RequestParamName.VNP_SECUREHASH}={vnp_SecureHash}";

            return baseUrl;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            string rspRaw = GetResponseData();
            string myChecksum = HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
        private string GetResponseData()
        {

            StringBuilder data = new();
            if (_responseData.ContainsKey(VnPayParameter.RequestParamName.VNP_SECUREHASHTYPE))
            {
                _responseData.Remove(VnPayParameter.RequestParamName.VNP_SECUREHASHTYPE);
            }
            if (_responseData.ContainsKey(VnPayParameter.RequestParamName.VNP_SECUREHASH))
            {
                _responseData.Remove(VnPayParameter.RequestParamName.VNP_SECUREHASH);
            }
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }
            return data.ToString();
        }
    }
}
