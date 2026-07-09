using JobSeeker.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace JobSeeker.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(_emailSettings.SmtpServer)
                && !string.IsNullOrWhiteSpace(_emailSettings.SenderEmail)
                && !string.IsNullOrWhiteSpace(_emailSettings.Username)
                && !string.IsNullOrWhiteSpace(_emailSettings.Password);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (!IsConfigured())
            {
                _logger.LogWarning("Email service is not configured. Email to {Recipient} was not sent.", to);
                return;
            }

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = _emailSettings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Recipient}", to);
        }
    }
}
