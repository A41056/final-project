namespace User.API.Services
{
    using global::User.API.Models;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using MimeKit;
    using System.Net.Mail;

    public interface IEmailService
    {
        Task SendEmailAsync(MailRequest request, CancellationToken cancellationToken = default);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(MailRequest request, CancellationToken cancellationToken = default)
        {
            var emailMessage = new MimeMessage
            {
                Sender = new MailboxAddress(_settings.DisplayName, request.From ?? _settings.From),
                Subject = request.Subject
            };

            var builder = new BodyBuilder
            {
                HtmlBody = request.Body
            };

            foreach (var file in request.Attachments)
            {
                if (file.FileByteArray?.Length > 0)
                {
                    builder.Attachments.Add(file.FileName, file.FileByteArray);
                }
            }

            emailMessage.Body = builder.ToMessageBody();

            if (request.ToAddresses?.Any() == true)
            {
                foreach (var to in request.ToAddresses)
                    emailMessage.To.Add(MailboxAddress.Parse(to));
            }
            else if (!string.IsNullOrEmpty(request.ToAddress))
            {
                emailMessage.To.Add(MailboxAddress.Parse(request.ToAddress));
            }
            else
            {
                throw new ArgumentException("No recipient address specified.");
            }

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                await smtp.ConnectAsync(_settings.SMTPServer, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
                await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
                await smtp.SendAsync(emailMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
                throw;
            }
            finally
            {
                await smtp.DisconnectAsync(true, cancellationToken);
            }
        }
    }

}
