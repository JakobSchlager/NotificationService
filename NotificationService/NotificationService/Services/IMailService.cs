using NotificationService.Models;

namespace NotificationService.Services;
public interface IMailService
{
    Task SendEmailAsync(MailRequest mailRequest);
}
