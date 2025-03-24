namespace Media.API.Helper;

public static class StringExtensions
{
    private static readonly string[] VietNamChar = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };

    public static string ConvertNonASCII(this string str)
    {
        str = str.Trim();
        for (int i = 1; i < VietNamChar.Length; i++)
        {
            for (int j = 0; j < VietNamChar[i].Length; j++)
            {
                str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
            }
        }

        str = str.Replace("\u202f", "-");
        str = str.Replace(" ", "-");
        str = str.Replace("--", "-");
        str = str.Replace("?", "");
        str = str.Replace("&", "");
        str = str.Replace(",", "");
        str = str.Replace(":", "");
        str = str.Replace("!", "");
        str = str.Replace("'", "");
        str = str.Replace("\"", "");
        str = str.Replace("%", "");
        str = str.Replace("#", "");
        str = str.Replace("$", "");
        str = str.Replace("*", "");
        str = str.Replace("`", "");
        str = str.Replace("~", "");
        str = str.Replace("@", "");
        str = str.Replace("^", "");
        str = str.Replace(".", "");
        str = str.Replace("/", "");
        str = str.Replace(">", "");
        str = str.Replace("<", "");
        str = str.Replace("[", "");
        str = str.Replace("]", "");
        str = str.Replace(";", "");
        str = str.Replace("+", "");
        return str.ToLower();
    }
}
