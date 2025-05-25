namespace User.API.Models
{
    public class MailRequest
    {
        public string From { get; set; }
        public string ToAddress { get; set; }
        public List<string> ToAddresses { get; set; } = new();
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<MailAttachment> Attachments { get; set; } = new();
    }

    public class MailAttachment
    {
        public string FileName { get; set; }
        public byte[] FileByteArray { get; set; }
    }

}
