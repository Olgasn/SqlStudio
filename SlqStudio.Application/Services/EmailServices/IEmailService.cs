namespace SlqStudio.Application.Services.EmailServices;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string recipientEmail, string subject, string body);
}