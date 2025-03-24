namespace Media.API.Model;

public class CloudflareSettingOption
{
    public const string CloudflareSetting = "CloudflareSetting";

    public string ServiceUrl { get; set; }
    public string AccessKeyId { get; set; }
    public string SecretAccessKey { get; set; }
    public string BucketName { get; set; }
}
