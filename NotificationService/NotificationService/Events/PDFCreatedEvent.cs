using MassTransit;
using NotificationService.Models;
using NotificationService.Services;

namespace PDFService.Events;
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
        var request = new MailRequest
        {
            ToEmail = "jakob.sschlager@gmail.com",
            Body = "Testing",
            Subject = "Test",
        };

        var attachments = new List<IFormFile>();

        using var stream = System.IO.File.OpenRead("Testrun.pdf");
        attachments.Add(new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf",
        });

        request.Attachments = attachments;

        await _mailService.SendEmailAsync(request);
    }
}
