using NotificationService.Models;

namespace NotificationService.Services;
public interface IMailService
{
    Task SendEmailAsync(MailRequest mailRequest);
    Task SendEmailAsync(MailRequest mailRequest, byte[] fileB, int ticketId);
}
