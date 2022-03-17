using MailKit.Net.Smtp;
using MailKit.Security;
using MassTransit;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Events;
using NotificationService.Models;

namespace NotificationService.Services;
public class MailService : IMailService
{
    private readonly IBus _bus;
    private readonly MailSettings _mailSettings;
    public MailService(IBus bus, IOptions<MailSettings> mailSettings)
    {
        _bus = bus; 
        _mailSettings = mailSettings.Value;
    }
    public async Task SendEmailAsync(MailRequest mailRequest, byte[] fileBytes, int ticketId)
    {
        System.Net.Mail.MailAddress.TryCreate(mailRequest.ToEmail, out System.Net.Mail.MailAddress? result); 
        if(result == null)
        {
            await _bus.Publish(new EmailFailedEvent
            {
                TicketId = ticketId,
            });
            Console.WriteLine("Email is incorrect. Sent out EmailFailedEvent"); 
            return; 
        }

        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
        email.Subject = mailRequest.Subject;
        var builder = new BodyBuilder();

        builder.Attachments.Add("KinoTicket.pdf", fileBytes, ContentType.Parse("application/pdf"));

        builder.HtmlBody = mailRequest.Body;
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }
    public async Task SendEmailAsync(MailRequest mailRequest)
    {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
        email.Subject = mailRequest.Subject;
        var builder = new BodyBuilder();
        if (mailRequest.Attachments != null)
        {
            byte[] fileBytes;
            foreach (var file in mailRequest.Attachments)
            {
                if (file.Length > 0)
                {

                    Console.WriteLine("in if");
                    using (var ms = new MemoryStream())
                    {

                        Console.WriteLine("in using");
                        file.CopyTo(ms);
                        Console.WriteLine("after file.copy");
                        fileBytes = ms.ToArray();
                        Console.WriteLine("after ToArray");
                    }

                    builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    Console.WriteLine("after builder.Atachments.add");
                    Console.WriteLine(file.FileName + " ," + fileBytes + ", " + file.ContentType);
                }
            }
        }
        builder.HtmlBody = mailRequest.Body;
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
        Console.WriteLine("End Of FIle reached!");
    }
}
