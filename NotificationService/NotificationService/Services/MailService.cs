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
    private readonly MailSettings _mailSettings;
    public MailService(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public async Task<bool> SendEmailAsync(MailRequest mailRequest)
    {
        if (!IsEmailValid(mailRequest.ToEmail))
        {
            Console.WriteLine($"MailService: Invalid Email: {mailRequest.ToEmail}");
            return false;
        }

        var messageBody = CreateMessageBody(mailRequest);
        var email = CreateEmail(mailRequest.ToEmail, mailRequest.Subject, messageBody); 
        await SendEmailAsync(email);

        return true; 
    }

    private bool IsEmailValid(string email)
    {
        return System.Net.Mail.MailAddress.TryCreate(email, out _);
    }

    private MimeEntity CreateMessageBody(MailRequest mailRequest)
    {
        var builder = new BodyBuilder();
        builder.HtmlBody = mailRequest.Body;

        if (mailRequest.Attachments == null) return builder.ToMessageBody();

        foreach (var file in mailRequest.Attachments)
        {
            if (file.Length <= 0) break; 

            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                var fileBytes = ms.ToArray();
                builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
            }
        }

        return builder.ToMessageBody();
    }

    private MimeMessage CreateEmail(string toEmail, string subject, MimeEntity? messageBody)
    {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject; 
        email.Body = messageBody;

        return email; 
    }

    private async Task SendEmailAsync(MimeMessage email)
    {
        var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }
}
