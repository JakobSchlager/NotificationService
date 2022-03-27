using MassTransit;
using NotificationService.Events;
using NotificationService.Models;
using NotificationService.Services;

namespace PDFService.Events;
public class PDFCreatedEvent
{
    public int TicketId { get; set; }
    public string Email { get; set; }
    public MessageData<byte[]> Document { get; set; }
}
public class PDFCreatedEventConsumer : IConsumer<PDFCreatedEvent>
{
    private readonly IBus _bus;
    private readonly IMailService _mailService;
    public PDFCreatedEventConsumer(IBus bus, IMailService mailService)
    {
        this._bus = bus;
        this._mailService = mailService;
    }

    public async Task Consume(ConsumeContext<PDFCreatedEvent> pdfCreatedEvent)
    {
        Console.WriteLine("PDFCreatedEventConsumer::Consume");

        //TODO: Change to List<byte[]> so users can reserver more than one Ticket
        var attachments = CreateAttachments(await pdfCreatedEvent.Message.Document.Value);
        var request = new MailRequest
        {
            ToEmail = pdfCreatedEvent.Message.Email,
            Body = "Thank you for choosing our movies!",
            Subject = "Movies Ticket",
            Attachments = attachments,
        };

        var emailSucceeded = await _mailService.SendEmailAsync(request);

        if (!emailSucceeded)
        {
            await _bus.Publish(new EmailFailedEvent
            {
                TicketId = pdfCreatedEvent.Message.TicketId,
            });
            Console.WriteLine($"MailService: Sent {nameof(EmailFailedEvent)}");
        }
    }

    private List<IFormFile> CreateAttachments(byte[] byteArr)
    {
        var attachments = new List<IFormFile>();

        var ms = new MemoryStream(byteArr);

        attachments.Add(new FormFile(ms, 0, ms.Length, "Kinoticket", "Kinoticket.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf",
        });

        return attachments;
    }
}
