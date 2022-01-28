using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Models;

namespace NotificationService.Services;
public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;
    public MailService(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public async Task SendEmailAsync(MailRequest mailRequest)
    {
        Console.WriteLine("MailService::SendEmailAsync");
        Console.WriteLine("Mailrequest.ToEmail:" + mailRequest.ToEmail);
        Console.WriteLine("Mailrequest.Subject:" + mailRequest.Subject);
        Console.WriteLine("Mailrequest.Body:" + mailRequest.Body);
        Console.WriteLine("Mailrequest.Attachments:" + mailRequest.Attachments);
        var email = new MimeMessage();
        Console.WriteLine("MailService::MimeMessage()");
        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        Console.WriteLine("email.Sender = MailboxAddress.Parse(_mailSettings.Mail)");
        email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
        Console.WriteLine("email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail))");
        email.Subject = mailRequest.Subject;
        Console.WriteLine("email.Subject = mailRequest.Subject;");
        var builder = new BodyBuilder();
        Console.WriteLine("var builder = new Builder()");
        /*if (mailRequest.Attachments != null)
        {
            byte[] fileBytes;
            foreach (var file in mailRequest.Attachments)
            {
                Console.WriteLine("in foreach loop above if");
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
                        Console.WriteLine(file.FileName + " ," + fileBytes + ", " +file.ContentType);
                }
            }
        }*/
        Console.WriteLine("mailrequest.Attachments if done");
        builder.HtmlBody = mailRequest.Body;
        Console.WriteLine("builder.HtmlBody = mailRequest.Body;");
        email.Body = builder.ToMessageBody();
        Console.WriteLine("email.Body = builder.ToMessageBody();");
        using var smtp = new SmtpClient();
        Console.WriteLine("using var smtp = new SmtpClient();");
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        Console.WriteLine("smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);");
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        Console.WriteLine("smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);");
        await smtp.SendAsync(email);
        Console.WriteLine("await smtp.SendAsync(email);");
        smtp.Disconnect(true);
        Console.WriteLine("smtp.Disconnect(true);");
        Console.WriteLine("End Of FIle reached!");
    }
}
