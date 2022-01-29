using MassTransit;
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
    private readonly IMailService _mailService; 
    public PDFCreatedEventConsumer(IMailService mailService)
    {
        this._mailService = mailService; 
    }

    public async Task Consume(ConsumeContext<PDFCreatedEvent> context)
    {
        Console.WriteLine("PDFCreatedEventConsumer::Consume");
        Console.WriteLine("PDFCreatedEventConsumer::Consume" + context.Message.Email);
        var request = new MailRequest
        {
            ToEmail = context.Message.Email, 
            Body = "Thank you for choosing our movies!",
            Subject = "Movies Ticket",
        };

        var attachments = new List<IFormFile>();

        byte[] byteArr = await context.Message.Document.Value;
        Console.WriteLine("byteArr:" + byteArr.ToString());
        using (var stream = new MemoryStream(byteArr))
        {
            //using var stream = System.IO.File.OpenRead("Testrun.pdf");
            attachments.Add(new FormFile(stream, 0, stream.Length, null, "Kinoticket")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf",
            });
        }
        Console.WriteLine(attachments.Count);
        request.Attachments = attachments;

        await _mailService.SendEmailAsync(request);
    }
}
