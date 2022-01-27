using MassTransit;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Events;
public class PDFCreatedEvent
{
    public int TicketId { get; set; }
    public string Email { get; set; }
}

public class PDFCreatedEventConsumer : IConsumer<PDFCreatedEvent>
{
    private readonly IMailService _mailService; 
    public PDFCreatedEventConsumer(IMailService mailService)
    {
        this._mailService = mailService; 
    }

    public async Task Consume(ConsumeContext<PDFCreatedEvent> context)
    {
        Console.WriteLine("PDFCreatedConsumer::Consume: " + context.Message.TicketId + ", Email:" + context.Message.Email);
        Console.WriteLine("MailService:" + _mailService); 

        var attachments = new List<IFormFile>();  

        using (var stream = File.OpenRead("Testrun.pdf"))
        {
            attachments.Add(new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf", 
            });
        }

        Console.WriteLine("MailService: attachmentCount" + attachments.Count);

        _mailService.SendEmailAsync(new MailRequest {
            ToEmail = "jakobschlager.biz@gmail.com",
            Subject = $"Kinoticket: {context.Message.TicketId}",
            Body = "Vielen Dank, dass Sie unser Kino gewählt haben!",
            Attachments = attachments,  
        });
    }
}
