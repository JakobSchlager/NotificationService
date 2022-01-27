using MailKit;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Controllers;
[Route("api/[controller]")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly Services.IMailService mailService;
    public EmailController(Services.IMailService mailService)
    {
        this.mailService = mailService;
    }
    [HttpPost("send")]
    public async Task<IActionResult> SendMail([FromForm] MailRequest request)
    {
        try
        {
            await mailService.SendEmailAsync(request);
            return Ok();
        }
        catch (Exception ex)
        {
            throw;
        }

    }

    [HttpPost("test")]
    public async Task<IActionResult> TestMail([FromForm] MailRequest request)
    {
        var attachments = new List<IFormFile>();

        using var stream = System.IO.File.OpenRead("Testrun.pdf");
        attachments.Add(new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf",
        });

        request.Attachments = attachments;
        
        await mailService.SendEmailAsync(request);

        return Ok(); 
    }
}
