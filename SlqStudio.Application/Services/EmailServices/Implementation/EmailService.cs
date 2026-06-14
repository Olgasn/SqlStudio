using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SlqStudio.Application.Services.EmailServices.Models;

namespace SlqStudio.Application.Services.EmailServices.Implementation;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string body)
    {
        try
        {
            var fromAddress = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName);
            var toAddress = new MailAddress(recipientEmail);

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            using var smtp = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.SenderEmail, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            await smtp.SendMailAsync(message);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}