using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Models;
using PDFService.Events;

namespace NotificationService.Services;
//violates SRP (Creating Email, connecting via smpt, converting files...., should also be validating)
public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;
    public MailService(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }
    //method sendemailasync with MailRequest
    public async Task SendEmailAsync(MailRequest mailRequest)
    {
        Console.WriteLine("MailService::SendEmailAsync(MailRequest mailRequest)");
         
        if (mailRequest.Attachments != null)
        {
            byte[] fileBytes;
            foreach (var file in mailRequest.Attachments)
            {
                if (file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {

                        file.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }

                    builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    Console.WriteLine(file.FileName + " ," + fileBytes + ", " + file.ContentType);
                }
            }
        }
    }

    public async Task SendEmailAsync(PDFCreatedEvent pdfCreatedEvent) {
        Console.WriteLine("EmailService::SendEmailAsync(PDFCreatedEvent pdfCreatedEvent)");
        var email = BuildEmail(pdfCreatedEvent.Email, "MovieTicket", "Thank you for choosing our Movies", await pdfCreatedEvent.Document.Value,"application/pdf", "Kinoticket");
        Send(await email); 
    }
    private async Task<MimeMessage> BuildEmail(string toEmail, string subject, string body, byte[] attachments, string contentType, string fileName) {
        Console.WriteLine("MailService::BuildEmail");

        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder();
        builder.Attachments.Add("KinoTicket", attachments, ContentType.Parse("application/pdf"));

        builder.HtmlBody = body;
        email.Body = builder.ToMessageBody();

        return email; 
    }
    private async Task Send(MimeMessage email)
    {
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }
    /*public async Task SendEmailAsync(MailRequest mailRequest, byte[] fileBytes)
    {
        Console.WriteLine("SendEmailAsync scuffed for test");
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
        email.Subject = mailRequest.Subject;
        var builder = new BodyBuilder();
        
        Console.WriteLine("try attachment added");
        builder.Attachments.Add("Kinoticket", fileBytes, ContentType.Parse("application/pdf"));

        Console.WriteLine("attachment added");
        builder.HtmlBody = mailRequest.Body;
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
        Console.WriteLine("End Of FIle reached!");
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
    */
}
