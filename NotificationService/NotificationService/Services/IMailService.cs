using NotificationService.Models;

namespace NotificationService.Services;
public interface IMailService
{
    Task<bool> SendEmailAsync(MailRequest mailRequest);
}
