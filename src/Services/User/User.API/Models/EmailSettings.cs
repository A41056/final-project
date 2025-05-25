namespace User.API.Models
{
    public class EmailSettings
    {
        public string DisplayName { get; set; }
        public string From { get; set; }
        public string SMTPServer { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
